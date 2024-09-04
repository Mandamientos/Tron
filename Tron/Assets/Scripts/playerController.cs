using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;


public class playerController : MonoBehaviour
{
    public gridCreator gridScript;
    public RectTransform player;
    public Node playerNode;
    public float speed;
    public Directions direction = Directions.Down;
    public enum Directions { Up, Down, Left, Right }
    public motorBike bike;
    public GameObject trailsPrefab;
    public GameObject playerPrefab;
    public GameObject explosionPrefab;
    public GameObject parentCavas;
    public Sprite riderUp;
    public Sprite riderDown;
    public Sprite riderLeft;
    public Sprite riderRight;
    public TMP_Text fuelRemainingText;
    public int fuelRemaining = 999;
    public powerHandler powerHandler;
    public objectsHandler objectsHandler;
    public bool inmune = false;
    public bool isAlive = false;
    public int k;
    public AudioSource bgMusic;
    public AudioSource dieSFX;
    public Animator dieAnim1;
    public Animator dieAnim2;

    void Start()
    {
        speed = UnityEngine.Random.Range(0.05f, 0.10f);
        direction = Directions.Down;

        objectsHandler = FindObjectOfType<objectsHandler>();

        Invoke("setGrid", 0.01f);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.UpArrow) && direction != Directions.Down && bike.head.thisNode.Up != null) direction = Directions.Up;
        if (Input.GetKeyDown(KeyCode.DownArrow) && direction != Directions.Up && bike.head.thisNode.Down != null) direction = Directions.Down;
        if (Input.GetKeyDown(KeyCode.RightArrow) && direction != Directions.Left && bike.head.thisNode.Right != null) direction = Directions.Right;
        if (Input.GetKeyDown(KeyCode.LeftArrow) && direction != Directions.Right && bike.head.thisNode.Left != null) direction = Directions.Left;
    }

    public IEnumerator playerMovement() {

        yield return new WaitForSeconds(speed);

        this.k = 0;
        
        while (isAlive) {
            yield return new WaitForSeconds(speed);

            if (fuelRemaining > 0) {
                if (k == 5) {
                    k = 0;
                    fuelRemainingText.text = $"{--fuelRemaining}/100";
                    bike.updateDirection(direction);
                    bike.updateChain();
                    bike.headMovement();
                    continue;
                }

                bike.updateDirection(direction);
                bike.updateChain();
                bike.headMovement();
                ++k;
            } else {
                StartCoroutine(dieEvent());
            }
        }


    }

    public IEnumerator dieEvent() {
        dieSFX.Play();
        GameObject explosion = Instantiate(explosionPrefab);
        explosion.transform.SetParent(parentCavas.transform);
        explosion.transform.localPosition = bike.head.thisNode.pos;
        isAlive = false;

        motorNode marker = bike.head;

        while (marker != null) {
            marker.thisNode.state = Node.states.unoccupied;
            Destroy(marker.identifier);
            marker = marker.nextNode;
            yield return new WaitForSeconds(0.015f);
        }

        stackNode stackMarker = powerHandler.stack.top;
        GameObject prefab;

        while (stackMarker != null) {
            if (stackMarker.powerType == Node.states.powerHyperVelocity) {
                prefab = powerHandler.hyperVelocityPrefab;
            } else {
                prefab = powerHandler.shieldPrefab;
            }
            StartCoroutine(powerHandler.generatePower(stackMarker.powerType, prefab));
            stackMarker = stackMarker.nextNode;
            powerHandler.stack.pop();
        }

        queueNode queueMarker = objectsHandler.queue.front;

        while (queueMarker != null) {
            if (queueMarker.itemType == Node.states.itemTrail) {
                prefab = objectsHandler.trailItemPrefab;
            } else if (queueMarker.itemType == Node.states.itemFuel) {
                prefab = objectsHandler.gascanItemPrefab;
            } else {
                prefab = objectsHandler.bombItemPrefab;
            }

            objectsHandler.generateItem(queueMarker.itemType, prefab);
            queueMarker = queueMarker.nextNode;
            objectsHandler.queue.dequeue();
            objectsHandler.updateQueue();
        }

        

        while (bgMusic.pitch >= 0) {
            bgMusic.pitch -= 0.5f * Time.deltaTime;
            Debug.Log(bgMusic.pitch);
            yield return null;
        }

        dieAnim1.SetTrigger("Start");
        dieAnim2.SetTrigger("Start");
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(0);

        bgMusic.Stop();

    }

    void setGrid () {
        gridScript = FindObjectOfType<gridCreator>();
        powerHandler = FindObjectOfType<powerHandler>();

        Node[,] grid = gridScript.grid;

        int xPos = UnityEngine.Random.Range(0, 50);
        int yPos = UnityEngine.Random.Range(0, 100);

        playerNode = grid[25, 50];

        GameObject childObject = Instantiate(playerPrefab);
        childObject.transform.SetParent(parentCavas.transform, false);
        childObject.transform.localPosition = playerNode.pos;

        childObject.name = "Player";
        
        bike = new motorBike(playerNode, childObject, direction, powerHandler, this, objectsHandler);
        bike.addTrail(3, trailsPrefab, parentCavas);

    }

    public GameObject instantiateTrail(Tuple< Vector2, Node, Directions > result) {
            GameObject child = Instantiate(trailsPrefab);
            child.transform.SetParent(parentCavas.transform);
            child.transform.SetSiblingIndex(2);
            child.transform.localPosition = result.Item1;
            child.name = $"Trail{++bike.size}";
            return child;
    }
}

public class motorNode {
    public Node thisNode;
    public motorNode nextNode;
    public playerController.Directions nodeDirectionNext;
    public playerController.Directions nodeDirectionPrev;
    public GameObject identifier;

    public motorNode(Node thisNode, GameObject identifier, playerController.Directions nodeDirection) {
        this.thisNode = thisNode;
        this.nextNode = null;
        this.identifier = identifier;
        this.nodeDirectionNext = nodeDirection;
    }

    public void setNext(motorNode nextNode) {
        this.nextNode = nextNode;
    }
}

public class motorBike {
    public motorNode head;
    public int size;
    private playerController Instance;
    private powerHandler powerHandler;
    private objectsHandler objectsHandler;


    public motorBike(Node playerNode, GameObject identifier, playerController.Directions direction, powerHandler powerHandler, playerController instance, objectsHandler objectsHandler) {
        head = new motorNode(playerNode, identifier, direction);
        ++size;
        this.powerHandler = powerHandler;
        this.objectsHandler = objectsHandler;
        this.Instance = instance;
    }

    public void addTrail(int quantity, GameObject trailsPrefab, GameObject parentCavas) {
        for (int i = 0; i < quantity; i++) {
            if (head.nextNode == null) {

                var result = handleNode(head.nodeDirectionNext, head);

                GameObject child = Instance.instantiateTrail(result);

                motorNode newTrail;
                newTrail = new motorNode(result.Item2, child, result.Item3);
                head.setNext(newTrail);

            } else {

                motorNode index = head; 

                while (index.nextNode != null) {
                    index = index.nextNode;
                }

                var result = handleNode(index.nodeDirectionNext, index);

                GameObject child = Instance.instantiateTrail(result);

                motorNode newTrail;
                newTrail = new motorNode(result.Item2, child, result.Item3);
                index.setNext(newTrail);
            }
        }
    }

    public void updateDirection (playerController.Directions direction) {
        this.head.nodeDirectionPrev = this.head.nodeDirectionNext;
        this.head.nodeDirectionNext = direction;
    }

    public void updateChain() {
        bool updateable;

        switch (this.head.nodeDirectionNext) {
            case playerController.Directions.Up:
                if (head.thisNode.Up == null) {
                    updateable = false;
                } else {
                    updateable = true;
                }
                break;
            case playerController.Directions.Down:
                if (head.thisNode.Down == null) {
                    updateable = false;
                } else {
                    updateable = true;
                }
                break;
            case playerController.Directions.Right:
                if (head.thisNode.Right == null) {
                    updateable = false;
                } else {
                    updateable = true;
                }
                break;
            case playerController.Directions.Left:
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
                motorNode index = head.nextNode;
                playerController.Directions previousDirection = head.nodeDirectionPrev;
                playerController.Directions nextDirection;
                while (index != null) {
                    nextDirection = previousDirection;
                    previousDirection = index.nodeDirectionNext;
                    
                    index.nodeDirectionPrev = previousDirection;
                    index.nodeDirectionNext = nextDirection;

                    if (nextDirection == playerController.Directions.Up) {
                        index.identifier.transform.localPosition = index.thisNode.Up.pos;
                        index.thisNode = index.thisNode.Up;
                        index.thisNode.state = Node.states.trail;
                    }
                    if (nextDirection == playerController.Directions.Down) {
                        index.identifier.transform.localPosition = index.thisNode.Down.pos;
                        index.thisNode = index.thisNode.Down;
                        index.thisNode.state = Node.states.trail;
                    }
                    if (nextDirection == playerController.Directions.Right) {
                        index.identifier.transform.localPosition = index.thisNode.Right.pos;
                        index.thisNode = index.thisNode.Right;
                        index.thisNode.state = Node.states.trail;
                    }                
                    if (nextDirection == playerController.Directions.Left) {
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

    public Tuple<Vector2, Node, playerController.Directions> handleNode(playerController.Directions nodeDirection, motorNode previousObjectNode) {
        
        Vector2 spawnPos;
        Node node;
        playerController.Directions objectDirection;

        switch (nodeDirection) {
            case playerController.Directions.Up:
                spawnPos = previousObjectNode.thisNode.Down.pos;
                node = previousObjectNode.thisNode.Down;
                objectDirection = playerController.Directions.Down;
                break;
            case playerController.Directions.Down:
                spawnPos = previousObjectNode.thisNode.Up.pos;
                node = previousObjectNode.thisNode.Up;
                objectDirection = playerController.Directions.Up;
                break;
            case playerController.Directions.Right:
                spawnPos = previousObjectNode.thisNode.Left.pos;
                node = previousObjectNode.thisNode.Left;
                objectDirection = playerController.Directions.Left;
                break;
            case playerController.Directions.Left:
                spawnPos = previousObjectNode.thisNode.Right.pos;
                node = previousObjectNode.thisNode.Right;
                objectDirection = playerController.Directions.Right;
                break;
            default:
                spawnPos = previousObjectNode.thisNode.Right.pos;
                node = previousObjectNode.thisNode.Right;
                objectDirection = playerController.Directions.Right;
                break;
        }
        return new Tuple<Vector2, Node, playerController.Directions>(spawnPos, node, objectDirection);
    }

    public void headMovement() {
            switch (head.nodeDirectionNext)
            {
                case playerController.Directions.Up:
                    if (head.thisNode.Up != null && head.thisNode.Up.state != Node.states.trail && head.thisNode.Up.state != Node.states.head) {
                        if (head.thisNode.Up.state == Node.states.unoccupied) {
                            this.head.thisNode.state = Node.states.trail;
                            this.head.thisNode = head.thisNode.Up;
                            this.head.thisNode.state = Node.states.head;
                            head.identifier.transform.localPosition = this.head.thisNode.pos;
                            UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                            imageComponet.sprite = Instance.riderUp;
                        } else {
                            pickupPowerAux(head.thisNode.Up.state, head.thisNode.Up);
                            this.head.thisNode.state = Node.states.trail;
                            this.head.thisNode = head.thisNode.Up;
                            this.head.thisNode.state = Node.states.head;
                            head.identifier.transform.localPosition = this.head.thisNode.pos;
                            UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                            imageComponet.sprite = Instance.riderUp;
                        }
                    } else {
                        if (!Instance.inmune) {
                            Instance.StartCoroutine(Instance.dieEvent());
                        } else {
                            if (head.thisNode.Up != null) {
                                this.head.thisNode.state = Node.states.trail;
                                this.head.thisNode = head.thisNode.Up;
                                this.head.thisNode.state = Node.states.head;
                                head.identifier.transform.localPosition = this.head.thisNode.pos;
                                UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                                imageComponet.sprite = Instance.riderUp;
                            }
                        }
                    }
                    break;
                case playerController.Directions.Down:
                    if (head.thisNode.Down != null && head.thisNode.Down.state != Node.states.trail && head.thisNode.Down.state != Node.states.head) {
                        if (head.thisNode.Down.state == Node.states.unoccupied) {
                            this.head.thisNode.state = Node.states.trail;
                            this.head.thisNode = head.thisNode.Down;
                            this.head.thisNode.state = Node.states.head;
                            head.identifier.transform.localPosition = this.head.thisNode.pos;
                            UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                            imageComponet.sprite = Instance.riderDown;
                        } else {
                            pickupPowerAux(head.thisNode.Down.state, head.thisNode.Down);
                            this.head.thisNode.state = Node.states.trail;
                            this.head.thisNode = head.thisNode.Down;
                            this.head.thisNode.state = Node.states.head;
                            head.identifier.transform.localPosition = this.head.thisNode.pos;
                            UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                            imageComponet.sprite = Instance.riderDown;
                        }
                    } else {
                        if (!Instance.inmune) {
                            Instance.StartCoroutine(Instance.dieEvent());
                        } else {
                            if (head.thisNode.Down != null) {
                                this.head.thisNode.state = Node.states.trail;
                                this.head.thisNode = head.thisNode.Down;
                                this.head.thisNode.state = Node.states.head;
                                head.identifier.transform.localPosition = this.head.thisNode.pos;
                                UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                                imageComponet.sprite = Instance.riderDown;
                            }
                        }
                    }
                    break;

                case playerController.Directions.Right:
                    if (head.thisNode.Right != null && head.thisNode.Right.state != Node.states.trail && head.thisNode.Right.state != Node.states.head) {
                        if (head.thisNode.Right.state == Node.states.unoccupied) {
                            this.head.thisNode.state = Node.states.trail;
                            this.head.thisNode = head.thisNode.Right;
                            this.head.thisNode.state = Node.states.head;
                            head.identifier.transform.localPosition = this.head.thisNode.pos;
                            UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                            imageComponet.sprite = Instance.riderRight;
                        } else {
                            pickupPowerAux(head.thisNode.Right.state, head.thisNode.Right);
                            this.head.thisNode.state = Node.states.trail;
                            this.head.thisNode = head.thisNode.Right;
                            this.head.thisNode.state = Node.states.head;
                            head.identifier.transform.localPosition = this.head.thisNode.pos;
                            UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                            imageComponet.sprite = Instance.riderRight;
                        }
                    } else {
                        if (!Instance.inmune) {
                            Instance.StartCoroutine(Instance.dieEvent());
                        } else {
                            if (head.thisNode.Right != null) {
                                this.head.thisNode.state = Node.states.trail;
                                this.head.thisNode = head.thisNode.Right;
                                this.head.thisNode.state = Node.states.head;
                                head.identifier.transform.localPosition = this.head.thisNode.pos;
                                UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                                imageComponet.sprite = Instance.riderDown;
                            }
                        }
                    }
                    break;
                case playerController.Directions.Left:
                    if (head.thisNode.Left != null && head.thisNode.Left.state != Node.states.trail && head.thisNode.Left.state != Node.states.head) {
                        if (head.thisNode.Left.state == Node.states.unoccupied) {
                            this.head.thisNode.state = Node.states.trail;
                            this.head.thisNode = head.thisNode.Left;
                            this.head.thisNode.state = Node.states.head;
                            head.identifier.transform.localPosition = this.head.thisNode.pos;
                            UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                            imageComponet.sprite = Instance.riderLeft;
                        } else {
                            pickupPowerAux(head.thisNode.Left.state, head.thisNode.Left);
                            this.head.thisNode.state = Node.states.trail;
                            this.head.thisNode = head.thisNode.Left;
                            this.head.thisNode.state = Node.states.head;
                            head.identifier.transform.localPosition = this.head.thisNode.pos;
                            UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                            imageComponet.sprite = Instance.riderLeft;
                        }
                    } else {
                        if (!Instance.inmune) {
                            Instance.StartCoroutine(Instance.dieEvent());
                        } else {
                            if (head.thisNode.Left != null) {
                                this.head.thisNode.state = Node.states.trail;
                                this.head.thisNode = head.thisNode.Left;
                                this.head.thisNode.state = Node.states.head;
                                head.identifier.transform.localPosition = this.head.thisNode.pos;
                                UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                                imageComponet.sprite = Instance.riderDown;
                            }
                        }
                    }
                    break;
            }
        }
    public void pickupPowerAux(Node.states state, Node node) {
        switch (state) {
            case Node.states.powerHyperVelocity:
                powerHandler.pickupPower(Node.states.powerHyperVelocity, node);
                break;
            case Node.states.powerShield:
                powerHandler.pickupPower(Node.states.powerShield, node);
                break;
            case Node.states.itemFuel:
                objectsHandler.pickUpItem(state, node);
                break;
            case Node.states.itemBomb:
                objectsHandler.pickUpItem(state, node);
                break;
            case Node.states.itemTrail:
                objectsHandler.pickUpItem(state, node);
                break;
        }
    } 
}