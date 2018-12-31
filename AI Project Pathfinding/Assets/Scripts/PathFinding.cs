using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour {

    private PathFindingGrid grid; //Reference to our grid of nodes.
    private RequestManager rm; //Reference to our request manager.

    private void Awake() {
        //Get the references when this object is instantiated.
        rm = GetComponent<RequestManager>();
        grid = GetComponent<PathFindingGrid>();
    }

    /// <summary>
    /// Starts the coroutine to find a path using A*.
    /// </summary>
    /// <param name="startPosition">Agents starting point.</param>
    /// <param name="targetPosition">Agents target point.</param>  
    public void FindPathAStar(Vector3 startPosition, Vector3 targetPosition) {
        PathAStar(startPosition, targetPosition);
    }

    /// <summary>
    /// Finds a path using BFS.
    /// </summary>
    /// <param name="startPosition">Agents starting point.</param>
    /// <param name="targetPosition">Agents target point.</param>
    public void FindPathBFS(Vector3 startPosition, Vector3 targetPosition) {
        PathBFS(startPosition, targetPosition);
    }


    /// <summary>
    /// Finds a path using A*
    /// </summary>
    /// <param name="startPosition">The starting position of the agent.</param>
    /// <param name="targetPosition">The target position.</param>
    private void PathAStar(Vector3 startPosition, Vector3 targetPosition) {

        //Get both nodes from the given world positions, start and target.
        Node startNode = grid.NodeFromWorldPosition(startPosition);
        Node targetNode = grid.NodeFromWorldPosition(targetPosition);

        //Initialize open and closed set.
        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        bool success = false;

        //First add starting node to the open set.
        openSet.Add(startNode);

        //Only loop while the open set has nodes.
        while (openSet.Count > 0) {

            Node curNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++) {

                if ((openSet[i].fCost < curNode.fCost ||
                    openSet[i].fCost == curNode.fCost) && openSet[i].hCost < curNode.hCost) {
                    //If the fCost of the node in open set is less than the current node.
                    curNode = openSet[i]; //Since the cost of that node is less, thats the node we want. (Shortest path)
                }
            } //Now that the current node is set to the node with the lowest fCost, remove it from open set and put in closed set.

            openSet.Remove(curNode);
            closedSet.Add(curNode);

            if (curNode == targetNode) {
                //If the current node is equal to the target node, we reached the destination.
                success = true;
                break;
            }

            //Get the neighbors of the current node.
            List<Node> neighbors = grid.GetNeighbors(curNode);

            //Loops through each of the current nodes neighbors.
            foreach (Node neighborNode in neighbors) {

                //if the neighbor is not walkable or the neighbor is in the closed set, skip to next neighbor.
                if (!neighborNode.walkable || closedSet.Contains(neighborNode))
                    continue;

                int newCost = curNode.gCost + GetManhattenDistance(neighborNode, targetNode);

                if (newCost < neighborNode.gCost || !openSet.Contains(neighborNode)) {
                    //Assign new values to neighbor node.
                    neighborNode.gCost = newCost;
                    neighborNode.hCost = GetManhattenDistance(neighborNode, targetNode);
                    neighborNode.parent = curNode;

                    //If a neighboring node is not in the open set add it to be explored.
                    if (!openSet.Contains(neighborNode)) {
                        openSet.Add(neighborNode);
                    }
                }
            }
        }

        Vector3[] pathPoints = new Vector3[0];

        if (success)
            pathPoints = GetPath(startNode, targetNode);

        rm.FinishedProcessing(pathPoints, success);
    }


    /*
     * Do not know why but cant seem to get BFS to work.
     * But the code tested is here or at least one of the
     * implementations that all lead to the same outcome.
    */

    /// <summary>
    /// Finds a path using BFS.
    /// </summary>
    /// <param name="startPosition">The starting position of the agent.</param>
    /// <param name="targetPosition">The target position.</param>
    private void PathBFS(Vector3 startPosition, Vector3 targetPosition) {
        /* Steps:
         * 
         * 1a. Add the starting node(node which the AI starts from) to the frontier.
         * 1b. Dequeue(Take next in line node out of the frontier queue and set it equal to the current node reference variable.
         * 2. Check to see if the current node is equal to the target(Destination).
         * 3. If not equal to the target, check to see if the node was explored already, if not add it to the explored list.
         * 4. Then get the neighbors for the 'current node'.
         * 5. For each of the neighbors in the neighbors list, enqueue each of the neighbors if it has not been explored and is not already in the frontier.
         * 6. Set each of the previous node references for each of the current nodes neighbors to the current node.
         * 7. Repeat, follow steps from 1b and on.
         * 
         */

        //Get both nodes from the given world positions, start and target.
        Node startNode = grid.NodeFromWorldPosition(startPosition);
        Node targetNode = grid.NodeFromWorldPosition(targetPosition);

        //Initialize frontier and explored data structures.
        Queue<Node> frontier = new Queue<Node>();
        HashSet<Node> explored = new HashSet<Node>();

        frontier.Enqueue(startNode);
        explored.Add(startNode);

        bool isDone = false; //Is the process done or not.
        bool success = false; //Did we reach the target node?

        while (!isDone) {
            Debug.Log("Made it to loop");
            if (frontier.Count > 0) {
                Node curNode = frontier.Dequeue(); //Set the current node.

                //if the current node equals the target node
                if (curNode.position == targetNode.position) {
                    Debug.Log("Arrived at target.");
                    //targetNode.parent = explored[explored.Count - 1];
                    isDone = true;
                    success = true;
                    break;
                }

                Debug.Log("Working...");

                if (!explored.Contains(curNode)) { //If the list of explored does not contain the current node we searchin, add it
                    explored.Add(curNode);
                }

                //Get the neighbors of the current node and store those nodes in a list.
                List<Node> neighbors = grid.GetNeighbors(curNode);

                //Iterate over the neighbors.
                for (var i = 0; i < neighbors.Count; i++) {

                    //Add neighbors to frontier if its not explored and not in the frontier already
                    if (!explored.Contains(neighbors[i]) && !frontier.Contains(neighbors[i])) {
                        frontier.Enqueue(neighbors[i]);
                    }

                    neighbors[i].parent = curNode; //Set the previous node for each of the neighbor nodes to the current node.
                }

            }
            else {
                isDone = true; //if the frontiers count is 0, nothing left, so its done
            }
        }

        Vector3[] pathPoints = new Vector3[0];

        if (success) {
            Debug.Log("success");
            pathPoints = GetPath(startNode, targetNode);
        }

        rm.FinishedProcessing(pathPoints, success);
        Debug.Log("At end.");
    }

    //This will be called once the target node was found.
    private Vector3[] GetPath(Node startNode, Node targetNode) {

       List<Node> path = new List<Node>(); //List to hold the points in the world for the path.

        Node curNode = targetNode;

        //Use the parents or each node to populate the list which will be the path.
        while (curNode != startNode) {
            path.Add(curNode);
            curNode = curNode.parent;
        }

        Vector3[] points = GetPositions(path);
        //Reverse the path to get the correct oder.
        Array.Reverse(points);

        return points;
    }

    //Gets the world positions of each node in the path.
    //This is so the agent can be moved  by its transform.
    private Vector3[] GetPositions(List<Node> path) {

        Vector3[] points = new Vector3[path.Count];

        for (int i = 0; i < path.Count; i++) {
            points[i] = path[i].position; //Get all the node positions in the world.
        }

        return points;
    }

    /// <summary>
    /// Gets the Manhatten distance between two given nodes.
    /// </summary>
    /// <param name="n1"></param>
    /// <param name="n2"></param>
    /// <returns></returns>
    private int GetManhattenDistance(Node n1, Node n2) {
        //Uses absolute values to ensure no negative distances.
        int ix = Mathf.Abs(n1.gridX - n2.gridX);
        int iy = Mathf.Abs(n1.gridY - n2.gridY);

        return ix + iy;
    }


}

