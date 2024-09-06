using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class powerHandler : MonoBehaviour
{
    public gridCreator gridScript;
    public bool isRunningPower;
    public playerController playerControllerScript;
    public GameObject hyperVelocityPrefab;
    public GameObject shieldPrefab;
    public GameObject parentObject;
    public GameObject shieldParent;
    public GameObject shieldActivatedPrefab;
    public GameObject velocityActivatedPrefab;
    public AudioSource powerActivated;
    public Sprite noItemSprite;
    public Sprite hyperVelocitySprite;
    public Sprite shieldSprite;
    public GameObject item1;
    public GameObject item2;
    public GameObject item3;
    public GameObject item4;
    public GameObject item5;
    public GameObject item6;
    public GameObject item7;
    public List<GameObject> itemFrames = new List<GameObject>();
    public List<UnityEngine.UI.Image> itemImages = new List<UnityEngine.UI.Image>();
    private Node[,] grid;
    public powerStack stack;

    void Start()
    {        
        itemFrames.AddRange(new GameObject[] {item1, item2, item3, item4, item5, item6, item7});

        foreach (GameObject item in itemFrames) {
            UnityEngine.UI.Image image = item.GetComponent<UnityEngine.UI.Image>();
            itemImages.Add(image);
        }

        playerControllerScript = FindObjectOfType<playerController>();

        stack = new powerStack(this);

        StartCoroutine(randomPower());
    }

    public IEnumerator randomPower() {
        while (true) {
            Debug.Log("poder generado");
            int random = UnityEngine.Random.Range(1, 3);
            if (random == 1) {
                StartCoroutine(generatePower(Node.states.powerHyperVelocity, hyperVelocityPrefab));
            } else {
                StartCoroutine(generatePower(Node.states.powerShield, shieldPrefab));
            }
            yield return new WaitForSeconds(10);
        }
    }

    public IEnumerator generatePower(Node.states state, GameObject prefab) {

        yield return new WaitForSeconds(6f);

        gridScript = FindObjectOfType<gridCreator>();
        this.grid = gridScript.grid;

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
            randomNode.Right.reference = randomNode;
        }

        GameObject power = Instantiate(prefab);
        power.transform.SetParent(parentObject.transform);
        power.transform.localPosition = randomNode.pos;

        randomNode.objectOnCell = power;
    }

    public void updatePowers () {
        stackNode marker = stack.top;
        foreach (UnityEngine.UI.Image image in itemImages) {
            if (marker == null) {
                image.sprite = noItemSprite;
            } else {
                switch (marker.powerType) {
                    case Node.states.powerHyperVelocity:
                        image.sprite = hyperVelocitySprite;
                        break;
                    case Node.states.powerShield:
                        image.sprite = shieldSprite;
                        break;
                }
                marker = marker.nextNode;
            }
        }
    }

    public void pickupPower(Node.states power, Node node) {
        if (stack.size != stack.maxSize) {
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
            stack.push(power);
        }
    }

    public IEnumerator powerVelocity() {
        isRunningPower = true;
        float previousSpeed = playerControllerScript.speed;
        playerControllerScript.speed = 0.03f;
        GameObject velocity = Instantiate(velocityActivatedPrefab);
        velocity.transform.SetParent(shieldParent.transform);
        velocity.transform.localPosition = playerControllerScript.bike.head.thisNode.pos;
        powerActivated.volume = PlayerPrefs.GetFloat("SFXVolume", 0);
        powerActivated.Play();
        playerControllerScript.k = 9999;
        yield return new WaitForSeconds(5);
        playerControllerScript.k = 0;
        Destroy(velocity);
        playerControllerScript.speed = previousSpeed;
        isRunningPower = false;
    }

    public IEnumerator powerShield() {
        isRunningPower = true;
        float startTime = Time.time;
        GameObject shield = Instantiate(shieldActivatedPrefab);
        shield.transform.SetParent(shieldParent.transform);
        shield.transform.localPosition = playerControllerScript.bike.head.thisNode.pos;
        powerActivated.volume = PlayerPrefs.GetFloat("SFXVolume", 0);
        powerActivated.Play();

        Animator animator = shield.GetComponent<Animator>();

        animator.SetTrigger("activate");

        playerControllerScript.inmune = true;

        while ((Mathf.RoundToInt(Time.time - startTime)) < 5) {

            shield.transform.localPosition = playerControllerScript.bike.head.thisNode.pos;
            yield return new WaitForSeconds(0.01f);

        }

        animator.SetTrigger("deactivate");
        powerActivated.Play();

        yield return new WaitForSeconds(0.5f);

        playerControllerScript.inmune = false;

        Destroy(shield);
        isRunningPower = false;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            if (!isRunningPower) {
                Node.states power;
                if (stack.top != null) {
                    power = stack.returnPower();
                    stack.pop();
                    Debug.Log(power);
                    if (power == Node.states.powerHyperVelocity) {
                        StartCoroutine(powerVelocity());
                    } else {
                        StartCoroutine(powerShield());
                    }
                }
            }
        }
        if(Input.GetKeyDown(KeyCode.Alpha2)) {
            stack.orderStack(2, stack);
        }
        if(Input.GetKeyDown(KeyCode.Alpha3)) {
            stack.orderStack(3, stack);
        }
        if(Input.GetKeyDown(KeyCode.Alpha4)) {
            stack.orderStack(4, stack);
        }
        if(Input.GetKeyDown(KeyCode.Alpha5)) {
            stack.orderStack(5, stack);
        }
        if(Input.GetKeyDown(KeyCode.Alpha6)) {
            stack.orderStack(6, stack);
        }
        if(Input.GetKeyDown(KeyCode.Alpha7)) {
            stack.orderStack(7, stack);
        }
    }

    
}

public class stackNode {
    public Node.states powerType;
    public stackNode nextNode;

    public stackNode(Node.states powerType) {
        this.powerType = powerType;
        this.nextNode = null;
    }
}

public class powerStack {
    public stackNode top;
    public int maxSize = 7;
    public int size;
    private powerHandler instance;

    public powerStack(powerHandler instance) {
        this.top = null;
        this.size = 0;
        this.instance = instance;
    }

    public void push(Node.states powerType) {
        stackNode newPower = new stackNode(powerType);
        newPower.nextNode = top;
        top = newPower;
        ++size;
        instance.updatePowers();
    }

    public void pop() {
        if (top != null) {
            top = top.nextNode;
            --size;
            instance.updatePowers();
        }
    }

    public Node.states returnPower() {
        return top.powerType;
    }

    public void orderStack(int index, powerStack stack) {
        if (stack.size >= index) {
            auxQueue auxQueue = new auxQueue();
            int customPriority = index;
            for (int i = 1; i < index; i++) {
                auxQueue.enqueue(top.powerType, customPriority);
                --customPriority;
                stack.pop();
                auxQueue.iterar();
            }

            stackNode newTop = stack.top;
            stack.pop();

            while (auxQueue.front != null) {
                Node.states newElement = auxQueue.dequeue();
                Debug.Log(newElement);
                Debug.Log(auxQueue.front);
                stack.push(newElement);
            }

            stack.push(newTop.powerType);
        }
    }
}

public class auxQueueNode {
    public auxQueueNode nextNode;
    public Node.states powerType;
    public int priority;

    public auxQueueNode(int priority, Node.states powerType) {
        this.priority = priority;
        this.nextNode = null;
        this.powerType = powerType;
    }
}

public class auxQueue {
    public auxQueueNode front;
    public auxQueueNode rear;

    public void enqueue (Node.states powerType, int priority) {
        auxQueueNode newNode = new auxQueueNode(priority, powerType);
            if (front == null) {
                front = newNode;
                rear = newNode;
            } else if (front.priority > newNode.priority) {
                newNode.nextNode = front;
                front = newNode;
            } else {
                auxQueueNode marker = front;
                while (marker.nextNode != null && marker.nextNode.priority <= newNode.priority) {
                    marker = marker.nextNode;
                }

                newNode.nextNode = marker.nextNode;
                marker.nextNode = newNode;
                if (newNode.nextNode == null) {
                    rear = newNode;
                }
            }
        }

    public Node.states dequeue () {
        auxQueueNode frontNode = front;
        front = front.nextNode;
        return frontNode.powerType;
    }
    public void iterar() {
        auxQueueNode marker = front;
        while (marker != null) {
            Debug.Log($"{marker.powerType} : {marker.priority} ");
            marker = marker.nextNode;
        }
    }
}