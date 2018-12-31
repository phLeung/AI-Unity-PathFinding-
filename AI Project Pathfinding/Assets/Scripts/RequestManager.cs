using System;
using System.Collections.Generic;
using UnityEngine;

public class RequestManager : MonoBehaviour {

    private Queue<PathRequest> pathRequests; //A queue to hold all the request to execute them in the oder thy were received.
    private PathRequest currentRequest; //The request that is currently being processed.
    private PathFinding pf; //Reference to the pathfinding object.
    private bool isProcessing; //Is a request being processed?

    private static RequestManager inst; //Singleton for the path request manager, we only want one.

    //Called by unity when the game object is first instantiated.
    private void Awake() {
        pathRequests = new Queue<PathRequest>();
        pf = GetComponent<PathFinding>();
        inst = this;
    }

    //Called by an agent when it needs a path.
    public static void RequestPath(Vector3 startPos, Vector3 targetPos, Action<Vector3[], bool> pathCallback, SearchType type) {

        PathRequest req = new PathRequest(startPos, targetPos, pathCallback, type);

        if (inst.pathRequests != null) {
            
            inst.pathRequests.Enqueue(req);
        }

        inst.TryNextProcess();
    }

    //Checks to see if there is a request to be processed.
    private void TryNextProcess() {

        if (!isProcessing && pathRequests.Count > 0) {
            currentRequest = pathRequests.Dequeue();
            isProcessing = true;

            if (currentRequest.type == SearchType.AStar) {
                Debug.Log("Began Search");
                pf.FindPathAStar(currentRequest.startPos, currentRequest.targetPos);
            }
            else if (currentRequest.type == SearchType.BFS) {
                pf.FindPathBFS(currentRequest.startPos, currentRequest.targetPos);
            }
        }
    }

    /// <summary>
    /// Called by the PathFinding class once finished processing a path.
    /// </summary>
    /// <param name="path">The processed path.</param>
    /// <param name="isSuccessful">Was a path found successfully.</param>
    public void FinishedProcessing(Vector3[] path, bool isSuccessful) {
        currentRequest.methodCallback(path, isSuccessful); //Call the call back method in the agents class.
        isProcessing = false;
        TryNextProcess(); //Processing path done, try and do another.
    }

    /// <summary>
    /// Stores the data for a given path and a methodCallback to call once processing is done.
    /// </summary>
    private struct PathRequest {

        public Vector3 startPos; //The starting point(position) for the request.
        public Vector3 targetPos; //The target point(position) for the request.

        //This holds a reference to a method. Any class with the same signature as the Action, can be assigned to the callback.
        public Action<Vector3[], bool> methodCallback; //The method to call once the path has been found.
        public SearchType type; //Is this request for A* or BFS

        /// <summary>
        /// Constructor for the path request data.
        /// </summary>
        /// <param name="startPos">Starting point in 3D space.</param>
        /// <param name="targetPos">Target point in 3D space.</param>
        /// <param name="methodCallback">The method to call once its done processing a path.</param>
        public PathRequest(Vector3 startPos, Vector3 targetPos, Action<Vector3[], bool> methodCallback, SearchType type) {
            this.startPos = startPos;
            this.targetPos = targetPos;
            this.methodCallback = methodCallback;
            this.type = type;
        }
    }

}

//Used to determine what algorithm to use.
public enum SearchType {
    AStar,
    BFS
}
