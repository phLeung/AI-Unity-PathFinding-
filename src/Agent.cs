using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour {

    public float moveSpeed = 15f; //The speed the agent will move at.

    private Vector3[] path; //The path the agent will follow.
    private int index; //The current index of the path array.

    //Below here are variables for a cycle and anything related.

    private List<GameObject> collectables; //The collectables will be target points for the agent to find a path to.
    private int collectablesIndex; //Keep track of the collectable we need to find.
    private Action<CycleData> LogDataCallback; //Method to call once a cycle is complete.
    

    //Temp variables used for each cycle.
    private float startTimeOfCycle;
    private float endTimeOfCycle;
    private float totalTimeThisCycle;
    private float totalDistanceThisCycle; //The total distance the agent traveled this cycle.
    private int cyclesIndex; //What cycle is the agent on. The GameManager will keep track of this. The agent only needs it to store in a CycleData object.
    private int totalNodesTraversed;

    private void Start() {
        totalNodesTraversed = 0;
        collectablesIndex = 0;

    }

    //This will be called from the game manager once all collectables are spawned.
    public void InitialCycleAStar(List<GameObject> collectables, int cyclesIndex, Action<CycleData> LogDataCallback, SearchType type) {
        collectablesIndex = 0;
        totalNodesTraversed = 0;
        this.collectables = collectables;
        this.LogDataCallback = LogDataCallback;
        this.cyclesIndex = cyclesIndex;

        startTimeOfCycle = Time.unscaledTime; //Store the start time.
        RequestManager.RequestPath(this.transform.position, collectables[0].transform.position, PathFoundCallback, SearchType.AStar);
    }

    public void NextCycle(List<GameObject> collectables, int cyclesIndex) {
        this.collectables = collectables;
        this.cyclesIndex = cyclesIndex;
        collectablesIndex = 0;
        totalNodesTraversed = 0;
        startTimeOfCycle = Time.unscaledTime;

        RequestManager.RequestPath(this.transform.position, collectables[0].transform.position, PathFoundCallback, SearchType.AStar);
    }

    private void CycleComplete() {
        endTimeOfCycle = Time.unscaledTime;
        totalTimeThisCycle = endTimeOfCycle - startTimeOfCycle;

        CycleData data = new CycleData(cyclesIndex, totalDistanceThisCycle, totalTimeThisCycle, totalNodesTraversed);
        LogDataCallback(data);

    }

    private void MoveToNextCollectable() {
        
        totalNodesTraversed += path.Length;
        totalDistanceThisCycle += GetTotalDistance();

        collectablesIndex++;

        if (collectablesIndex >= collectables.Count) {
            Debug.Log("Cycle complete.");
            CycleComplete();
        }
        else {
            Debug.Log("Collectables Index: " + collectablesIndex + " Collectables Count: " + collectables.Count);
            RequestManager.RequestPath(this.transform.position, collectables[collectablesIndex].transform.position,
                PathFoundCallback, SearchType.AStar);
        }
    }


    //DONT USE THIS! BFS is broken dont know why cant figure it out.
    //Tried multiple implementations.
    public void BegineSearchBFS(List<GameObject> collectables, Action<float> LogTimeBFS, SearchType type) {
        collectablesIndex = 0;
        this.collectables = collectables;
       // RequestManager.RequestPath(this.transform.position, testTarget.position, PathFoundCallback, SearchType.BFS);
    }


    //The call back once a path is found.
    public void PathFoundCallback(Vector3[] path, bool success) {


        if (success) {
            this.path = path;
            index = 0;

            StopCoroutine(FollowPath()); //Make sure the coroutine is not running before calling start.
            StartCoroutine(FollowPath());
        }
    }

    //Makes agent follow path.
    IEnumerator FollowPath() {

        //Get the first point in the path.
        Vector3 curPoint = path[0];

        //While the coroutine is running continue this loop.
        while (true) {
            if (transform.position == curPoint) {
                index++;

                //Dont want index out of bounds exception.
                if (index >= path.Length) {
                    MoveToNextCollectable();
                    yield break;
                }

                curPoint = path[index];
            }
            //Move the agent towards the next point. Anything with moving/physics multiply by Time.deltaTime!!
            transform.position = Vector3.MoveTowards(transform.position, curPoint, moveSpeed * Time.deltaTime);

            //Wait one frame.
            yield return null;
        }

    }

    //Adds up all the distances between each node the agent moved to.
    private float GetTotalDistance() {

        float total = 0;

        total = Vector3.Distance(path[path.Length - 1], path[path.Length - 2]) * path.Length;
        //Since the distance between each node is the same, find the distance then multiply it by how many nodes the agent traversed.
 
        return total;
    }

    //Method called by unity when game object is disabled or application is closed.
    private void OnDisable() {
        LogDataCallback = null;
    }

}
