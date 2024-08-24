using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class powerHandler : MonoBehaviour
{
    public gridCreator gridScript;
    public GameObject hyperVelocityPrefab;
    public GameObject shieldPrefab;
    public GameObject parentObject;
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
    powerStack stack;

    void Start()
    {
        StartCoroutine(generatePower(Node.states.powerHyperVelocity, hyperVelocityPrefab));
        
        itemFrames.AddRange(new GameObject[] {item1, item2, item3, item4, item5, item6, item7});

        foreach (GameObject item in itemFrames) {
            UnityEngine.UI.Image image = item.GetComponent<UnityEngine.UI.Image>();
            itemImages.Add(image);
            Debug.Log(image);
        }

        stack = new powerStack(this);
    }

    public IEnumerator generatePower(Node.states state, GameObject prefab) {

        yield return new WaitForSeconds(0.1f);

        gridScript = FindObjectOfType<gridCreator>();
        this.grid = gridScript.grid;

        int cols = Random.Range(0, 100);
        int rows = Random.Range(0, 50);

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
        if (randomNode.Right != null) {
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
            }
            marker = marker.nextNode;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            int random = Random.Range(1, 3);
            Debug.Log(random);
            if (random == 1) {
                stack.push(Node.states.powerShield);
            } else {
                stack.push(Node.states.powerHyperVelocity);
            }
        }
    }
}

class stackNode {
    public Node.states powerType;
    public stackNode nextNode;

    public stackNode(Node.states powerType) {
        this.powerType = powerType;
        this.nextNode = null;
    }
}

class powerStack {
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

}