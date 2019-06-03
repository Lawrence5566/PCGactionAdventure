using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	[Header("Change to change difficulty:")]
	public int levelPointsValue = 3;

	[Header("")]
	public GraphToMapConverter graphToMapConverter;
	public GraphGenerator graphGenerator;
	//public MeshGenerator meshGenerator;
	public MobSpawner mobSpawner;

	public GameObject playerObject;

	public GameObject[] traps; 
	public GameObject doorPrefab;
	public GameObject keyPrefab;
	public GameObject teleporterPrefab;
	public GameObject chestPrefab;

    // Start is called before the first frame update
    void Start()
	{
		graphGenerator.Init (); //initialise most of the generation
		PlacePlayer();
		PlaceMonsters ();
		PlaceTraps(); //place traps
		PlaceDoors(); //place doors
		PlaceKeys();
		PlaceHiddens ();
        PlaceItems();
		PlaceGoal ();

		//when player wins the level, reset and spawn new one with harder enemies?
    }

	public void PlaceMonsters(){
        List<Vector3> locations = new List<Vector3>();

        //get all locations
        foreach (KeyValuePair<Vector3, token> k in graphToMapConverter.monsterAndTrapLocations) {
            if (k.Value.type == "monster")
                locations.Add(k.Key);
        }

        if (graphToMapConverter.goalLocationAndType.Value.type == "boss"){ //if goal is boss
			locations.Add(graphToMapConverter.goalLocationAndType.Key); //add boss location
			mobSpawner.createStack(levelPointsValue,locations, locations.Count-1); //create a stack of 3 points value, monster tasks locations, boss at last index
		} else{
			mobSpawner.createStack(levelPointsValue,locations, -1); //create a stack of 3 points value, monster tasks locations, no boss
		}

	}

	public void PlacePlayer(){
		List<Vector3> roomCenters = graphToMapConverter.roomCenterCoords;
		int startNodeNumber  = System.Array.IndexOf (graphGenerator.nodeArray, graphGenerator.startNode);

		//now spawn player (already in scene so just set position)
		playerObject.transform.position = roomCenters[startNodeNumber];
	}

	public void PlaceTraps(){
        List<KeyValuePair<Vector3, token>> trapLocations = graphToMapConverter.monsterAndTrapLocations.FindAll(x => x.Value.type == "trap");
		//pick random trap from trap array, place on entrances (get entrances locations from graph to map coords?)
		foreach (KeyValuePair < Vector3, token > location in trapLocations){
			Instantiate (traps [Random.Range (0, traps.Length - 1)], location.Key, Quaternion.identity); //create random trap in trap location
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
		List<KeyValuePair<Vector3, token>> keyLocations = graphToMapConverter.ItemLocations.FindAll(x => x.Value.type == "key"); //get all key locations from itemLocations list

		foreach (KeyValuePair<Vector3, token> k in keyLocations){
			GameObject key = Instantiate (keyPrefab, k.Key, Quaternion.identity); 	//spawn key
			key.GetComponent<Key>().keyToken = k.Value;								//give key its token
		}
	}

	public void PlaceHiddens(){
		List<KeyValuePair<Vector3, Vector3>> locations = graphToMapConverter.hiddenLocations;
		foreach (KeyValuePair<Vector3, Vector3> k in locations) {
			GameObject tele = Instantiate (teleporterPrefab, k.Key, Quaternion.identity);
			tele.GetComponent<Teleporter> ().teleLocation = k.Value;
		}
	}

	public void PlaceGoal(){
		KeyValuePair<Vector3, token> goal = graphToMapConverter.goalLocationAndType;

		if (goal.Value.type == "lock") { //if its a lock type goal, you have to get a key for a chest
			GameObject chest = Instantiate (chestPrefab, goal.Key, Quaternion.identity); //spawn chest
			chest.GetComponent<Chest>().keyToken = goal.Value.keyLink;
		}

	}

    public void PlaceItems() {
        List<KeyValuePair<Vector3, token>> items = graphToMapConverter.ItemLocations.FindAll(x => x.Value.type != "key"); //get all non-key locations from itemLocations list


    }
}
