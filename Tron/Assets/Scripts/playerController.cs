using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;


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
    public GameObject parentCavas;

    void Start()
    {
        speed = Random.Range(0.05f, 0.15f);

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

        while (true){
            switch (direction)
            {
                case Directions.Up:
                    if (this.playerNode.Up != null) {
                        this.playerNode = playerNode.Up;
                        player.anchoredPosition = this.playerNode.pos;
                    } else {
                        //die
                    }
                    break;
                case Directions.Down:
                    if (this.playerNode.Down != null) {
                        this.playerNode = playerNode.Down;
                        player.anchoredPosition = this.playerNode.pos;
                    } else {
                        //die
                    }
                    break;
                case Directions.Right:
                    if (this.playerNode.Right != null) {
                        this.playerNode = playerNode.Right;
                        player.anchoredPosition = this.playerNode.pos;
                    } else {
                        //die
                    }
                    break;
                case Directions.Left:
                    if (this.playerNode.Left != null) {
                        this.playerNode = playerNode.Left;
                        player.anchoredPosition = this.playerNode.pos;
                    } else {
                        //die
                    }
                    break;
            }

            bike.updateTrail(playerNode, direction);
            yield return new WaitForSeconds(speed);
        }
    }

    void setGrid () {
        gridScript = FindObjectOfType<gridCreator>();

        Node[,] grid = gridScript.grid;

        int xPos = Random.Range(0, 50);
        int yPos = Random.Range(0, 100);

        player.anchoredPosition = grid[xPos, yPos].pos;
        this.playerNode = grid[xPos, yPos];
        
        bike = new motorBike(playerNode);
        StartCoroutine(bike.addTrail(trailsPrefab, parentCavas));
    }

}

public class motorNode {
    public Node thisNode;
    public motorNode nextNode;

    public motorNode(Node thisNode) {
        this.thisNode = thisNode;
        this.nextNode = null;
    }
}

public class motorBike : playerController {
    public motorNode head;
    public Directions directions;
    private int size;


    public motorBike(Node playerNode) {
        head = new motorNode(playerNode);
        ++size;
    }

    public IEnumerator addTrail(GameObject trailsPrefab, GameObject parentCavas) {
        while (true) {
            yield return new WaitForSeconds(0.1f);
                switch (this.directions) {
                    case Directions.Up:
                        if (head.thisNode.Down != null) { 
                            head.nextNode = new motorNode(head.thisNode.Down);
                            GameObject childObject = Instantiate(trailsPrefab);
                            childObject.transform.SetParent(parentCavas.transform);
                            childObject.transform.localPosition = head.thisNode.Down.pos;
                            break;
                        }
                        break;
                }
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void updateTrail (Node playerNode, Directions direction) {
        head.thisNode = playerNode;
        this.directions = direction;
    }

}