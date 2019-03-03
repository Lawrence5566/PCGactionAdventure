using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

//need to clear up/remove/merge connections, connected nodes and connectednodeforgraph

public class GraphGenerator : MonoBehaviour {
	public GameObject TextBasePrefab;
	public Sprite circle;
	public Sprite connectionSpr;
	public Sprite connectionArrowSpr;
	public Sprite enemyCircle;
	public Sprite lockKeyCircle;
	public Sprite obstacleCircle;
	public Sprite itemCircle;
	public Sprite hiddenCircle;
	node[] nodeArray = new node[12];
	node startNode;

	int maxRouteLength = 8;
		
	void Start () {

		int n = 0;
		for (int x = 0; x < 3; x++) { //3 across and 4 down node array - starts bottom left in world, goes up
			for (int y = 0; y < 4; y++) {
				nodeArray[n] = new node(this.transform.position + new Vector3 (x * 4f, y * 4f, 0f), "node" + n, circle); //space nodes out 3.5 units apart
				n ++;
			}
		}

		n = 0;
		for (int x = 0; x < 3; x++) { //creating starting connections and setting adjacent nodes
			for (int y = 0; y < 4; y++) {
				for (int i = 0; i < nodeArray.Length; i++) { //for each node in array
					var relativePoint = nodeArray[n].obj.transform.InverseTransformPoint(nodeArray[i].obj.transform.position);

					if (relativePoint.x == -4.0 && relativePoint.y == 0) { //has left node (and not above or below)
						nodeArray[n].connections[0] = new connection(nodeArray[n].obj.transform.position + new Vector3(-2f,0f,0f), connectionSpr, 90f); //add left connection
						nodeArray[n].connectedNodes.Add(nodeArray[i]);
						nodeArray[n].connectionToNodes.Add(new KeyValuePair<connection, node>(nodeArray[n].connections[0],nodeArray[i]));
					}
					if (relativePoint.x == 4.0 && relativePoint.y == 0) { //has right node
						nodeArray[n].connections[2] = new connection(nodeArray[n].obj.transform.position + new Vector3(2f,0f,0f), connectionSpr, 90f); //right
						nodeArray[n].connectedNodes.Add(nodeArray[i]);
						nodeArray[n].connectionToNodes.Add(new KeyValuePair<connection, node>(nodeArray[n].connections[2],nodeArray[i]));
					}
					if (relativePoint.y == 4.0 && relativePoint.x == 0) { //has top node (and not right or left)
						nodeArray[n].connections[1] = new connection(nodeArray[n].obj.transform.position + new Vector3(0f,2f,0f), connectionSpr, 0f); //top
						nodeArray[n].connectedNodes.Add(nodeArray[i]);
						nodeArray[n].connectionToNodes.Add(new KeyValuePair<connection, node>(nodeArray[n].connections[1],nodeArray[i]));
					}
					if (relativePoint.y == -4.0 && relativePoint.x == 0) { //has bot node
						nodeArray[n].connections[3] = new connection(nodeArray[n].obj.transform.position + new Vector3(0f,-2f,0f), connectionSpr, 0f); //bot
						nodeArray[n].connectedNodes.Add(nodeArray[i]);
						nodeArray[n].connectionToNodes.Add(new KeyValuePair<connection, node>(nodeArray[n].connections[3],nodeArray[i]));
					}
				}
				n++;
			}
		}

		//create dungeon:
		DungeonRule();

		//pass on to map converter:
		int[,] map = FindObjectOfType<GraphToMapConverter>().CreateMap(nodeArray);
		//generate mesh:
		FindObjectOfType<MeshGenerator>().GenerateMesh(map,1); //squareSize of 1

	}

	List<node> findAPath(node curr, List<node> visited, List<node> p){ //recursivly finds a path to goal
		List<node> path = p;
		visited.Add (curr); //mark current node as visited
		path.Add(curr); 	//add to path

		if (curr.obj.name == "GoalNode") {
			return path; //return path
		} else{ //not at destination
			//recurse for next random node
			//List<node> validNodes = curr.connectedNodes; //this does not work, if you delete from valid nodes, you delete from connectedNodes as this is a reference
			List<node>  validNodes = new List<node> (curr.connectedNodes); //use this instead
			node newNode;

			for (int i = 0; i < 4; i++) { //4 = max number of adjacent nodes

				int index = Random.Range(0, validNodes.Count); 			//pick a node
				newNode = validNodes [index]; 								//set node

				if (!visited.Contains (newNode) && newNode != null) { 		//check if node is valid
					path = findAPath(newNode, visited, path);				//recurse to extend the path
					break; 													//break, we have what we came for!
				} else{
					validNodes.Remove(newNode); 							//remove from valid nodes
				}
					
				if(validNodes.Count == 0){ 									//if no nodes are valid, we got trapped in a corner
					path.Remove(curr);										//remove current node from path, but keep as visited so its ignored
					newNode = path[path.Count - 1];							//set next node to look at to be the previous node (which is currently at the end)
					path.Remove(newNode); 									//remove the new node(previous) from path, so it isn't added again by accident - !important to stop infinte recursion
					visited.Remove(newNode); 								//remove the new node from path, same reason ^^
					path = findAPath(newNode, visited, path);				//recurse to go back to previous node
					break; //to stop the max 4 loops
				}
			}
		}
		return path;
	}

	void findLoopsInGraph(){
		
	}

	KeyValuePair<connection, node> addConnection(node a, node b){ //create connection from A -> B and add to each other's connectedNodes
		var relativePoint = a.obj.transform.InverseTransformPoint(b.obj.transform.position); //for arrow direction
		int relPoint;
		float angle;
		if (relativePoint.x == -4.0) {//b is to the left (0)
			relPoint = 0;
			angle = 270f;
		} else if (relativePoint.x == 4.0) {//right (2)
			relPoint = 2;
			angle = 90f;
		} else if (relativePoint.y == 4.0) {//above (1)
			relPoint = 1;
			angle = 180f;
		} else if (relativePoint.y == -4.0) {//below (3)
			relPoint = 3;
			angle = 0f;
		} else { //no connection
			return new KeyValuePair<connection, node>(new connection(),new node());
		}
			
		connection connectionOld = a.connections[relPoint];
		a.connections[relPoint] = new connection(connectionOld.position, connectionArrowSpr, angle);
		KeyValuePair<connection, node> newConnectionToNode = new KeyValuePair<connection, node> (a.connections [relPoint], b);
		a.connectionToNodes.Add(newConnectionToNode);
		Destroy(connectionOld.obj); 

		if (!a.connectedNodes.Contains (b)) {
			a.connectedNodes.Add (b); //make nodes adjacent, if they were not already
		}
		if (!b.connectedNodes.Contains (a)) {
			b.connectedNodes.Add(a);
		}

		return newConnectionToNode;
	}

	int DetermineRouteType(List<node> route){
		int maxRouteLength = nodeArray.GetLength (0);

		//short route is on or below 1/4 of max route length (max number of nodes)
		if (route.Count <= 0.25 * maxRouteLength) {
			return 1; //1 = short
		} else { 
			return 2; //2 = long
		}

	}


	// graph grammer functions: 

	//Dungeon > Rooms + Goal
	void DungeonRule(){
		//set start symbol - picks random location
		startNode = nodeArray[Random.Range(0,11)];
		GameObject text = Instantiate(TextBasePrefab,  startNode.obj.transform);
		startNode.obj.name = "StartNode";
		text.GetComponent<TextMesh>().text = "Start";

		//fire 'goal' function
		GoalRule();
		
	}

	void GoalRule(){
		//create goal location
		GameObject goalNode;
		do { //keep picking different spots till you get one that isnt the start node
			goalNode = nodeArray [Random.Range (0, 11)].obj;
		} while (goalNode.name == "StartNode");
		GameObject text = Instantiate(TextBasePrefab,  goalNode.transform);
		goalNode.name = "GoalNode";
		text.GetComponent<TextMesh>().text = "Goal";

		//generate 2 paths between them
		//find a path between start and goal:

		List<node> routeA = findAPath (startNode, new List<node> (), new List<node> ());
		List<node> routeB = findAPath (startNode, new List<node> (), new List<node> ());

		while (true){ 
			if (routeA.Count > maxRouteLength) { //make sure route is not too long
				routeA = findAPath (startNode, new List<node> (), new List<node> ());
				continue;
			} 
			if (routeB.Count > maxRouteLength) {
				routeB = findAPath (startNode, new List<node> (), new List<node> ());
				continue;
			}
			if (Enumerable.SequenceEqual (routeA, routeB)) {//make sure routes are different
				routeB = findAPath (startNode, new List<node> (), new List<node> ());
				continue;
			}

			break;
		}

		
		do { //make sure routes are different
			routeA = findAPath (startNode, new List<node> (), new List<node> ());
			routeB = findAPath (startNode, new List<node> (), new List<node> ());
		} while (Enumerable.SequenceEqual(routeA, routeB));

		//log paths for testing:
		string s1 = "";
		string s2 = "";
		foreach (node n in routeA) {
			s1 += (n.name + ", ");
		}
		foreach (node n in routeB) {
			s2 += (n.name + ", ");
		}
		Debug.Log("path1: " + s1);
		Debug.Log("path2: " + s2);

		//do this only when passing on to map converter: (it eliminates unneccisary nodes)
		ProcessNodeArray (routeA, routeB);

		//decide goal (for this game its just a boss + item)
		//create boss token, create item token, generate item + enemy stacks

	}

	/*
	void GrowRoute(List<KeyValuePair<connection,node>> route){ //testing
		//rooms > rooms + room
		//this will add a connection to a room, but not add the room to the route (so it'll be a side room?)
		System.Random r = new System.Random();
		foreach (int i in Enumerable.Range(1, route.Count - 1).OrderBy(x => r.Next())){ //pick random node on route that isn't start or end
			node room = route[i].Value; 
			List<KeyValuePair<connection,node>> posibleRooms = new List<KeyValuePair<connection,node>>();
			foreach (KeyValuePair<connection,node> k in room.connectionToNodes){
				if (!route.Contains (k)) { //if its not on the route, add it to posibles
					posibleRooms.Add(k);
				}
			}
			if (posibleRooms.Count == 0) {
				continue;//try a different room
			}

			addConnection (room, posibleRooms [Random.Range (0, posibleRooms.Count)].Value); //pick a random room from posibles and add a route connection to it
			break; //found a room we can use, stop the loop
		}

		//Rooms > Rooms + room
		//if (room.connectedNodes.Count == 0){
		//	return; //no free rooms to expand to
		//} else{
		//	addConnection (room, room.connectedNodes [Random.Range (0, room.connectedNodes.Count)]); //pick a random room and add a route connection to it
		//}
		//Rooms > Rooms + room + Obstacle
		//Rooms > Rooms + room + item

	}
	*/

	void ProcessNodeArray(List<node> routeA, List<node> routeB){ //final prep processing for converting GraphToMap 
		//destroy all connections
		for (int i = 0; i < nodeArray.Length; i++) {  //foreach nodes
			nodeArray [i].connectedNodes.Clear ();
			nodeArray [i].connectionToNodes.Clear ();
			for (int j = 0; j < 4; j++) {			//foreach connection
				if (nodeArray [i].connections [j] != null) {
					Destroy (nodeArray [i].connections [j].obj);
				}
			}
		}

		//reconnect along routes
		//add connections for route & converting to KeyValuePairs
		List<KeyValuePair<connection, node>> RouteA = new List<KeyValuePair<connection, node>> ();
		List<KeyValuePair<connection, node>> RouteB = new List<KeyValuePair<connection, node>> ();
		for (int i = 1; i < routeA.Count; i++) { //start from one ahead, so that we don't go out of range
			RouteA.Add(addConnection (routeA [i - 1], routeA [i])); //add connection between nodes, and return key value pair for each to new route list
		}
		for (int i = 1; i < routeB.Count; i++) { 
			RouteB.Add(addConnection (routeB [i - 1], routeB [i]));
		}

		int ACount = routeA.Count;
		int BCount = routeB.Count;
		if (ACount > 3 && BCount > 3) { //both routes are long
			TwoAlternativePaths(RouteA,RouteB); //alternative paths rule
		} else if (ACount > 3 && BCount <= 3) { //A is long, B is short
			HiddenShortcut(RouteA,RouteB); //add hidden shortcut to short route
		} else if (ACount <= 3 && BCount > 3) { //A is short, B is long
			HiddenShortcut(RouteB, RouteA); 
		} else { //both are short
			
		}

		routeA [0].AddFeature (new token("obstacle", obstacleCircle)); //test add
		routeA [0].AddFeature (new token("obstacle", obstacleCircle)); //test add
	}

	connection getRandomUniqueConnection(List<KeyValuePair<connection, node>> routeA, List<KeyValuePair<connection, node>> routeB){ //gets a random unique connection from routeA by comparing to routeB
		List<KeyValuePair<connection, node>> routeAunique = new List<KeyValuePair<connection, node>>();
		foreach (KeyValuePair<connection, node> k1 in routeA){ //get list of unique key value pairs by connection
			bool isEqual = false;
			foreach (KeyValuePair<connection, node> k2 in routeB){
				if (k1.Key.position == k2.Key.position) {								//if equal, skip to next pair in routeA
					isEqual = true;
					break;
				}
			}
			//got to the end, check if it was equal
			if (!routeAunique.Contains (k1) && !isEqual) {				//if not already in unique list, and not equal to any
				routeAunique.Add (k1);
			}
		}
			
		if (routeAunique.Count == 0){
			Debug.Log("no unique connection found");
			return new connection ();//if empty, return null object
		}

		return routeAunique [Random.Range (0, routeAunique.Count)].Key; //return random chosen connection
	}

	node getRandomUniqueNode(List<KeyValuePair<connection, node>> routeA, List<KeyValuePair<connection, node>> routeB){ //gets a random unique node from routeA by comparing to routeB
		List<KeyValuePair<connection, node>> routeAunique = new List<KeyValuePair<connection, node>>();

		foreach (KeyValuePair<connection, node> k1 in routeA){ //get list of unique key value pairs by node
			bool isEqual = false;
			foreach (KeyValuePair<connection, node> k2 in routeB){
				if (k1.Value == k2.Value) {								//if equal, skip to next pair in routeA
					isEqual = true;
					break;
				}
			}
			//got to the end, check if it was equal
			if (!routeAunique.Contains (k1) && !isEqual) {				//if not already in unique list, and not equal to any
				routeAunique.Add (k1);
			}
		}

		if (routeAunique.Count == 0){
			Debug.Log("no unique node found");
			return new node ();//if empty, return null object
		}

		return routeAunique [Random.Range (0, routeAunique.Count)].Value; //return random chosen node
	}


	//pattern rules:
	void TwoAlternativePaths(List<KeyValuePair<connection, node>> routeA, List<KeyValuePair<connection, node>> routeB){ //only ran on both long paths
		Debug.Log("run Alternate Paths rule");

		node uniqueNode = getRandomUniqueNode (routeA, routeB);
		if (uniqueNode.name == "null node") { // if no nodes unique, add to connction instead
			//place obstacle on one route(monster)
			getRandomUniqueConnection(routeA,routeB).AddFeatureToConnection (new token ("obstacle", obstacleCircle)); 
		} else {								//node is unique, so add monster
			//place obstacle on one node(monster)
			uniqueNode.AddFeature(new token("obstacle", obstacleCircle)); //add a obstacle
		}
			
		getRandomUniqueConnection(routeB, routeA).AddFeatureToConnection(new token("obstacle", obstacleCircle)); 				//add a obstacle on a connection (trap)
	}

	void HiddenShortcut(List<KeyValuePair<connection, node>> longRoute, List<KeyValuePair<connection, node>> shortRoute){
		Debug.Log ("run Hidden Shortcut rule");
		getRandomUniqueConnection(shortRoute,longRoute).AddFeatureToConnection(new token("hidden", hiddenCircle)); 
	
	}
		

}

//utlilty classes used for this generator and other parts of the project

public class node{
	public List<KeyValuePair<connection, node>> connectionToNodes = new List<KeyValuePair<connection, node>>(); //connects connections to connected nodes - might be able to just use this?

	public string name;
	public connection[] connections = new connection[4]; //array of 4 connections: left, top, right, bot
	public GameObject obj;
	public List<node> connectedNodes = new List<node>();
	private List<token> features = new List<token> ();

	public node(){
		name = "null node";
		obj = new GameObject ("null node");
	}

	public node(Vector3 pos, string n, Sprite i){
		name = n;

		//create node in world
		obj = new GameObject(name);		
		obj.transform.position = pos;
		SpriteRenderer ren = obj.AddComponent<SpriteRenderer>();	
		ren.sprite = i;
	}

	public void AddFeature(token newToken){
		features.Add (newToken); //add new node to features

		//destroy all features from node, re-add them around node incl. new one
		float angle = 360 / features.Count;
		for (int i = 0; i < features.Count; i++){
			if (features [i].obj != null) {
				GameObject.Destroy (features[i].obj); 	//destroy old object
			}

			float curAngle = angle*i; 				//calculate new object position
			Vector3 pos = new Vector3(0f,0f,0f);
			pos.x = obj.transform.position.x + .8f * Mathf.Sin(curAngle * Mathf.Deg2Rad); 	//harcoded number is radius
			pos.y = obj.transform.position.y + .8f * Mathf.Cos(curAngle * Mathf.Deg2Rad);
			pos.z = obj.transform.position.z;

			features[i].obj = new GameObject(features[i].type);
			features[i].obj.transform.position = pos;
			features [i].obj.transform.localScale = new Vector3(.5f,.5f,.5f);
			features [i].obj.transform.SetParent (obj.transform); //set parent to node
			SpriteRenderer ren = features[i].obj.AddComponent<SpriteRenderer>();	
			ren.sprite = features[i].sprite;
			ren.sortingOrder = 2;  //set order in layer infront of parent
		}
		features.Add (newToken);
	}

}

public class token{
	public string type;
	public Sprite sprite;
	public GameObject obj;

	public token(string t, Sprite s){
		type = t;
		sprite = s;
	}
}

public class connection{ //may not need
	public Vector3 position;
	public GameObject obj;
	private List<token> features = new List<token> ();

	public connection(){ //empty constructor = predictable null object
		position = new Vector3();
		obj = new GameObject("null connection");
	}

	public connection(Vector3 pos, Sprite i, float rot){
		position = pos;

		//create connection in world
		obj = new GameObject("connection");	
		obj.transform.position = position;
		obj.transform.Rotate(new Vector3(0f,0f,rot));
		SpriteRenderer ren = obj.AddComponent<SpriteRenderer>();	
		ren.sprite = i;
	}

	public void AddFeatureToConnection(token newToken){ //for adding to connection
		newToken.obj = new GameObject(newToken.type);
		newToken.obj.transform.position = position;
		newToken.obj.transform.localScale = new Vector3(.5f,.5f,.5f);
		newToken.obj.transform.SetParent (obj.transform); //set parent to connection
		SpriteRenderer ren = newToken.obj.AddComponent<SpriteRenderer>();	
		ren.sprite = newToken.sprite;
		ren.sortingOrder = 2;  							//set order in layer infront of parent
		features.Add (newToken); 				//add new node to list of connection features
	}
		
}
