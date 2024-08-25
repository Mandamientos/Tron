using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objectsHandler : MonoBehaviour
{
    void Start()
    {
        
    }
    void Update()
    {
        
    }
}

public class queueNode {
    public Node.states itemType;
    public queueNode nextNode;
    
    public queueNode(Node.states itemType) {
        this.itemType = itemType;
        this.nextNode = null;
    }
}

public class itemQueue {

}