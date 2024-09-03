using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class botController : MonoBehaviour
{
    public gridCreator gridScript;
    public Node[,] grid;
    public GameObject parentObject;
    public GameObject botPrefab;
    public GameObject botTrailPrefab;
    public GameObject explosionPrefab;
    public Sprite botUp;
    public Sprite botDown;
    public Sprite botLeft;
    public Sprite botRight;
    public AudioSource dieSFX;
    public enum Directions {Up, Down, Left, Right};
    public botBike bike;
    public int spawnPosX;
    public int spawnPosY;
    public int fuelRemaining = 100;
    public int k;
    public float botSpeed;
    public bool isAlive = false;
    public bool isInmune = false;
    public Directions direction;
    public botQueue queue;
    public botStack stack;

    void Start()
    {
        botSpeed = UnityEngine.Random.Range(0.05f, 0.1f);
        gridScript = FindObjectOfType<gridCreator>();
        queue = GetComponent<botQueue>();
        stack = GetComponent<botStack>();
        
        Invoke("instantiateHead", 0.01f);
    }

    public void instantiateHead () {
        grid = gridScript.grid;
        Node spawnPos = grid[spawnPosX, spawnPosY];

        GameObject bot = Instantiate(botPrefab);
        bot.transform.SetParent(parentObject.transform);
        bot.transform.localPosition = spawnPos.pos;

        bike = new botBike(this, spawnPos, bot, Directions.Right);
        bike.addTrail(5, botTrailPrefab, parentObject);
    }

    public IEnumerator updateBot () {
        yield return new WaitForSeconds(botSpeed);
        this.k = 0;


        while(isAlive) {
            yield return new WaitForSeconds(botSpeed);

            if (fuelRemaining > 0) {
                if (k == 5) {
                    --fuelRemaining;
                    k = 0;
                }
                decideDirection();
                bike.updateDirection(direction);
                bike.updateChain();
                bike.headMovement();
                k++;
            } else {
                StartCoroutine(dieEvent());
            }
        }
    }

    public void decideDirection () {
        int randomNum = UnityEngine.Random.Range(0, 15);
        List<Directions> possibleMovements = new List<Directions>();
        if (randomNum == 5) {
            if (bike.head.thisNode.Up != null && bike.head.nodeDirectionNext != Directions.Down) possibleMovements.Add(Directions.Up);
            if (bike.head.thisNode.Down != null && bike.head.nodeDirectionNext != Directions.Up) possibleMovements.Add(Directions.Down);
            if (bike.head.thisNode.Left != null && bike.head.nodeDirectionNext != Directions.Right) possibleMovements.Add(Directions.Left);
            if (bike.head.thisNode.Right != null && bike.head.nodeDirectionNext != Directions.Left) possibleMovements.Add(Directions.Right);
            int randomIndex = UnityEngine.Random.Range(0, possibleMovements.Count);
            direction = possibleMovements[randomIndex];
        } else {
            if (bike.head.thisNode.Up == null && bike.head.nodeDirectionNext == Directions.Up) {
                if (bike.head.thisNode.Left != null && bike.head.nodeDirectionNext != Directions.Right) possibleMovements.Add(Directions.Left);
                if (bike.head.thisNode.Right != null && bike.head.nodeDirectionNext != Directions.Left) possibleMovements.Add(Directions.Right);
                int randomIndex = UnityEngine.Random.Range(0, possibleMovements.Count);
                direction = possibleMovements[randomIndex];
            }
            if (bike.head.thisNode.Down == null && bike.head.nodeDirectionNext == Directions.Down) {
                if (bike.head.thisNode.Left != null && bike.head.nodeDirectionNext != Directions.Right) possibleMovements.Add(Directions.Left);
                if (bike.head.thisNode.Right != null && bike.head.nodeDirectionNext != Directions.Left) possibleMovements.Add(Directions.Right);
                int randomIndex = UnityEngine.Random.Range(0, possibleMovements.Count);
                direction = possibleMovements[randomIndex];
            }
            if (bike.head.thisNode.Right == null && bike.head.nodeDirectionNext == Directions.Right) {
                if (bike.head.thisNode.Up != null && bike.head.nodeDirectionNext != Directions.Down) possibleMovements.Add(Directions.Up);
                if (bike.head.thisNode.Down != null && bike.head.nodeDirectionNext != Directions.Up) possibleMovements.Add(Directions.Down);
                int randomIndex = UnityEngine.Random.Range(0, possibleMovements.Count);
                direction = possibleMovements[randomIndex];
            }
            if (bike.head.thisNode.Left == null && bike.head.nodeDirectionNext == Directions.Left) {
                if (bike.head.thisNode.Up != null && bike.head.nodeDirectionNext != Directions.Down) possibleMovements.Add(Directions.Up);
                if (bike.head.thisNode.Down != null && bike.head.nodeDirectionNext != Directions.Up) possibleMovements.Add(Directions.Down);
                int randomIndex = UnityEngine.Random.Range(0, possibleMovements.Count);
                direction = possibleMovements[randomIndex];
            }
        }

    }
    public GameObject instantiateTrail(Tuple< Vector2, Node, Directions > result) {
            GameObject child = Instantiate(botTrailPrefab);
            child.transform.SetParent(parentObject.transform);
            child.transform.SetSiblingIndex(0);
            child.transform.localPosition = result.Item1;
            child.name = $"Trail{++bike.size}";
            return child;
    }

    public IEnumerator dieEvent() {
        dieSFX.Play();
        GameObject explosion = Instantiate(explosionPrefab);
        explosion.transform.SetParent(parentObject.transform);
        explosion.transform.localPosition = bike.head.thisNode.pos;
        isAlive = false;

        botBikeNode marker = bike.head;

        while (marker != null) {
            marker.thisNode.state = Node.states.unoccupied;
            Destroy(marker.identifier);
            marker = marker.nextNode;
            yield return new WaitForSeconds(0.015f);
        }

        botStackNode stackMarker = stack.stack.top;
        GameObject prefab;

        while (stackMarker != null) {
            if (stackMarker.powerType == Node.states.powerHyperVelocity) {
                prefab = stack.hyperVelocityPrefab;
            } else {
                prefab = stack.shieldPrefab;
            }
            StartCoroutine(stack.generatePower(stackMarker.powerType, prefab));
            stackMarker = stackMarker.nextNode;
            stack.stack.pop();
        }

        botQueueNode queueMarker = queue.queue.front;

        while (queueMarker != null) {
            if (queueMarker.itemType == Node.states.itemTrail) {
                prefab = queue.trailItemPrefab;
            } else if (queueMarker.itemType == Node.states.itemFuel) {
                prefab = queue.gascanItemPrefab;
            } else {
                prefab = queue.bombItemPrefab;
            }

            queue.generateItem(queueMarker.itemType, prefab);
            queueMarker = queueMarker.nextNode;
            queue.queue.dequeue();
        }
        yield return new WaitForSeconds(2);
        Destroy(explosion);

 }

public class botBikeNode {
    public Node thisNode;
    public botBikeNode nextNode;
    public botController.Directions nodeDirectionNext;
    public botController.Directions nodeDirectionPrev;
    public GameObject identifier;

    public botBikeNode (Node thisNode, GameObject identifier, botController.Directions nodeDirection) {
        this.thisNode = thisNode;
        nextNode = null;
        this.identifier = identifier;
        this.nodeDirectionNext = nodeDirection;
    }
}

public class botBike {
    public botController instance;
    public botBikeNode head;
    public botQueue queue;
    public int size;

    public botBike(botController instance, Node spawnPos, GameObject identifier, botController.Directions direction) {
        this.instance = instance;
        this.head = new botBikeNode(spawnPos, identifier, direction);
        this.queue = instance.queue;
        ++size;
    }

        public void updateDirection (botController.Directions direction) {
        this.head.nodeDirectionPrev = this.head.nodeDirectionNext;
        this.head.nodeDirectionNext = direction;
    }

    public void headMovement () {
        switch (this.head.nodeDirectionNext) {
            case botController.Directions.Up:
                    if (head.thisNode.Up != null && head.thisNode.Up.state != Node.states.trail && head.thisNode.Up.state != Node.states.head) {
                        if (head.thisNode.Up.state == Node.states.unoccupied) {
                            this.head.thisNode.state = Node.states.unoccupied;
                            this.head.thisNode = head.thisNode.Up;
                            this.head.thisNode.state = Node.states.head;
                            head.identifier.transform.localPosition = this.head.thisNode.pos;
                            UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                            imageComponet.sprite = instance.botUp;
                        } else {
                            pickupPowerAux(head.thisNode.Up.state, head.thisNode.Up);
                            this.head.thisNode.state = Node.states.unoccupied;
                            this.head.thisNode = head.thisNode.Up;
                            this.head.thisNode.state = Node.states.head;
                            head.identifier.transform.localPosition = this.head.thisNode.pos;
                            UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                            imageComponet.sprite = instance.botUp;
                        }
                    } else {
                        if (!instance.isInmune) {
                            instance.StartCoroutine(instance.dieEvent());
                        } else {
                            if (head.thisNode.Up != null) {
                                this.head.thisNode.state = Node.states.unoccupied;
                                this.head.thisNode = head.thisNode.Up;
                                this.head.thisNode.state = Node.states.head;
                                head.identifier.transform.localPosition = this.head.thisNode.pos;
                                UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                                imageComponet.sprite = instance.botUp;
                            }
                        }
                    }
                break;
                
            case botController.Directions.Down:
                    if (head.thisNode.Down != null && head.thisNode.Down.state != Node.states.trail && head.thisNode.Down.state != Node.states.head) {
                        if (head.thisNode.Down.state == Node.states.unoccupied) {
                            this.head.thisNode.state = Node.states.unoccupied;
                            this.head.thisNode = head.thisNode.Down;
                            this.head.thisNode.state = Node.states.head;
                            head.identifier.transform.localPosition = this.head.thisNode.pos;
                            UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                            imageComponet.sprite = instance.botDown;
                        } else {
                            pickupPowerAux(head.thisNode.Down.state, head.thisNode.Down);
                            this.head.thisNode.state = Node.states.unoccupied;
                            this.head.thisNode = head.thisNode.Down;
                            this.head.thisNode.state = Node.states.head;
                            head.identifier.transform.localPosition = this.head.thisNode.pos;
                            UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                            imageComponet.sprite = instance.botDown;
                        }
                    } else {
                        if (!instance.isInmune) {
                            instance.StartCoroutine(instance.dieEvent());
                        } else {
                            if (head.thisNode.Up != null) {
                                this.head.thisNode.state = Node.states.unoccupied;
                                this.head.thisNode = head.thisNode.Down;
                                this.head.thisNode.state = Node.states.head;
                                head.identifier.transform.localPosition = this.head.thisNode.pos;
                                UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                                imageComponet.sprite = instance.botDown;
                            }
                        }
                    }
                break;
            
            case botController.Directions.Left:
                    if (head.thisNode.Left != null && head.thisNode.Left.state != Node.states.trail && head.thisNode.Left.state != Node.states.head) {
                        if (head.thisNode.Left.state == Node.states.unoccupied) {
                            this.head.thisNode.state = Node.states.unoccupied;
                            this.head.thisNode = head.thisNode.Left;
                            this.head.thisNode.state = Node.states.head;
                            head.identifier.transform.localPosition = this.head.thisNode.pos;
                            UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                            imageComponet.sprite = instance.botLeft;
                        } else {
                            pickupPowerAux(head.thisNode.Left.state, head.thisNode.Left);
                            this.head.thisNode.state = Node.states.unoccupied;
                            this.head.thisNode = head.thisNode.Left;
                            this.head.thisNode.state = Node.states.head;
                            head.identifier.transform.localPosition = this.head.thisNode.pos;
                            UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                            imageComponet.sprite = instance.botLeft;
                        }
                    } else {
                        if (!instance.isInmune) {
                            instance.StartCoroutine(instance.dieEvent());
                        } else {
                            if (head.thisNode.Up != null) {
                                this.head.thisNode.state = Node.states.unoccupied;
                                this.head.thisNode = head.thisNode.Left;
                                this.head.thisNode.state = Node.states.head;
                                head.identifier.transform.localPosition = this.head.thisNode.pos;
                                UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                                imageComponet.sprite = instance.botLeft;
                            }
                        }
                    }
                break;

            case botController.Directions.Right:
                    if (head.thisNode.Right != null && head.thisNode.Right.state != Node.states.trail && head.thisNode.Right.state != Node.states.head) {
                        if (head.thisNode.Right.state == Node.states.unoccupied) {
                            this.head.thisNode.state = Node.states.unoccupied;
                            this.head.thisNode = head.thisNode.Right;
                            this.head.thisNode.state = Node.states.head;
                            head.identifier.transform.localPosition = this.head.thisNode.pos;
                            UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                            imageComponet.sprite = instance.botRight;
                        } else {
                            pickupPowerAux(head.thisNode.Right.state, head.thisNode.Right);
                            this.head.thisNode.state = Node.states.unoccupied;
                            this.head.thisNode = head.thisNode.Right;
                            this.head.thisNode.state = Node.states.head;
                            head.identifier.transform.localPosition = this.head.thisNode.pos;
                            UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                            imageComponet.sprite = instance.botRight;
                        }
                    } else {
                        if (!instance.isInmune) {
                            instance.StartCoroutine(instance.dieEvent());
                        } else {
                            if (head.thisNode.Up != null) {
                                this.head.thisNode.state = Node.states.unoccupied;
                                this.head.thisNode = head.thisNode.Right;
                                this.head.thisNode.state = Node.states.head;
                                head.identifier.transform.localPosition = this.head.thisNode.pos;
                                UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                                imageComponet.sprite = instance.botRight;
                            }
                        }
                    }
                break;
        }
    }

    public void pickupPowerAux(Node.states state, Node node) {
        switch (state) {
            case Node.states.powerHyperVelocity:
                queue.pickUpItem(Node.states.powerHyperVelocity, node);
                break;
            case Node.states.powerShield:
                queue.pickUpItem(Node.states.powerShield, node);
                break;
            case Node.states.itemFuel:
                queue.pickUpItem(state, node);
                break;
            case Node.states.itemBomb:
                queue.pickUpItem(state, node);
                break;
            case Node.states.itemTrail:
                queue.pickUpItem(state, node);
                break;
        }
    } 

    public void addTrail(int quantity, GameObject trailsPrefab, GameObject parentCavas) {
        for (int i = 0; i < quantity; i++) {
            if (head.nextNode == null) {

                var result = handleNode(head.nodeDirectionNext, head);

                GameObject child = instance.instantiateTrail(result);

                botBikeNode newTrail;
                newTrail = new botBikeNode(result.Item2, child, result.Item3);
                head.nextNode = newTrail;

            } else {

                botBikeNode index = head; 

                while (index.nextNode != null) {
                    index = index.nextNode;
                }

                var result = handleNode(index.nodeDirectionNext, index);

                GameObject child = instance.instantiateTrail(result);

                botBikeNode newTrail;
                newTrail = new botBikeNode(result.Item2, child, result.Item3);
                index.nextNode = newTrail;
            }
        }
    }
    public Tuple<Vector2, Node, botController.Directions> handleNode(botController.Directions nodeDirection, botBikeNode previousObjectNode) {
        
        Vector2 spawnPos;
        Node node;
        botController.Directions objectDirection;

        switch (nodeDirection) {
            case botController.Directions.Up:
                spawnPos = previousObjectNode.thisNode.Down.pos;
                node = previousObjectNode.thisNode.Down;
                objectDirection = botController.Directions.Down;
                break;
            case botController.Directions.Down:
                spawnPos = previousObjectNode.thisNode.Up.pos;
                node = previousObjectNode.thisNode.Up;
                objectDirection = botController.Directions.Up;
                break;
            case botController.Directions.Right:
                spawnPos = previousObjectNode.thisNode.Left.pos;
                node = previousObjectNode.thisNode.Left;
                objectDirection = botController.Directions.Left;
                break;
            case botController.Directions.Left:
                spawnPos = previousObjectNode.thisNode.Right.pos;
                node = previousObjectNode.thisNode.Right;
                objectDirection = botController.Directions.Right;
                break;
            default:
                spawnPos = previousObjectNode.thisNode.Right.pos;
                node = previousObjectNode.thisNode.Right;
                objectDirection = botController.Directions.Right;
                break;
        }
        return new Tuple<Vector2, Node, botController.Directions>(spawnPos, node, objectDirection);
    }

    public void updateChain() {
        bool updateable;

        switch (this.head.nodeDirectionNext) {
            case botController.Directions.Up:
                if (head.thisNode.Up == null) {
                    updateable = false;
                } else {
                    updateable = true;
                }
                break;
            case botController.Directions.Down:
                if (head.thisNode.Down == null) {
                    updateable = false;
                } else {
                    updateable = true;
                }
                break;
            case botController.Directions.Right:
                if (head.thisNode.Right == null) {
                    updateable = false;
                } else {
                    updateable = true;
                }
                break;
            case botController.Directions.Left:
                if (head.thisNode.Left == null) {
                    updateable = false;
                } else {
                    updateable = true;
                }
                break;
            default:
                updateable = false;
                break;
        }

        if (updateable) {
                if(head.nextNode != null) {
                botBikeNode index = head.nextNode;
                botController.Directions previousDirection = head.nodeDirectionPrev;
                botController.Directions nextDirection;
                while (index != null) {
                    nextDirection = previousDirection;
                    previousDirection = index.nodeDirectionNext;
                    
                    index.nodeDirectionPrev = previousDirection;
                    index.nodeDirectionNext = nextDirection;

                    if (nextDirection == botController.Directions.Up) {
                        index.identifier.transform.localPosition = index.thisNode.Up.pos;
                        index.thisNode = index.thisNode.Up;
                        index.thisNode.state = Node.states.trail;
                    }
                    if (nextDirection == botController.Directions.Down) {
                        index.identifier.transform.localPosition = index.thisNode.Down.pos;
                        index.thisNode = index.thisNode.Down;
                        index.thisNode.state = Node.states.trail;
                    }
                    if (nextDirection == botController.Directions.Right) {
                        index.identifier.transform.localPosition = index.thisNode.Right.pos;
                        index.thisNode = index.thisNode.Right;
                        index.thisNode.state = Node.states.trail;
                    }                
                    if (nextDirection == botController.Directions.Left) {
                        index.identifier.transform.localPosition = index.thisNode.Left.pos;
                        index.thisNode = index.thisNode.Left;
                        index.thisNode.state = Node.states.trail;
                    }                   

                    if (index.nextNode == null) {
                        var result = handleNode(index.nodeDirectionNext, index);
                        result.Item2.state = Node.states.unoccupied;
                    }

                    index = index.nextNode;
                }
            } 
        }
    }
    }
}