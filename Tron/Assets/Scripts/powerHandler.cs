using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class powerHandler : MonoBehaviour
{
    public gridCreator gridScript;
    public GameObject hyperVelocityPrefab;
    public GameObject shieldPrefab;
    public GameObject parentObject;
    private Node[,] grid;

    void Start()
    {
        Invoke("generatePower", 0.01f);
    }

    void generatePower() {

        gridScript = FindObjectOfType<gridCreator>();
        this.grid = gridScript.grid;

        int cols = Random.Range(0, 100);
        int rows = Random.Range(0, 50);

        Node randomNode = grid[rows, cols];
        randomNode.state = Node.states.powerHyperVelocity;

        GameObject power = Instantiate(hyperVelocityPrefab);
        power.transform.SetParent(parentObject.transform);
        power.transform.localPosition = randomNode.pos;

        randomNode.objectOnCell = power;

        Animator animator = power.GetComponent<Animator>();
        animator.Play("levitateHV");
    }

    void Update()
    {
        
    }
}
