using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node {

    public int gridX, gridY; //X and Y position in the grid (index). Have each node keep track of its own index in the array.
    public bool walkable; //Can the agent walk on this node, is there an obstacle there?
    public Vector3 position; //The transform.position of this node in the world, 3D space.

    public Node parent; //The node that came before 'this' node. This is assigned when searching for a path.

    public int moveCost; //Cost to move to this node.

    //gCost is the cost to move to the next node.
    //hCost is the distance from this node to the goal node.
    public int gCost, hCost;

    public int fCost {
        get { return gCost + hCost; }
    }

    public Node(bool walkable, Vector3 position, int gridX, int gridY) {
        this.position = position;
        this.gridX = gridX;
        this.gridY = gridY;
        this.walkable = walkable;
    }

    public Node(bool walkable, Vector3 position, int gridX, int gridY, int moveCost) {
        this.position = position;
        this.gridX = gridX;
        this.gridY = gridY;
        this.walkable = walkable;
        this.moveCost = moveCost;
    }

}


