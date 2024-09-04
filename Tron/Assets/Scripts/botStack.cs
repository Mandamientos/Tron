using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class botStack : MonoBehaviour
{
    public botPowerStack stack;
    public botController botControllerScript;
    public GameObject powerApplied;
    public GameObject velocityActivatedPrefab;
    public GameObject shieldActivatedPrefab;
    public GameObject parentCanvas;
    public GameObject hyperVelocityPrefab;
    public GameObject shieldPrefab;
    public AudioSource powerActivated;
    public Node[,] grid;
    void Start()
    {
        stack = new botPowerStack(this);
        botControllerScript = GetComponent<botController>();
        StartCoroutine(randomPowers());
        StartCoroutine(applyTop());
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

    IEnumerator randomPowers() {
        while (true) {
            int randomInt = Random.Range(0, 1);
            if (randomInt == 0 && stack.maxSize != stack.size) {
                List<Node.states> powers = new List<Node.states>{Node.states.powerShield, Node.states.powerHyperVelocity};
                int randomIndex = Random.Range(0, powers.Count);
                Debug.Log($"Bot poder generado {powers[randomIndex]}");
                stack.push(powers[randomIndex]);
            }
            yield return new WaitForSeconds(6);
        }
    }

    IEnumerator applyTop() {

        int randomInt = Random.Range(10, 20);

        while(true) {

            for (int i = randomInt; i >= 0; i--) {
                yield return new WaitForSeconds(1);
            }

            if (stack.top != null && botControllerScript.isAlive) {
                switch (stack.top.powerType) {
                    case Node.states.powerHyperVelocity:
                        stack.pop();
                        StartCoroutine(powerVelocity());
                        break;
                    case Node.states.powerShield:
                        stack.pop();
                        StartCoroutine(powerShield());
                        break;
                }
            }
        }
    }

    public IEnumerator powerVelocity() {
        float previousSpeed = botControllerScript.botSpeed;
        botControllerScript.botSpeed = 0.03f;
        GameObject velocity = Instantiate(velocityActivatedPrefab);
        velocity.transform.SetParent(powerApplied.transform);
        velocity.transform.localPosition = botControllerScript.bike.head.thisNode.pos;
        powerActivated.Play();
        botControllerScript.k = 9999;
        yield return new WaitForSeconds(5);
        botControllerScript.k = 0;
        Destroy(velocity);
        botControllerScript.botSpeed = previousSpeed;
    }

    public IEnumerator powerShield() {
        float startTime = Time.time;
        GameObject shield = Instantiate(shieldActivatedPrefab);
        shield.transform.SetParent(powerApplied.transform);
        shield.transform.localPosition = botControllerScript.bike.head.thisNode.pos;
        powerActivated.Play();

        Animator animator = shield.GetComponent<Animator>();

        animator.SetTrigger("activate");

        botControllerScript.isInmune = true;

        while ((Mathf.RoundToInt(Time.time - startTime)) < 5) {

            shield.transform.localPosition = botControllerScript.bike.head.thisNode.pos;
            yield return new WaitForSeconds(0.01f);

        }

        animator.SetTrigger("deactivate");
        powerActivated.Play();

        yield return new WaitForSeconds(0.5f);

        botControllerScript.isInmune = false;

        Destroy(shield);
    }

    public void botPickupPower(Node.states power, Node node) {
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

        public IEnumerator generatePower(Node.states state, GameObject prefab) {

        yield return new WaitForSeconds(0.1f);


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
            randomNode.Right.reference = randomNode;
        }

        GameObject power = Instantiate(prefab);
        power.transform.SetParent(parentCanvas.transform);
        power.transform.localPosition = randomNode.pos;

        randomNode.objectOnCell = power;
    }

    void Update()
    {
        
    }
}

public class botStackNode {
    public Node.states powerType;
    public botStackNode nextNode;

    public botStackNode(Node.states powerType) {
        this.powerType = powerType;
        this.nextNode = null;
    }
}

public class botPowerStack {
    public botStackNode top;
    public int maxSize = 7;
    public int size;
    private botStack instance;

    public botPowerStack(botStack instance) {
        this.top = null;
        this.size = 0;
        this.instance = instance;
    }

    public void push(Node.states powerType) {
        botStackNode newPower = new botStackNode(powerType);
        newPower.nextNode = top;
        top = newPower;
        ++size;
    }

    public void pop() {
        if (top != null) {
            top = top.nextNode;
            --size;
        }
    }

    public Node.states returnPower() {
        return top.powerType;
    }

    public void orderStack(int index, powerStack stack) {
        if (stack.size >= index) {
            botPowerStack auxStack = new botPowerStack(instance);
            for (int i = 1; i < index; i++) {
                auxStack.push(top.powerType);
                stack.pop();
            }

            stackNode newTop = stack.top;
            stack.pop();

            while (auxStack.top != null) {
                stack.push(auxStack.top.powerType);
                auxStack.pop();
            }

            stack.push(newTop.powerType);
        }
    }
}