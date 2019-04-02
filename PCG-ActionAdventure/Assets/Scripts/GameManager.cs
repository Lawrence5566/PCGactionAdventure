using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public GraphToMapConverter graphToMapConverter;
	public GraphGenerator graphGenerator;
	public MeshGenerator meshGenerator;
	public MobSpawner mobSpawner;

	public GameObject playerObject;

    // Start is called before the first frame update
    void Start()
    {
		graphGenerator.Init (); //initialise most of the generation
		PlacePlayer();
		mobSpawner.createStack(3,3,0); //create a stack of 3 points value, 3 monster tasks, no boss (just for testing for now) 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void PlacePlayer(){
		List<Vector3> roomCenters = graphToMapConverter.roomCenterCoords;
		int startNodeNumber  = System.Array.IndexOf (graphGenerator.nodeArray, graphGenerator.startNode);
		Debug.Log (startNodeNumber);

		//now spawn player
		//instantiate (playerPrefab)
		playerObject.transform.position = roomCenters[startNodeNumber];
	}
}
