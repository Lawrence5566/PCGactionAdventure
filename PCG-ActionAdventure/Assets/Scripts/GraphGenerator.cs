using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

//need to clear up/remove/merge connections, connected nodes and connectednodeforgraph
//try to remove 'connections' keyvalues are more useful

public class GraphGenerator : MonoBehaviour {
	public GameObject TextBasePrefab;
	public Sprite circle;
	public Sprite connectionSpr;
	public Sprite connectionBlockedSpr;
	public Sprite connectionArrowSpr;
	public Sprite connectionDramatic;
	public Sprite connectionCollapse;
	public Sprite enemyCircle;
	public Sprite lockCircle;
	public Sprite keyCircle;
    public Sprite monsterCircle;
    public Sprite trapCircle;

    public Sprite itemCircle;
	public Sprite hiddenCircle;
	node[] nodeArray = new node[12];
	node startNode;
	node goalNode;

	int maxRouteLength = 8;

	node[] dramaticCycleNodes = new node[2];
		
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
						connection newCon = new connection(nodeArray[n].obj.transform.position + new Vector3(-2f,0f,0f), connectionSpr, 90f); //add left connection
						nodeArray[n].connectedNodes.Add(nodeArray[i]);
						nodeArray[n].connectionToNodes.Add(new KeyValuePair<connection, node>(newCon ,nodeArray[i]));
					}
					if (relativePoint.x == 4.0 && relativePoint.y == 0) { //has right node
						connection newCon = new connection(nodeArray[n].obj.transform.position + new Vector3(2f,0f,0f), connectionSpr, 90f); //right
						nodeArray[n].connectedNodes.Add(nodeArray[i]);
						nodeArray[n].connectionToNodes.Add(new KeyValuePair<connection, node>(newCon ,nodeArray[i]));
					}
					if (relativePoint.y == 4.0 && relativePoint.x == 0) { //has top node (and not right or left)
						connection newCon = new connection(nodeArray[n].obj.transform.position + new Vector3(0f,2f,0f), connectionSpr, 0f); //top
						nodeArray[n].connectedNodes.Add(nodeArray[i]);
						nodeArray[n].connectionToNodes.Add(new KeyValuePair<connection, node>(newCon,nodeArray[i]));
					}
					if (relativePoint.y == -4.0 && relativePoint.x == 0) { //has bot node
						connection newCon = new connection(nodeArray[n].obj.transform.position + new Vector3(0f,-2f,0f), connectionSpr, 0f); //bot
						nodeArray[n].connectedNodes.Add(nodeArray[i]);
						nodeArray[n].connectionToNodes.Add(new KeyValuePair<connection, node>(newCon,nodeArray[i]));
					}
				}
				n++;
			}
		}

		//create dungeon:
		DungeonRule();

		//pass on to map converter:
		int[,] map = FindObjectOfType<GraphToMapConverter>().CreateMap(nodeArray, dramaticCycleNodes);

		//generate mesh from nodeArray:
		FindObjectOfType<MeshGenerator>().GenerateMesh(map, 1, false); //squareSize of 1

		//generate floor mesh - works by flipping map bits and generating like a wall but without sides

		//now take any '2' bits (bits that are gunna be dramatic cycle stuff) and turn them into 1s (these will be 0 in floor mesh)

		FindObjectOfType<MeshGenerator>().GenerateMesh(map, 1, true);  //generate floor mesh

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
		
	List<List<node>> findLoopsInGraph(node curr, node previous, List<node> path, List<node> deadEndNodes, int activeNodes, List<List<node>> loops){
		//find loops in graph by looking through nodes connected to first past in 'curr' node

		if (!path.Contains(curr)) //only if path doesn't already contain current
			path.Add (curr);	

		//recurse for next random node
		List<node>  validNodes = new List<node> (curr.connectedNodes);

		validNodes.Remove (previous);							//remove previous node
		foreach(node n in deadEndNodes) validNodes.Remove(n); 	//remove dead end nodes

		int validNodesCount = validNodes.Count;
		node newNode;

		for (int i = 0; i < validNodesCount; i++) {
			int index = Random.Range(0, validNodes.Count); 			//pick a node from connections
			newNode = validNodes [index]; 								//set node

			if (!path.Contains (newNode)) { //if node is not visited, recur
				loops = findLoopsInGraph (newNode, curr, path, deadEndNodes, activeNodes, loops);
				break; 
			} else if (newNode != previous) { //visited before (since its in path, and its not the previous)
				//found loop

				int indexNewNode = path.IndexOf (newNode); //find previous instance of this newNode in the path
				//start from first branch off (first instance of newNode) and remove all nodes before
				for (int n = indexNewNode; n >= 0; n-- ) {
					if (path [n] == newNode) //skip the branch node
						continue;
					path.RemoveAt(n);
				}

                loops.Add (new List<node>(path)); //add path to loops by copying (not reference)

				validNodes.Remove(newNode); //remove from valid nodes so we can look for a new path
			}
				
		}

		if(validNodes.Count == 0){ 	//hit a dead end, no loops here, go back to node previous node to try find a loop
			if (deadEndNodes.Count >= activeNodes){ //looked through all possible nodes
				return loops;
			}

			path.Remove(curr);										//remove current node from path, and add to dead end nodes
			deadEndNodes.Add(curr);

			node newPrevious = null;
			if (path.Count > 1)
				newPrevious = path [path.Count - 2];

			loops = findLoopsInGraph(path[path.Count - 1], newPrevious, path, deadEndNodes, activeNodes, loops);				//recurse to go back to previous node
		}

		return loops;

		//remove duplicates when outputting (as some can be made cus of dead end stuff)
			
	}

	KeyValuePair<connection, node> addConnection(node a, node b, Sprite sprite){ //create connection from A -> B and add to each other's connectedNodes
		var relativePoint = a.obj.transform.InverseTransformPoint(b.obj.transform.position); //for arrow direction
		Vector2 relPoint = new Vector2(relativePoint.x, relativePoint.y);
		float angle = 0f;

		//right and left is flipped from expected
		if (relPoint == new Vector2 (0f, 4f)) { //B is above so 0*, top 
			angle = 0f;
		} else if (relPoint == new Vector2 (4f, 4f)) { //top right
			angle = -45f;
		} else if (relPoint == new Vector2 (4f, 0f)) { //right
			angle = -90f;
		} else if (relPoint == new Vector2 (4f, -4f)) { //bottom right
			angle = -135f;
		} else if (relPoint == new Vector2 (0f, -4f)) { //bottom
			angle = 180f;
		} else if (relPoint == new Vector2 (-4f, -4f)) { //bottom left
			angle = 135f;
		} else if (relPoint == new Vector2 (-4f, 0f)) { //left
			angle = 90f;
		} else if (relPoint == new Vector2 (-4f, 4f)) { //top left
			angle = 45f;
		}

		//get old connection if there is one, to destroy it
		connection oldCon = a.connectionToNodes.Find (x => x.Value == b).Key; 	//find connection that connects to B
		if (oldCon != null) Destroy(oldCon.obj);

		//create new one
		connection newCon = new connection(new Vector3(relPoint.x/2, relPoint.y/2, 0f) + a.obj.transform.position, sprite, angle);

		//add to connectionsToNodes
		KeyValuePair<connection, node> newConnectionToNode = new KeyValuePair<connection, node> (newCon, b);
		a.connectionToNodes.Add(newConnectionToNode);

		if (!a.connectedNodes.Contains (b)) {
			a.connectedNodes.Add (b); //make nodes adjacent, if they were not already
		}
		if (!b.connectedNodes.Contains (a)) {
			b.connectedNodes.Add(a);
		}

		return newConnectionToNode;
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
		//GameObject goalNode;
		do { //keep picking different spots till you get one that isnt the start node
			goalNode = nodeArray [Random.Range (0, 11)];
		} while (goalNode.obj.name == "StartNode");
		GameObject text = Instantiate(TextBasePrefab,  goalNode.obj.transform);
		goalNode.obj.name = "GoalNode";
		text.GetComponent<TextMesh>().text = "Goal";

		//generate 2 paths between them
		//find a path between start and goal:

		List<node> routeA = findAPath (startNode, new List<node> (), new List<node> ());
		List<node> routeB = findAPath (startNode, new List<node> (), new List<node> ());

		//make sure routes are not too long, and not the same
		while (true){ 
			if (routeA.Count > maxRouteLength) { 
				routeA = findAPath (startNode, new List<node> (), new List<node> ());
				continue;
			} 
			if (routeB.Count > maxRouteLength) {
				routeB = findAPath (startNode, new List<node> (), new List<node> ());
				continue;
			}
			if (Enumerable.SequenceEqual (routeA, routeB)) {
				routeB = findAPath (startNode, new List<node> (), new List<node> ());
				continue;
			}
			break;
		}
			
		//log paths for testing:
		string s1 = "";
		string s2 = "";
		foreach (node n in routeA) {
			s1 += (n.name + ", ");
		}
		foreach (node n in routeB) {
			s2 += (n.name + ", ");
		}
		Debug.Log("routeA: " + s1);
		Debug.Log("routeB: " + s2);

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
		//destroy all connections - what was point in having them in the first place?
		for (int i = 0; i < nodeArray.Length; i++) {  //foreach node
			nodeArray [i].connectedNodes.Clear ();
			foreach (KeyValuePair<connection, node> k in nodeArray [i].connectionToNodes) {	//foreach connection
				Destroy(k.Key.obj); //destroy connection object
			}
			nodeArray [i].connectionToNodes.Clear ();
		}

		//reconnect along routes
		//add connections for route & converting to KeyValuePairs
        //keep routeA and routeB solo nodes as we use them later
		List<KeyValuePair<connection, node>> RouteA = new List<KeyValuePair<connection, node>> ();
		List<KeyValuePair<connection, node>> RouteB = new List<KeyValuePair<connection, node>> ();
		for (int i = 1; i < routeA.Count; i++) { //start from one ahead, so that we don't go out of range
			RouteA.Add(addConnection (routeA [i - 1], routeA [i], connectionArrowSpr)); //add connection between nodes, and return key value pair for each to new route list
		}
		for (int i = 1; i < routeB.Count; i++) { 
			RouteB.Add(addConnection (routeB [i - 1], routeB [i], connectionArrowSpr));
		}

		RouteA.Insert (0, new KeyValuePair<connection, node> (new connection (), startNode)); //add start node to beginning of routes
		RouteB.Insert (0, new KeyValuePair<connection, node> (new connection (), startNode)); //(this is for loop stuff later)

		//routeA [0].AddFeature (new token("obstacle", obstacleCircle)); //test add
		//routeA [0].AddFeature (new token("obstacle", obstacleCircle)); //test add

		int activeNodes = 0;
		foreach (node n in nodeArray) { //calculate number of nodes in graph that are active
			if (n.connectionToNodes.Count > 0)
				activeNodes++;
		}

		List<List<node>> loops = findLoopsInGraph (startNode, null, new List<node> (), new List<node> (), activeNodes, new List<List<node>> ());

        foreach (List<node> loop in loops) {
        
            List<KeyValuePair<connection, node>> loopRouteA = new List<KeyValuePair<connection, node>>();
            List<KeyValuePair<connection, node>> loopRouteB = new List<KeyValuePair<connection, node>>();

            foreach (KeyValuePair<connection, node> k in RouteA) {  //foreach connection in RouteA
                if (loop.Contains(k.Value)){                        //if loop contains node from that connection
                    loopRouteA.Add(k);                              //add to connection to RouteA half of loop
                }
            }

            foreach (KeyValuePair<connection, node> k in RouteB) {  //foreach connection in RouteA
                if (loop.Contains(k.Value)){                        //if loop contains node
                    loopRouteB.Add(k);                              //add to route connection
                }
            }


            //testing
            string one = "loopPart1: ";
            string two = "loopPart2: ";
            foreach (KeyValuePair<connection, node> k in loopRouteA)
            {
                one += k.Value.name + ", ";
            }
            foreach (KeyValuePair<connection, node> k in loopRouteB)
            {
                two += k.Value.name + ", ";
            }

            Debug.Log(one);
            Debug.Log(two);

            //now perform patterns on the two halfs of the route
            //make sure when doing patterns not to put obstacles on the node in both loop parts (both loop parts contain the node that joins them)

            int ACount = loopRouteA.Count;
            int BCount = loopRouteB.Count;
            
            if (ACount > 3 && BCount > 3){ //both routes are long
				Debug.Log("Long a, Long b");
                //TwoAlternativePaths(loopRouteA, loopRouteB); //alternative paths rule
            }
            else if (ACount > 3 && BCount <= 3) { //A is long, B is short
				Debug.Log("Long a, Short b");
                //HiddenShortcut(loopRouteA, loopRouteB); //add hidden shortcut to short route
				DramaticCycle(loopRouteB);
				//DangerousRoute (loopRouteB, loopRouteA);

            }
            else if (ACount <= 3 && BCount > 3){ //A is short, B is long
				Debug.Log("Short a, Long b");
				//UnknownReturn(loopRouteA,loopRouteB);
				//LockAndKey(loopRouteA,loopRouteB);
            }
            else{ //both are short, but still pass the shorter one into the shorter postion!
				Debug.Log("Short a, Short b");

            }

        }

    }

	//may not be needed now we are using loops?
	connection getRandomUniqueConnection(List<KeyValuePair<connection, node>> routeA, List<KeyValuePair<connection, node>> routeB){ //gets a random unique connection from routeA by comparing to routeB
		List<KeyValuePair<connection, node>> routeAunique = new List<KeyValuePair<connection, node>>();
		foreach (KeyValuePair<connection, node> k1 in routeA){ //get list of unique key value pairs by connection
			bool isEqual = false;
			foreach (KeyValuePair<connection, node> k2 in routeB){
				if (k1.Key.obj.transform.position == k2.Key.obj.transform.position) {								//if equal, skip to next pair in routeA
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

	//returns random unique node from routeA, comparing to routeB
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


	// pattern rules: //

	void TwoAlternativePaths(List<KeyValuePair<connection, node>> routeA, List<KeyValuePair<connection, node>> routeB){ //only ran on both long paths
        //add monster on routeA, trap on routeB
		Debug.Log("run Alternate Paths rule");

		DangerousRoute (routeA, routeB); //add monster to routeA (mirrors dangerous route)
		/*node uniqueNode = getRandomUniqueNode (routeA, routeB);
		if (uniqueNode.name == "null node") { // if no nodes unique, add to connection instead
			//place obstacle on one route(monster)
			getRandomUniqueConnection(routeA,routeB).AddFeatureToConnection (new token ("monster", monsterCircle)); 
		} else {								//node is unique, so add monster
			//place monster on a nodes
			uniqueNode.AddFeature(new token("monster", monsterCircle)); //add a obstacle
		}*/
			
		getRandomUniqueConnection(routeB, routeA).AddFeatureToConnection(new token("trap", trapCircle)); 				//add a obstacle on a connection (trap)
	}

	void HiddenShortcut(List<KeyValuePair<connection, node>> longRoute, List<KeyValuePair<connection, node>> shortRoute){
		Debug.Log ("run Hidden Shortcut rule");
		getRandomUniqueConnection(shortRoute,longRoute).AddFeatureToConnection(new token("hidden", hiddenCircle)); 
	
	}

	void DramaticCycle(List<KeyValuePair<connection, node>> shortRoute){
		Debug.Log("dramatic cycle");
		//Random.Range (0, routeAunique.Count)].Key
		dramaticCycleNodes[0] = shortRoute[0].Value; 					//start of dramatic view
		dramaticCycleNodes[1] = shortRoute[shortRoute.Count - 1].Value;	//end of dramatic view

		//add dramatic cycle connection
		addConnection (dramaticCycleNodes [0], dramaticCycleNodes [1], connectionDramatic);

		//foreach node on shortRoute
		//remove 

		//remove connection to nodes on short path
		//find some way to remove the connections in connectedNodes also
		//foreach node in nodearray remove connections to this node? etc
		/*
		for(int i = 0; i < shortRoute.Count; i++){ //skip first connection
			Debug.Log("remove: " + shortRoute[i].Value.name);
			Destroy (shortRoute [i].Key.obj);

		}*/
	}

	void DangerousRoute(List<KeyValuePair<connection, node>> shortRoute,List<KeyValuePair<connection, node>> longRoute){
		//place a danger (monster) on the short route
		Debug.Log ("run DangerousRoute rule ");

		node uniqueNode = getRandomUniqueNode (shortRoute, longRoute);
		if (uniqueNode.name == "null node") { // if no nodes unique, add to connection instead
			//place obstacle on one route(monster)
			getRandomUniqueConnection(shortRoute,longRoute).AddFeatureToConnection (new token ("monster", monsterCircle)); 
		} else {
			//node is unique, so add monster
			uniqueNode.AddFeature(new token("monster", monsterCircle)); 
		}
	}

	void UnknownReturn(List<KeyValuePair<connection, node>> shortRoute,List<KeyValuePair<connection, node>> longRoute){
		Debug.Log ("run Unknown return rule ");

		//get random unique connection, and add collapsing bridge type

		connection con = getRandomUniqueConnection (shortRoute, longRoute);
		con.ChangeType (ConType.collapsing, connectionCollapse);
	}

	void LockAndKey(List<KeyValuePair<connection, node>> shortRoute,List<KeyValuePair<connection, node>> longRoute){
		Debug.Log("Lock and Key cycle");

		node endNode = shortRoute [shortRoute.Count - 1].Value; 
		bool foundOutwardConnections = false;
		foreach (KeyValuePair<connection, node> k in endNode.connectionToNodes) {
			if (!shortRoute.Contains (k) && !longRoute.Contains(k)){  //if connection not in shortRoute or longRoute (therefore not in this loop)
				k.Key.AddFeatureToConnection(new token("lock", lockCircle));//add lock
				foundOutwardConnections = true;
			}
		}

		//if key on goal node, make the goal only achivable when key is collected (eg if its a monster, keep the monster in stone till key is found)
		if (!foundOutwardConnections) { //if no connections were outside of routes, then the goal node of loop is the actually goal node!
			goalNode.AddFeature(new token("lock", lockCircle));
		}
			

		//place key at the first node of long route, and close long route off in that direction (so player encounters lock before seeing key)
		longRoute[1].Value.AddFeature(new token("key", keyCircle)); //add key to the first node (exclusing start node)
		longRoute[1].Key.ChangeType(ConType.blocked, connectionBlockedSpr);	//block connection to it
	}

}

// utlilty classes used for this generator and other parts of the project //

public class node{
	public List<KeyValuePair<connection, node>> connectionToNodes = new List<KeyValuePair<connection, node>>(); //connects connections to connected nodes - might be able to just use this?

	public string name;
	//public connection[] connections = new connection[4]; //array of 4 connections: left, top, right, bot - might not be needed
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

public class connection{
	public ConType type = ConType.normal;
	public GameObject obj;
	private List<token> features = new List<token> ();

	public connection(){ //empty constructor = predictable null object
		obj = new GameObject("null connection");
	}

	public connection(Vector3 pos, Sprite i, float rot){

		//create connection in world
		obj = new GameObject("connection");	
		obj.transform.position = pos;
		obj.transform.Rotate(new Vector3(0f,0f,rot));
		SpriteRenderer ren = obj.AddComponent<SpriteRenderer>();	
		ren.sprite = i;
	}

	public void AddFeatureToConnection(token newToken){ //for adding to connection
		newToken.obj = new GameObject(newToken.type);
		newToken.obj.transform.position = obj.transform.position;
		newToken.obj.transform.localScale = new Vector3(.5f,.5f,.5f);
		newToken.obj.transform.SetParent (obj.transform); //set parent to connection
		SpriteRenderer ren = newToken.obj.AddComponent<SpriteRenderer>();	
		ren.sprite = newToken.sprite;
		ren.sortingOrder = 2;  							//set order in layer infront of parent
		features.Add (newToken); 				//add new node to list of connection features
	}

	public void ChangeType(ConType t, Sprite spr){
		type = t;
		obj.GetComponent<SpriteRenderer> ().sprite = spr;
	}
		
}
	
public enum ConType{
	normal, dramatic, collapsing, blocked
}