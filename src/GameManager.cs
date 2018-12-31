using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public Agent agent; //A reference to the agent.

    public GameObject collectablePrefab; //The game object that represents the collectable.

    public List<GameObject> agentCollectables; //The objects the agent will collect.

    private PathFindingGrid grid; //Need a reference to the grid to check if a collectable was spawned in a location where the agent can get to.

    private int searchCycles = 3; //How many times should the agent collect all the collectables.
    //With each cycle the collectables are spawned at random points.

    private int collectablesToSpawn = 5; //How many collectables should the agent have to find for each entire search cycle.

    private int cycleIndex;

    private List<CycleData> cyclesData;

    //UI Related
    public List<UIDisplayData> dataDisplay;

	// Use this for initialization
	private void Start () {
	    cycleIndex = 0;
	    cyclesData = new List<CycleData>();
	    agentCollectables = new List<GameObject>();
	    grid = GetComponent<PathFindingGrid>(); //Get the reference.

	    Invoke("InitializeCollectables", 1f); //Call this method in 2 seconds. Ensures all things are setup first.
        //There is a better way to do this but for our case this is fine.
	    Invoke("LookForPath", 1f);
    }

    public void LookForPath() {
        agent.InitialCycleAStar(agentCollectables, cycleIndex, LogDataCallback, SearchType.AStar);
    }

    //Spawn collectables at new positions ensuring its a valid location the agent can move to.
    private void SpawnCollectable() {

        Vector3 spawnPoint = Vector3.zero;
        bool isValidPosition = false;

        for (int i = 0; i < collectablesToSpawn; i++) {
            spawnPoint = RandomPosition();

            Node tmp = grid.NodeFromWorldPosition(spawnPoint); //Get a reference to the node at the spawn point.

            if (tmp.walkable != true) {
                while (!isValidPosition) {
                    spawnPoint = RandomPosition();
                    tmp = grid.NodeFromWorldPosition(spawnPoint);
                    if (tmp.walkable) {
                        isValidPosition = true;
                        agentCollectables[i].gameObject.SetActive(true);
                        agentCollectables[i].transform.position = spawnPoint;
                    }
                }
            }
            else {
                agentCollectables[i].gameObject.SetActive(true);
                agentCollectables[i].transform.position = spawnPoint; //Add(Instantiate(collectablePrefab, RandomPosition(), Quaternion.identity));
            }

        }
    }

    //Basically the same as spawn collectables method except that we instantiate them but in spawn collectables we just reuse the objects and change their position.
    private void InitializeCollectables() {
        Vector3 spawnPoint = Vector3.zero;
        bool isValidPosition = false;

        for (int i = 0; i < collectablesToSpawn; i++) {
            spawnPoint = RandomPosition();

            Node tmp = grid.NodeFromWorldPosition(spawnPoint); //Get a reference to the node at the spawn point.

            if (tmp.walkable != true) {
                while (!isValidPosition) {
                    spawnPoint = RandomPosition();
                    tmp = grid.NodeFromWorldPosition(spawnPoint);
                    if (tmp.walkable) {
                        isValidPosition = true;
                        agentCollectables.Add(Instantiate(collectablePrefab, RandomPosition(), Quaternion.identity));
                    }
                }
            }
            else {
                agentCollectables.Add(Instantiate(collectablePrefab, RandomPosition(), Quaternion.identity));
            }

        }
    }


    //Add the data to the array.
    public void LogDataCallback(CycleData data) {
        cyclesData.Add(data);
        cycleIndex++;

        if (cycleIndex >= searchCycles) {
            AllCyclesComplete();
        }
        else {
            SpawnCollectable();
            agent.NextCycle(agentCollectables, cycleIndex);
        }

    }

    private void AllCyclesComplete() {
        Debug.Log("All cycles complete.");

        for (int i = 0; i < dataDisplay.Count; i++) {
            dataDisplay[i].totalDistance.text = "Total Distance: " + cyclesData[i].totalDistance.ToString();
            dataDisplay[i].totalTime.text = "Total Time: " + cyclesData[i].totalTime.ToString();
            dataDisplay[i].totalNodes.text = "Total Nodes: " + cyclesData[i].nodesTraversed.ToString();
            Debug.Log(cyclesData[i].totalTime);
        }

    }

    /// <summary>
    /// Returns a new Vector3 used for a new random position in the world.
    /// </summary>
    /// <returns></returns>
    private Vector3 RandomPosition() {
        return new Vector3((int)Random.Range(-24f, 24f), 1f, (int)Random.Range(-24f, 24f));
    }

}

public struct CycleData {
    public int cycleIndex;
    public float totalDistance; //Total distance the agent traveled this cycle.
    public float totalTime; //Total time it took to complete this cycle.
    public int nodesTraversed; //The total amount of nodes that were traversed.
    //The total nodes traversed will have a direct correlation with the total distance.

    public CycleData(int cycleIndex, float totalDistance, float totalTime, int nodesTraversed) {
        this.cycleIndex = cycleIndex;
        this.totalDistance = totalDistance;
        this.totalTime = totalTime;
        this.nodesTraversed = nodesTraversed;
    }
}
