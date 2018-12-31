using System.Collections.Generic;
using UnityEngine;

    public class PathFindingGrid : MonoBehaviour {

        public LayerMask obstacleMask; //The layer to check against to see if there is an obstacle there.
        
        public Vector2 gridSize; //The size of the grid x * x.
        
        public float nodeRadius; //The radius of the node. The smaller the radius the more nodes there will be within the grid.

        //This is for debugging purposes within the unity editor. Has no effect on the actual path finding.
        public float gizmoNodeRadius = .5f; 

        //2D array for all the nodes in the grid.
        private Node[,] nodes;

        //This was for testing early on. Agents will be using the request manager class.
        public List<Node> path; //The path the agent will take.

        private float nodeDiameter; //The diameter of the nodes.
        private int sizeX, sizeY; //How many nodes in each axis.

        //Called by unity when the game object is instantiated.
        private void Awake() {

            //Calculate how many nodes will be in each axis x and y.
            nodeDiameter = 2 * nodeRadius;
            sizeX = Mathf.RoundToInt(gridSize.x / nodeDiameter);
            sizeY = Mathf.RoundToInt(gridSize.y / nodeDiameter);

            //Generate the grid.
            GenerateGrid();
        }

        private void GenerateGrid() {

            nodes = new Node[sizeX, sizeY]; //Initialize the nodes array.

            //Get the bottom left most position(world position).
            Vector3 bottomLeft = transform.position - Vector3.right * gridSize.x / 2 - Vector3.forward * gridSize.y / 2;

            for (int y = 0; y < sizeY; y++) {
                for (int x = 0; x < sizeX; x++) {

                    //Calculate the world position.
                    Vector3 worldPosition = bottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) +
                                         Vector3.forward * (y * nodeDiameter + nodeRadius);

                    //Do collision check for each point to find out if an agent can walk at this nodes position.
                    //Create an invisible sphere the size of our node, at the current(iteration in loop)
                    //world point of the current node being generated. If the sphere overlaps with a solid object
                    //based on the unwalkable layer mask it will set node that is being created to not walkable. (node.walkable = false).
                    bool canWalk = true;
                    if (Physics.CheckSphere(worldPosition, nodeRadius, obstacleMask)) {
                        canWalk = false;
                    }
                    //Create a new node and store it in the 2D array that represents the grid.
                    nodes[x, y] = new Node(canWalk, worldPosition, x, y);

                }
            }

        }

        //Gets the correct node in the nodes array from a world position(Vector3).
        /// <summary>
        /// Gets a node at a given world coordinate.
        /// </summary>
        /// <param name="worldPosition">A position or point in the world, 3D space.</param>
        /// <returns></returns>
        public Node NodeFromWorldPosition(Vector3 worldPosition) {

            float xPoint = (worldPosition.x + gridSize.x / 2) / gridSize.x;
            float yPoint = (worldPosition.z + gridSize.y / 2) / gridSize.y;
            xPoint = Mathf.Clamp01(xPoint);
            yPoint = Mathf.Clamp01(yPoint);

            //Gets the correct indecies in the nodes array.
            int x = Mathf.RoundToInt((sizeX - 1) * xPoint);
            int y = Mathf.RoundToInt((sizeY - 1) * yPoint);

            //return node that corresponds to the position in the world
            return nodes[x, y];

        }

        //Gets the neighbors of a node that is passed in.
        /// <summary>
        /// Returns a list of all the neighbors of a given node, max of 8 neighbors.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public List<Node> GetNeighbors(Node node) {

            //New list for the neighbors.
            List<Node> neighbors = new List<Node>();

            //Loop around the node that was passed in.
            for (var x = -1; x <= 1; x++) {
                for (var y = -1; y <= 1; y++) {
                    //if we are on the node tha was passed in, skip this iteration.
                    if (x == 0 && y == 0) {
                        continue;
                    }

                    //Make sure we are within the grid.
                    int xInGrid = node.gridX + x;
                    int yInGrid = node.gridY + y;

                    //Make sure the node is within the grid.
                    if (xInGrid >= 0 && xInGrid < gridSize.x && yInGrid >= 0 && yInGrid < gridSize.y) {
                        neighbors.Add(nodes[xInGrid, yInGrid]); //Adds to the neighbours list.
                    }

                    /* This is how the loop works. Iterates through each possible point in a 3x3 square(grid).
                    * Just a visualization of whats going on.
                     * (-1,1) (0,1) (1,1)
                     * (-1,0) (0,0) (1,0)
                     * (-1,-1) (0,-1) (1,-1)
                     */

                }
            }

            return neighbors;

        }

#if UNITY_EDITOR //This is only editor code for debugging and visualization.
    private void OnDrawGizmos() {
            Gizmos.DrawWireCube(transform.position, new Vector3(gridSize.x, 1, gridSize.y));

            if (nodes != null) {

                foreach (Node n in nodes) {
                    if (!n.walkable) {
                        Gizmos.color = Color.red;
                    }
                    else {
                        Gizmos.color = Color.green;
                    }

                    Gizmos.DrawWireSphere(n.position, gizmoNodeRadius);

                }

                if (path != null) {
                    Gizmos.color = Color.cyan;

                    foreach (Node n in path) {
                        Gizmos.DrawWireSphere(n.position, gizmoNodeRadius);
                    }

                }


            }
        }
#endif
}

