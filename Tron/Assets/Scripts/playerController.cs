using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


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
    public GameObject parentCavas;
    public Sprite riderUp;
    public Sprite riderDown;
    public Sprite riderLeft;
    public Sprite riderRight;
    public TMP_Text fuelRemainingText;
    public int fuelRemaining = 100;

    void Start()
    {
        speed = UnityEngine.Random.Range(0.05f, 0.15f);
        direction = Directions.Down;

        Invoke("setGrid", 0.01f);
        StartCoroutine(playerMovement());
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.UpArrow) && direction != Directions.Down) direction = Directions.Up;
        if (Input.GetKeyDown(KeyCode.DownArrow) && direction != Directions.Up) direction = Directions.Down;
        if (Input.GetKeyDown(KeyCode.RightArrow) && direction != Directions.Left) direction = Directions.Right;
        if (Input.GetKeyDown(KeyCode.LeftArrow) && direction != Directions.Right) direction = Directions.Left;
    }

    IEnumerator playerMovement() {

        yield return new WaitForSeconds(speed);

        int k = 0;
        
        while (true) {
            yield return new WaitForSeconds(speed);

            if (fuelRemaining > 0) {
                if (k == 5) {
                    k = 0;
                    fuelRemainingText.text = $"{--fuelRemaining}/100";
                    bike.updateDirection(direction);
                    bike.headMovement(player);
                    bike.updateChain();
                    continue;
                }

                bike.updateDirection(direction);
                bike.headMovement(player);
                bike.updateChain();
                ++k;
            } else {
                //die
            }
        }


    }

    void setGrid () {
        gridScript = FindObjectOfType<gridCreator>();

        Node[,] grid = gridScript.grid;

        int xPos = UnityEngine.Random.Range(0, 50);
        int yPos = UnityEngine.Random.Range(0, 100);

        playerNode = grid[xPos, yPos];

        GameObject childObject = Instantiate(playerPrefab);
        childObject.transform.SetParent(parentCavas.transform, false);
        childObject.transform.localPosition = playerNode.pos;

        childObject.name = "Player";
        
        bike = new motorBike(playerNode, childObject, direction, this);
        bike.addTrail(10, trailsPrefab, parentCavas);

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


    public motorBike(Node playerNode, GameObject identifier, playerController.Directions direction, playerController instance) {
        head = new motorNode(playerNode, identifier, direction);
        ++size;
        this.Instance = instance;
    }

    public void addTrail(int quantity, GameObject trailsPrefab, GameObject parentCavas) {
        for (int i = 0; i < quantity; i++) {
            if (head.nextNode == null) {

                var result = handleNode(head.nodeDirectionNext, head);

                GameObject child = Instance.instantiateTrail(result);

                motorNode newTrail;
                newTrail = new motorNode(result.Item2, child, result.Item3);
                Debug.Log($"{result.Item1}, {result.Item2.pos}, {result.Item3} | {newTrail}");
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
                Debug.Log($"{result.Item1}, {result.Item2.pos}, {result.Item3} | {newTrail}");
                index.setNext(newTrail);
            }
        }
    }

    public void updateDirection (playerController.Directions direction) {
        this.head.nodeDirectionPrev = this.head.nodeDirectionNext;
        this.head.nodeDirectionNext = direction;
        Debug.Log($"{this.head.nodeDirectionPrev}, ${this.head.nodeDirectionNext}");
    }

    public void updateChain() {
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
                }
                if (nextDirection == playerController.Directions.Down) {
                    index.identifier.transform.localPosition = index.thisNode.Down.pos;
                    index.thisNode = index.thisNode.Down;
                }
                if (nextDirection == playerController.Directions.Right) {
                    index.identifier.transform.localPosition = index.thisNode.Right.pos;
                    index.thisNode = index.thisNode.Right;
                }                
                if (nextDirection == playerController.Directions.Left) {
                    index.identifier.transform.localPosition = index.thisNode.Left.pos;
                    index.thisNode = index.thisNode.Left;
                }                   
                Debug.Log(nextDirection);

                index = index.nextNode;
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

    public void headMovement(RectTransform player) {
            switch (head.nodeDirectionNext)
            {
                case playerController.Directions.Up:
                    if (head.thisNode.Up != null) {
                        this.head.thisNode = head.thisNode.Up;
                        head.identifier.transform.localPosition = this.head.thisNode.pos;
                        UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                        imageComponet.sprite = Instance.riderUp;
                    } else {
                        //die
                    }
                    break;
                case playerController.Directions.Down:
                    if (head.thisNode.Down != null) {
                        this.head.thisNode = head.thisNode.Down;
                        head.identifier.transform.localPosition = this.head.thisNode.pos;
                        UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                        imageComponet.sprite = Instance.riderDown;
                    } else {
                        //die
                    }
                    break;
                case playerController.Directions.Right:
                    if (head.thisNode.Right != null) {
                        this.head.thisNode = head.thisNode.Right;
                        head.identifier.transform.localPosition = this.head.thisNode.pos;
                        UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                        imageComponet.sprite = Instance.riderRight;
                    } else {
                        //die
                    }
                    break;
                case playerController.Directions.Left:
                    if (head.thisNode.Left != null) {
                        this.head.thisNode = head.thisNode.Left;
                        head.identifier.transform.localPosition = this.head.thisNode.pos;
                        UnityEngine.UI.Image imageComponet = head.identifier.GetComponent<UnityEngine.UI.Image>();
                        imageComponet.sprite = Instance.riderLeft;
                    } else {
                        //die
                    }
                    break;
            }
        }
}