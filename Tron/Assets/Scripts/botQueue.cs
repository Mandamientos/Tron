using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class botQueue : MonoBehaviour
{
    public GameObject parent;
    public GameObject trailItemPrefab;
    public GameObject gascanItemPrefab;
    public GameObject bombItemPrefab;
    public GameObject parentCanvas;
    public botController botControllerScript;
    public gridCreator gridScript;
    public Node[,] grid;
    public BotItemQueue queue;
    void Start()
    {
        botControllerScript = parent.GetComponent<botController>();
        gridScript = FindAnyObjectByType<gridCreator>();

        queue = new BotItemQueue(this);

        StartCoroutine(randomPowers());
        StartCoroutine(applyFront());
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

    IEnumerator randomPowers() {
        while (true) {
            int randomInt = Random.Range(0, 1);
            if (randomInt == 0 && queue.maxSize != queue.size) {
                List<Node.states> powers = new List<Node.states>{Node.states.itemTrail, Node.states.itemFuel};
                int randomIndex = Random.Range(0, powers.Count);
                Debug.Log($"Bot poder generado {powers[randomIndex]}");
                int priority;

                if (powers[randomInt] == Node.states.itemFuel) {
                    priority = 0;
                } else {
                    priority = 1;
                }
                queue.enqueue(powers[randomIndex], priority);
            }
            yield return new WaitForSeconds(8);
        }
    }

    IEnumerator applyFront() {

        while(true) {

            for (int i = 7; i >= 0; i--) {
                yield return new WaitForSeconds(1);
            }

            if (queue.front != null && botControllerScript.isAlive) {
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
                        StartCoroutine(botControllerScript.dieEvent());
                        break;
                }
            }
        }

    }

    public void fuelItem () {
        if (botControllerScript.fuelRemaining != 100) {
            int randomFuelQuantity = Random.Range(15, 25);
            int newFuel = randomFuelQuantity + botControllerScript.fuelRemaining;
            if (newFuel <= 100) {
                botControllerScript.fuelRemaining = newFuel;
            } else {
                botControllerScript.fuelRemaining = 100;
            }
        }
    }

    public void trailItem() {
        botControllerScript.bike.addTrail(7, botControllerScript.botTrailPrefab, botControllerScript.parentObject);
    }

    public void generateItem(Node.states state, GameObject prefab) {

        grid = gridScript.grid;

        int cols = UnityEngine.Random.Range(0, 100);
        int rows = UnityEngine.Random.Range(0, 50);

        Node randomNode = grid[rows, cols];

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
        power.transform.SetParent(parentCanvas.transform);
        power.transform.localPosition = randomNode.pos;

        randomNode.objectOnCell = power;
    }

}

public class botQueueNode {
    public Node.states itemType;
    public botQueueNode nextNode;
    public int priority;
    
    public botQueueNode(Node.states itemType, int priority) {
        this.itemType = itemType;
        this.priority = priority;
        this.nextNode = null;
    }
}

public class BotItemQueue {
    public botQueueNode front;
    public botQueueNode rear;
    public int maxSize;
    public int size;
    public botQueue instance;


    public BotItemQueue(botQueue instance) {
        front = null;
        rear = null;
        maxSize = 8;
        size = 0;
        this.instance = instance;
    }

    public void enqueue(Node.states itemType, int priority) {
        if (size <= maxSize) {
            botQueueNode newNode = new botQueueNode(itemType, priority);
            if (front == null) {
                front = newNode;
                rear = newNode;
                ++size;
            } else if (front.priority > newNode.priority) {
                newNode.nextNode = front;
                front = newNode;
                ++size;
            } else {
                botQueueNode marker = front;
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
        }
    }

    public Node.states dequeue() {
        Node.states itemType = front.itemType;
        this.front = front.nextNode;
        --size;
        return itemType;
    }

    public void iterar() {
        botQueueNode marker = front;
        while (marker != null) {
            Debug.Log($"{marker.itemType} : {marker.priority} ");
            marker = marker.nextNode;
        }
    }
}