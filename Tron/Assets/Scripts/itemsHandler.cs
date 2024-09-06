using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class objectsHandler : MonoBehaviour
{
    public GameObject item1;
    public GameObject item2;
    public GameObject item3;
    public GameObject item4;
    public GameObject item5;
    public GameObject item6;
    public GameObject item7;
    public GameObject item8;
    public GameObject trailItemPrefab;
    public GameObject gascanItemPrefab;
    public GameObject bombItemPrefab;
    public GameObject parentObject;
    public List<GameObject> items;
    public List<UnityEngine.UI.Image> images;
    public itemQueue queue;
    public Sprite gascanSprite;
    public Sprite bombSprite;
    public Sprite trailSprite;
    public Sprite noItemSprite;
    public TMP_Text timeRemaining;
    public playerController playerControllerScript;
    public gridCreator gridScript;
    public Node[,] grid;

    void Start()
    {
        items.AddRange(new GameObject[] {item1,item2,item3,item4,item5,item6,item7,item8});

        foreach (GameObject item in items) {
            UnityEngine.UI.Image image = item.GetComponent<UnityEngine.UI.Image>();
            images.Add(image);
        }

        queue = new itemQueue(this);

        playerControllerScript = FindObjectOfType<playerController>();
        gridScript = FindObjectOfType<gridCreator>();

        grid = gridScript.grid;

        StartCoroutine(randomItem());
        StartCoroutine(applyFront());
    }

    IEnumerator applyFront() {

        while(true) {

            for (int i = 7; i >= 0; i--) {
                timeRemaining.text = $"{i}S";
                yield return new WaitForSeconds(1);
            }

            if (queue.front != null) {
                switch (queue.front.itemType) {
                    case Node.states.itemTrail:
                        queue.dequeue();
                        trailItem();
                        break;
                    case Node.states.itemFuel:
                        queue.dequeue();
                        fuelItem();
                        break;
                    case Node.states.itemBomb:
                        queue.dequeue();
                        StartCoroutine(playerControllerScript.dieEvent());
                        break;
                }
            }
            updateQueue();
        }

    }

    public void fuelItem () {
        if (playerControllerScript.fuelRemaining != 100) {
            int randomFuelQuantity = Random.Range(15, 25);
            int newFuel = randomFuelQuantity + playerControllerScript.fuelRemaining;
            if (newFuel <= 100) {
                playerControllerScript.fuelRemaining = newFuel;
            } else {
                playerControllerScript.fuelRemaining = 100;
            }
        }
    }

    public void trailItem() {
        playerControllerScript.bike.addTrail(7, playerControllerScript.trailsPrefab, playerControllerScript.parentCavas);
    }

    public void updateQueue() {
        queueNode marker = queue.front;
        foreach (UnityEngine.UI.Image image in images) {
            if (marker == null) {
                image.sprite = noItemSprite;
            } else {
                switch (marker.itemType) {
                    case Node.states.itemFuel:
                        image.sprite = gascanSprite;
                        break;
                    case Node.states.itemBomb:
                        image.sprite = bombSprite;
                        break;
                    case Node.states.itemTrail:
                        image.sprite = trailSprite;
                        break;
                }
                marker = marker.nextNode;
            }
        }
    }

    public void pickUpItem(Node.states powerType, Node node) {
        if (powerType == Node.states.itemFuel) {
            queue.enqueue(powerType, 0);
        } else {
            queue.enqueue(powerType, 1);
        }

        Node middleNode = node.reference;
        middleNode.state = Node.states.unoccupied;
        Destroy(middleNode.objectOnCell);
        if (middleNode.Up != null) {
            middleNode.Up.state = Node.states.unoccupied;
            middleNode.Up.reference = null;
        }
        if (middleNode.Down != null) {
            middleNode.Down.state = Node.states.unoccupied;
            middleNode.Down.reference = null;
        }
        if (middleNode.Left != null) {
            middleNode.Left.state = Node.states.unoccupied;
            middleNode.Left.reference = null;
        }
        if (middleNode.Right != null) {
            middleNode.Right.state = Node.states.unoccupied;
            middleNode.Right.reference = null;
        }
    }

    public IEnumerator randomItem() {
    
        while (true) {
            int randomNumber = Random.Range(0, 5);

            if (randomNumber == 0 || randomNumber == 1) generateItem(Node.states.itemFuel, gascanItemPrefab);
            if (randomNumber == 2 || randomNumber == 3) generateItem(Node.states.itemTrail, trailItemPrefab);
            if (randomNumber == 4) generateItem(Node.states.itemBomb, bombItemPrefab);

            yield return new WaitForSeconds(5f);
        }
    }

    public void generateItem(Node.states state, GameObject prefab) {

        Node randomNode = null;
        bool generable = false;

        while (!generable) {
            int cols = UnityEngine.Random.Range(0, 100);
            int rows = UnityEngine.Random.Range(0, 50);

            randomNode = grid[rows, cols];
            
            if (randomNode.Up != null) {
                if (randomNode.Up.state != Node.states.unoccupied) {
                    continue;
                }
            }
            if (randomNode.Down != null) {
                if (randomNode.Down.state != Node.states.unoccupied) {
                    continue;
                }
            }
            if (randomNode.Left != null) {
                if (randomNode.Left.state != Node.states.unoccupied) {
                    continue;
                }
            }
            if (randomNode.Right != null) {
                if (randomNode.Right.state != Node.states.unoccupied) {
                    continue;
                }
            }

            generable = true;
        }

        randomNode.state = state;
        randomNode.reference = randomNode;

        if (randomNode.Up != null) {
            randomNode.Up.state = state;
            randomNode.Up.reference = randomNode;
        }
        if (randomNode.Down != null) {
            randomNode.Down.state = state;
            randomNode.Down.reference = randomNode;
        }
        if (randomNode.Left != null) {
            randomNode.Left.state = state;
            randomNode.Left.reference = randomNode;
        }
        if (randomNode.Left != null) {
            randomNode.Left.state = state;
            randomNode.Left.reference = randomNode;
        }

        GameObject power = Instantiate(prefab);
        power.transform.SetParent(parentObject.transform);
        power.transform.localPosition = randomNode.pos;

        randomNode.objectOnCell = power;
    }
}

public class queueNode {
    public Node.states itemType;
    public queueNode nextNode;
    public int priority;
    
    public queueNode(Node.states itemType, int priority) {
        this.itemType = itemType;
        this.priority = priority;
        this.nextNode = null;
    }
}

public class itemQueue {
    public queueNode front;
    public queueNode rear;
    public int maxSize;
    public int size;
    public objectsHandler instance;


    public itemQueue(objectsHandler instance) {
        front = null;
        rear = null;
        maxSize = 8;
        size = 0;
        this.instance = instance;
    }

    public void enqueue(Node.states itemType, int priority) {
        if (size <= maxSize) {
            queueNode newNode = new queueNode(itemType, priority);
            if (front == null) {
                front = newNode;
                rear = newNode;
                ++size;
            } else if (front.priority > newNode.priority) {
                newNode.nextNode = front;
                front = newNode;
                ++size;
            } else {
                queueNode marker = front;
                while (marker.nextNode != null && marker.nextNode.priority <= newNode.priority) {
                    marker = marker.nextNode;
                }

                newNode.nextNode = marker.nextNode;
                marker.nextNode = newNode;
                if (newNode.nextNode == null) {
                    rear = newNode;
                }
                ++size;
            }
            instance.updateQueue();
        }
    }

    public Node.states dequeue() {
        Node.states itemType = front.itemType;
        this.front = front.nextNode;
        --size;
        return itemType;
    }

    public void iterar() {
        queueNode marker = front;
        while (marker != null) {
            Debug.Log($"{marker.itemType} : {marker.priority} ");
            marker = marker.nextNode;
        }
    }
}