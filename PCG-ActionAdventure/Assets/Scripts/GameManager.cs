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

	public GameObject[] traps; 

    // Start is called before the first frame update
    void Start()
    {
		graphGenerator.Init (); //initialise most of the generation
		PlacePlayer();
		mobSpawner.createStack(3,3,0); //create a stack of 3 points value, 3 monster tasks, no boss (just for testing for now) 
		PlaceTraps(); //place traps
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void PlacePlayer(){
		List<Vector3> roomCenters = graphToMapConverter.roomCenterCoords;
		int startNodeNumber  = System.Array.IndexOf (graphGenerator.nodeArray, graphGenerator.startNode);

		//now spawn player (already in scene so just set position)
		playerObject.transform.position = roomCenters[startNodeNumber];
	}

	public void PlaceTraps(){
		List<Vector3> trapLocations = graphToMapConverter.trapLocations;
		//pick random trap from trap array, place on entrances (get entrances locations from graph to map coords?)
		foreach (Vector3 location in trapLocations){
			Instantiate (traps [Random.Range (0, traps.Length - 1)], location, Quaternion.identity); //create random trap in trap location
		}


	}
}
