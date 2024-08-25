using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
    public List<GameObject> items;
    public List<UnityEngine.UI.Image> images;
    public itemQueue queue;

    void Start()
    {
        items.AddRange(new GameObject[] {item1,item2,item3,item4,item5,item6,item7,item8});

        foreach (GameObject item in items) {
            UnityEngine.UI.Image image = item.GetComponent<UnityEngine.UI.Image>();
            images.Add(image);
        }

        queue = new itemQueue();
    }
    void Update()
    {
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

    public itemQueue() {
        front = null;
        rear = null;
    } 

    public void enqueue(Node.states itemType, int priority) {
        queueNode newNode = new queueNode(itemType, priority);
        if (front == null) {
            front = newNode;
            rear = newNode;
        } else if (front.priority > newNode.priority) {
            newNode.nextNode = front;
            front = newNode;
        } else {
            queueNode marker = front;
            while (marker.nextNode != null && marker.nextNode.priority <= newNode.priority) {
                marker = marker.nextNode;
            }

            newNode.nextNode = marker.nextNode;
            if (newNode.nextNode == null) {
                rear = newNode;
            }
        }
    }

    public Node.states dequeue() {
        Node.states itemType = front.itemType;
        front = front.nextNode;
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