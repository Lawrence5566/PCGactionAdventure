﻿using System.Collections;
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
	public GameObject doorPrefab;
	public GameObject keyPrefab;

    // Start is called before the first frame update
    void Start()
    {
		graphGenerator.Init (); //initialise most of the generation
		PlacePlayer();
		PlaceMonsters ();
		PlaceTraps(); //place traps
		PlaceDoors(); //place doors
		PlaceKeys();

		//when player wins the level, reset and spawn new one with harder enemies?
    }

	public void PlaceMonsters(){
		//get all locations
		List<Vector3> locations = graphToMapConverter.monsterLocations;
		mobSpawner.createStack(6,locations,0); //create a stack of 3 points value, monster tasks locations, no boss (just for testing for now) 

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

	public void PlaceDoors(){
		List<KeyValuePair<Vector3[], token>> DoorLocations = graphToMapConverter.DoorLocations;

		foreach (KeyValuePair<Vector3[], token> k in DoorLocations){
			GameObject door = Instantiate (doorPrefab, k.Key[0], Quaternion.identity); //spawn door

			door.transform.LookAt(k.Key[1]); //rotate door to face entrance
			//remove x and z rotations
			door.transform.eulerAngles = new Vector3(0, door.transform.eulerAngles.y, 0);

			door.GetComponent<Door>().keyToken = DoorLocations [0].Value; //set key token for detecting which key opens which door
		}
	}

	public void PlaceKeys(){
		List<KeyValuePair<Vector3, token>> keyLocations = graphToMapConverter.keyLocations;

		foreach (KeyValuePair<Vector3, token> k in keyLocations){
			GameObject key = Instantiate (keyPrefab, k.Key, Quaternion.identity); 	//spawn key
			key.GetComponent<Key>().keyToken = k.Value;								//give key its token
		}
	}

}
