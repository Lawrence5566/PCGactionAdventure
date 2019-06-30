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
	public Sprite lockCircle;
	public Sprite keyCircle;
    public Sprite monsterCircle;
    public Sprite trapCircle;

    public Sprite itemCircle;
    public Sprite healItemCircle;
	public Sprite hiddenCircle;
	public node[] nodeArray = new node[12];
	public node startNode;
	node goalNode;

    int maxRouteLength = 6;

	node[] dramaticCycleNodes = new node[2];
		
	public void Init () {

		int n = 0;
		for (int x = 0; x < 3; x++) { //3 across and 4 down node array - starts bottom left in world, goes up
			for (int y = 0; y < 4; y++) {
				nodeArray[n] = new node(this.transform.position + new Vector3 (x * 4f, y * 4f, 0f), "node" + n, circle, this.transform); //space nodes out 3.5 units apart
				n ++;
			}
		}

		n = 0;
		for (int x = 0; x < 3; x++) { //creating starting connections and setting adjacent nodes
			for (int y = 0; y < 4; y++) {
				for (int i = 0; i < nodeArray.Length; i++) { //for each node in array
					var relativePoint = nodeArray[n].obj.transform.InverseTransformPoint(nodeArray[i].obj.transform.position);

					if (relativePoint.x == -4.0 && relativePoint.y == 0) { //has left node (and not above or below)
						connection newCon = new connection(nodeArray[n].obj.transform.position + new Vector3(-2f,0f,0f), connectionSpr, 90f, this.transform); //add left connection
						nodeArray[n].connectedNodes.Add(nodeArray[i]);
						nodeArray[n].connectionToNodes.Add(new KeyValuePair<node, connection>(nodeArray[i], newCon));
					}
					if (relativePoint.x == 4.0 && relativePoint.y == 0) { //has right node
						connection newCon = new connection(nodeArray[n].obj.transform.position + new Vector3(2f,0f,0f), connectionSpr, 90f, this.transform); //right
						nodeArray[n].connectedNodes.Add(nodeArray[i]);
						nodeArray[n].connectionToNodes.Add(new KeyValuePair<node, connection>(nodeArray[i], newCon));
					}
					if (relativePoint.y == 4.0 && relativePoint.x == 0) { //has top node (and not right or left)
						connection newCon = new connection(nodeArray[n].obj.transform.position + new Vector3(0f,2f,0f), connectionSpr, 0f, this.transform); //top
						nodeArray[n].connectedNodes.Add(nodeArray[i]);
						nodeArray[n].connectionToNodes.Add(new KeyValuePair<node, connection>(nodeArray[i], newCon));
					}
					if (relativePoint.y == -4.0 && relativePoint.x == 0) { //has bot node
						connection newCon = new connection(nodeArray[n].obj.transform.position + new Vector3(0f,-2f,0f), connectionSpr, 0f, this.transform); //bot
						nodeArray[n].connectedNodes.Add(nodeArray[i]);
						nodeArray[n].connectionToNodes.Add(new KeyValuePair<node, connection>(nodeArray[i], newCon));
					}
				}
				n++;
			}
		}

		//create dungeon:
		DungeonRule();

		//pass on to map converter:
		int[,] map = FindObjectOfType<GraphToMapConverter>().CreateMap(nodeArray, dramaticCycleNodes, breadthFirstNodeSearch());

		//generate wall mesh from nodeArray:
		FindObjectOfType<MeshGenerator>().GenerateMesh(map, 1, false); //generate wall mesh squareSize of 1

		//generate floor mesh - works by flipping map bits and generating like a wall but without sides
		FindObjectOfType<MeshGenerator>().GenerateMesh(map, 1, true);  //generate floor mesh

	}

    // graph manipulation functions: // 

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
		List<node> validNodes = new List<node> (curr.connectedNodes);

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

		if(validNodes.Count == 0){ 	//hit a dead end, no loops here, go back to previous node to try find a loop
			if (deadEndNodes.Count >= activeNodes){ //looked through all possible nodes
				return loops;
			}

			path.Remove(curr);										//remove current node from path, and add to dead end nodes
			deadEndNodes.Add(curr);

			node newPrevious = null;
			if (path.Count > 1) //if after removing current, there is still one more previous node
				newPrevious = path [path.Count - 2]; //set it

			loops = findLoopsInGraph(path[path.Count - 1], newPrevious, path, deadEndNodes, activeNodes, loops);				//recurse to go back to previous node
		}

		return loops;

		//remove duplicates when outputting (as some can be made cus of dead end stuff)
			
	}

	KeyValuePair<node, connection> addConnection(node a, node b, Sprite sprite){ //create connection from A -> B add to each other's connectedNodes
		var relativePoint = a.obj.transform.InverseTransformPoint(b.obj.transform.position); //for arrow direction
		Vector2 relPoint = new Vector2(relativePoint.x, relativePoint.y);
		float angle = 0f;

		//calcualte arrow orentation for sprte - right and left is flipped from expected
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
		KeyValuePair<node, connection> oldCon = a.connectionToNodes.Find (x => x.Key == b); 	//find connection that connects to B
		if (oldCon.Value != null) Destroy(oldCon.Value.obj); //destroy old connection
		a.connectionToNodes.Remove (oldCon);

		//create new one
		connection newCon = new connection(new Vector3(relPoint.x/2, relPoint.y/2, 0f) + a.obj.transform.position, sprite, angle, this.transform);

		//add to connectionsToNodes, if it doesn't already have one?
		KeyValuePair<node, connection> newConnectionToNode = new KeyValuePair<node, connection> (b, newCon);
		a.connectionToNodes.Add(newConnectionToNode);

        // destroy connection in B
        KeyValuePair<node, connection> oldCon2 = b.connectionToNodes.Find(x => x.Key == a);  //find connection that connects to A
        if (oldCon2.Value != null) Destroy(oldCon2.Value.obj); //destroy old connection, if there was one
        a.connectionToNodes.Remove(oldCon2);

        // add reverse connection KeyValuePair that shares same connection object
        KeyValuePair<node, connection> newConnectionToNode2 = new KeyValuePair<node, connection>(a, newCon);
        b.connectionToNodes.Add(newConnectionToNode2);

        if (!a.connectedNodes.Contains (b)) {
			a.connectedNodes.Add (b); //make nodes adjacent, if they were not already
		}
		if (!b.connectedNodes.Contains (a)) {
			b.connectedNodes.Add(a);
		}

		return newConnectionToNode;
	}

    connection getRandomUniqueConnection(List<KeyValuePair<node, connection>> routeA, List<KeyValuePair<node, connection>> routeB)
    { //gets a random unique connection from routeA by comparing to routeB
        List<KeyValuePair<node, connection>> routeAunique = new List<KeyValuePair<node, connection>>();
        foreach (KeyValuePair<node, connection> k1 in routeA)
        { //get list of unique key value pairs by connection
            bool isEqual = false;
            foreach (KeyValuePair<node, connection> k2 in routeB)
            {
                if (k1.Value.obj.transform.position == k2.Value.obj.transform.position)
                {                               //if equal, skip to next pair in routeA
                    isEqual = true;
                    break;
                }
            }
            //got to the end, check if it was equal
            if (!routeAunique.Contains(k1) && !isEqual)
            {               //if not already in unique list, and not equal to any
                routeAunique.Add(k1);
            }
        }

        if (routeAunique.Count == 0)
        {
            Debug.Log("no unique connection found");
            return new connection();//if empty, return null object
        }

        return routeAunique[Random.Range(0, routeAunique.Count)].Value; //return random chosen connection
    }

    node getRandomUniqueNode(List<KeyValuePair<node, connection>> routeA, List<KeyValuePair<node, connection>> routeB)
    { //gets a random unique node from routeA by comparing to routeB
        List<KeyValuePair<node, connection>> routeAunique = new List<KeyValuePair<node, connection>>();

        foreach (KeyValuePair<node, connection> k1 in routeA)
        { //get list of unique key value pairs by node
            bool isEqual = false;
            foreach (KeyValuePair<node, connection> k2 in routeB)
            {
                if (k1.Key == k2.Key)
                {                               //if equal, skip to next pair in routeA
                    isEqual = true;
                    break;
                }
            }
            //got to the end, check if it was equal
            if (!routeAunique.Contains(k1) && !isEqual)
            {               //if not already in unique list, and not equal to any
                routeAunique.Add(k1);
            }
        }

        if (routeAunique.Count == 0)
        {
            Debug.Log("no unique node found");
            return new node();//if empty, return null object
        }

        return routeAunique[Random.Range(0, routeAunique.Count)].Key; //return random chosen node
    }

    void DisconnectNodes(node a, node b)
    {

        int index = (a.connectionToNodes.FindIndex(x => x.Key == b)); //check if A contains B in connectionToNodes
        if (index != -1)
        {
            Destroy(a.connectionToNodes[index].Value.obj);    //destroy connection object
            a.connectionToNodes.RemoveAt(index);            //destroy connectionTo
        }
        a.connectedNodes.Remove(b); //remove from connections if you find

        int index2 = (b.connectionToNodes.FindIndex(x => x.Key == a));
        if (index2 != -1)
        {
            Destroy(b.connectionToNodes[index2].Value.obj);
            b.connectionToNodes.RemoveAt(index2);
        }
        b.connectedNodes.Remove(a);
    }

    string checkForAdjacency(node a, node b) {

        var relativePoint = a.obj.transform.InverseTransformPoint(b.obj.transform.position);

        if (relativePoint.x == -4.0 && relativePoint.y == 0)
        { //b is left of a (and not above or below)
            return "left";
        }
        if (relativePoint.x == 4.0 && relativePoint.y == 0)
        {
            return "right";
        }
        if (relativePoint.y == 4.0 && relativePoint.x == 0)
        {
            return "top";
        }
        if (relativePoint.y == -4.0 && relativePoint.x == 0)
        { //has bot node
            return "bot";
        }

        return null;
    }

    List<KeyValuePair<node, connection>> breadthFirstNodeSearch() { //uses queue ordering to breadthfirstSearch the nodeArray and return an order the player encounters connections and nodes by breadth
        Queue<node> q = new Queue<node>();
        List<KeyValuePair<node, connection>> ordered = new List<KeyValuePair<node, connection>>(); //acts as 'visited' and to accumalate the ordered nodes
        q.Enqueue(startNode); //start from the root node (start node in this case)
        
        while (q.Count > 0) {        
            node current = q.Dequeue();
            //ordered.Add(current);

            if (current == null) //skip if queue is empty (current is blank as there was nothing to take)
                continue;

           // string log = "current: " + current.name + " connections: ";

            foreach (KeyValuePair<node, connection> k in current.connectionToNodes) {
               // log += k.Key.name + ", ";
                if (!ordered.Contains(k) && ordered.FindIndex(x => x.Value == k.Value) < 0 && k.Key.name != startNode.name ) { //make sure keyvaluepair with same connection doesn't exist, and node isn't start node
                    q.Enqueue(k.Key);
                    ordered.Add(k);
                }
            }
            //Debug.Log(log);
        }

        //int counter = 0;
        //string test = ""; //for testing breadth function
        //foreach (KeyValuePair<node, connection> k in ordered) {
           // k.Value.obj.name = "con" + counter; //rename the connections to see if there are duplicates 
            //counter++;
            //Debug.Log("count");
       //     test = test + k.Key.name + ", " + k.Value.obj.name + ", ";
       // }
        //Debug.Log(test);

        return ordered;
     }


    // graph grammer functions: //

    //Dungeon > Rooms + Goal
    void DungeonRule(){
		//set start symbol - picks random location
		startNode = nodeArray[Random.Range(0,11)];
		GameObject text = Instantiate(TextBasePrefab,  startNode.obj.transform);
        text.transform.position = new Vector3(text.transform.position.x, text.transform.position.y, -0.1f); //modify z so text is in front of node
        startNode.obj.name = "StartNode";
		text.GetComponent<TextMesh>().text = "Start";
        text.layer = 15; //change culling layer

        //fire 'goal' function
        GoalRule();
		
	}

	void GoalRule(){
		//create goal location

		do { //keep picking different spots till you get one that isnt the start node
			goalNode = nodeArray [Random.Range (0, 11)];
		} while (goalNode.obj.name == "StartNode");

		GameObject text = Instantiate(TextBasePrefab,  goalNode.obj.transform);
        text.transform.position = new Vector3(text.transform.position.x, text.transform.position.y, -0.01f); //modify z so text is in front of node
        text.layer = 15; //change culling layer
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
			if (Enumerable.SequenceEqual (routeA, routeB)) { //if equal, generate B again
				routeB = findAPath (startNode, new List<node> (), new List<node> ());
				continue;
			}
			break;
		}
			
		//log paths for testing:
        /*
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
        */

		//do this only when passing on to map converter: (it eliminates unneccisary nodes)
		ProcessNodeArray (routeA, routeB);

    }

    void ProcessNodeArray(List<node> routeA, List<node> routeB){ //final prep processing for converting GraphToMap 
		//destroy all connections - what was point in having them in the first place?
		for (int i = 0; i < nodeArray.Length; i++) {  //foreach node
			nodeArray [i].connectedNodes.Clear ();
			foreach (KeyValuePair<node, connection> k in nodeArray [i].connectionToNodes) {	//foreach connection
				Destroy(k.Value.obj); //destroy connection object
			}
			nodeArray [i].connectionToNodes.Clear ();
		}

		//reconnect along routes
		//add connections for route & converting to KeyValuePairs
        //keep routeA and routeB solo nodes as we use them later
		List<KeyValuePair<node, connection>> RouteA = new List<KeyValuePair<node, connection>> ();
		List<KeyValuePair<node, connection>> RouteB = new List<KeyValuePair<node, connection>> ();
		for (int i = 1; i < routeA.Count; i++) { //start from one ahead, so that we don't go out of range
			RouteA.Add(addConnection (routeA [i - 1], routeA [i], connectionArrowSpr)); //add connection between nodes, and return key value pair for each to new route list
		}
		for (int i = 1; i < routeB.Count; i++) { 
			RouteB.Add(addConnection (routeB [i - 1], routeB [i], connectionArrowSpr));
		}
		//add start node to beginning of routes (this is for loop stuff later)
		RouteA.Insert (0, new KeyValuePair<node, connection> (startNode, new connection()));
		RouteB.Insert (0, new KeyValuePair<node, connection> (startNode, new connection()));

        int activeNodes = 0;
		foreach (node n in nodeArray) { //calculate number of nodes in graph that are active
            if (n.connectionToNodes.Count > 0 && n != goalNode) //don't include goal node in loop finding
				activeNodes++;
		}

		List<List<node>> loops = findLoopsInGraph (startNode, null, new List<node> (), new List<node> (), activeNodes, new List<List<node>> ());

        //loops.Distinct().ToList (); //remove duplicates (can sometimes get them)?

        foreach (List<node> loop in loops)
        {

            List<KeyValuePair<node, connection>> loopRouteA = new List<KeyValuePair<node, connection>>();
            List<KeyValuePair<node, connection>> loopRouteB = new List<KeyValuePair<node, connection>>();

            foreach (KeyValuePair<node, connection> k in RouteA)
            {  //foreach connection in RouteA
                if (loop.Contains(k.Key))
                {                        //if loop contains node from that connection
                    loopRouteA.Add(k);                              //add to connection to RouteA half of loop
                }
            }

            foreach (KeyValuePair<node, connection> k in RouteB)
            {  //foreach connection in RouteB, may have duplicates from A this way, remove them here?
                if (loop.Contains(k.Key))
                {
                    loopRouteB.Add(k);
                }
            }


            //testing output
            string one = "loopPart1: ";
            string two = "loopPart2: ";
            foreach (KeyValuePair<node, connection> k in loopRouteA)
            {
                one += k.Key.name + ", ";
            }
            foreach (KeyValuePair<node, connection> k in loopRouteB)
            {
                two += k.Key.name + ", ";
            }

            Debug.Log(one);
            Debug.Log(two);

            //now perform patterns on the two halfs of the route
            //make sure when doing patterns not to put obstacles on the node in both loop parts (both loop parts contain the node that joins them)

            //condense these down? switch statements?

            int ACount = loopRouteA.Count;
            int BCount = loopRouteB.Count;

            int choice = Random.Range(0, 4);

            if (ACount > 4 && BCount > 4)
            { //both routes are long 
                Debug.Log("Long a, Long b");
                if (choice == 0 || choice == 1)
                {
                    TwoAlternativePaths(loopRouteA, loopRouteB);
                }
                else
                {
                    TwoAlternativePaths(loopRouteB, loopRouteA);
                }
            }
            else if (ACount > 4 && BCount <= 4)
            { //A is long, B is short
                Debug.Log("Long a, Short b");

                if (choice == 0)
                {
                    HiddenShortcut(loopRouteB, loopRouteA);
                }
                else if (choice == 1)
                {
                    DramaticCycle(loopRouteB);
                }
                else if (choice == 2)
                {
                    DangerousRoute(loopRouteB, loopRouteA);
                }
                else
                {
                    LockAndKey(loopRouteB, loopRouteA);
                }
            }
            else if (ACount <= 4 && BCount > 4)
            { //A is short, B is long
              //UnknownReturn(loopRouteA,loopRouteB); //not using?
                Debug.Log("Short a, Long b");

                if (choice == 0)
                {
                    HiddenShortcut(loopRouteA, loopRouteB);
                }
                else if (choice == 1)
                {
                    DramaticCycle(loopRouteA);
                }
                else if (choice == 2)
                {
                    DangerousRoute(loopRouteA, loopRouteB);
                }
                else
                {
                    LockAndKey(loopRouteA, loopRouteB);
                }

            }
            else
            { //both are short, so run on the shorter one
                Debug.Log("Short a, Short b");

                List<KeyValuePair<node, connection>> shorter = new List<KeyValuePair<node, connection>>();
                List<KeyValuePair<node, connection>> longer = new List<KeyValuePair<node, connection>>();

                if (ACount >= BCount)
                {
                    shorter = loopRouteB;
                    longer = loopRouteA;
                }
                else
                {
                    shorter = loopRouteA;
                    longer = loopRouteB;
                }

                if (choice == 0)
                {
                    HiddenShortcut(shorter, longer);
                }
                else if (choice == 1)
                {
                    DramaticCycle(shorter);
                }
                else if (choice == 2)
                {
                    DangerousRoute(shorter, longer);
                }
                else
                {
                    LockAndKey(shorter, longer);
                }
            }

            // TreasureRoom route on longest route
            if (ACount >= BCount) { //A is longer
                TreasureRoom(loopRouteA);
            }
            else { //B is longer
                TreasureRoom(loopRouteB);
            }

            //TwoEmptyRooms pattern 
            TwoEmptyRooms(loopRouteA);
            TwoEmptyRooms(loopRouteB);

            if (goalNode.features.Count == 0)
            { //if goal node has no features, add a boss
                goalNode.AddFeature(new token("boss", monsterCircle));

                BossPrepHPpattern(loopRouteA); //if 3+ enemies or traps present in the route, will add hp right before the boss
                BossPrepHPpattern(loopRouteB);

            }

        }

        /* //for if we add other monsters to goal node:
		if (goalNode.features.Any(x => x.type == "monster") && !goalNode.features.Any(x => x.type == "lock")){
			//if goal node has any monsters featured at end of cycle and no lock, add boss

			//remove monsters
			goalNode.features.RemoveAll(x => x.type == "monster");
			//add boss
			goalNode.AddFeature(new token ("boss", monsterCircle));
		}*/

    }

    // pattern rules: //

    void TwoEmptyRooms(List<KeyValuePair<node, connection>> route) {
        bool emptyRoom = false;
        bool emptyConnection = false;
        for (int i = 0; i < route.Count; i++) {
            if (route[i].Key.features.Count == 0 && route[i].Key != startNode && route[i].Key != goalNode) { //if room i is empty, and not start or goal node
                if (emptyRoom && emptyConnection) {//if previous room was empty and connection between them is empty, therefore two empty rooms!
                    int choice = Random.Range(0, 2); //inclusive, exclusive
                    switch (choice){ //add heal or item in the second room
                        case 0:
                            route[i].Key.AddFeature(new token("heal", healItemCircle));
                            Debug.Log("added heal from two empty");
                            break;
                        case 1:
                            Debug.Log("added item from two empty");
                            route[i].Key.AddFeature(new token("item", itemCircle));  
                            break;
                    }
          

                    Debug.Log("TwoEmptyRooms");
                    break;
                }

                emptyRoom = true; //we know this room is empty

                if (route[i].Value.features.Count == 0) //now check its connection to the next one
                    emptyConnection = true;
            }
        }
    }

    void TreasureRoom(List<KeyValuePair<node, connection>> longRoute) {
        Debug.Log("TreasureRoom");
        List<node> monsterRooms = new List<node>();
        foreach (KeyValuePair<node, connection> k in longRoute) {
            if (k.Key.features.Exists(x => x.type == "monster") || k.Key.features.Exists(x => x.type == "trap")) { //if room contains an enemy or trap
                monsterRooms.Add(k.Key);
            }
        }

        List<int> indexes = Enumerable.Range(0, monsterRooms.Count()).ToList();

        for (int i = 0; i < monsterRooms.Count(); i++) { //pick from all possible enemy rooms
            int currIndex = indexes[Random.Range(0, indexes.Count())]; //pick random index in indexes

            bool foundEmptyAdjacentRoom = false;

            foreach (node n in nodeArray) {  // try to add treasure room by looking at all nodes for a surrounding node
                string pos = checkForAdjacency(monsterRooms[currIndex], n);
                if (pos != null) { //found an adjacent room!
                    if (n.connectionToNodes.Count == 0 && n != startNode && n != goalNode) { //if room is not part of a route, and is not goal or start node
                        addConnection(n, monsterRooms[currIndex], connectionArrowSpr); //create connection 
                        n.AddFeature(new token("item", itemCircle));                   //and add an item
                        Debug.Log("added item from treasure room prep");
                        foundEmptyAdjacentRoom = true;
                        break;
                    }
                }
            }

            if (foundEmptyAdjacentRoom) //break from loop since we found a room for the treasure room
                break;

            indexes.RemoveAt(currIndex);  //remove index as we didnt find an empty room
        }

    }

    void BossPrepHPpattern(List<KeyValuePair<node, connection>> route) { //returns true if it could place a hp pickup
        //if player encounters >= 3 enemies or trap before boss, add HP item
        int obstacleCount = 0;
        foreach (KeyValuePair<node, connection> k in route) {
            if (k.Key.features.Exists(x => x.type == "trap" || x.type == "monster"))
                obstacleCount++;
        }

        if (obstacleCount >= 3) { //found 3+ obstacles on this route, so add hp to room one before end
            Debug.Log("added heal from boss prep");
            route[route.Count - 2].Key.AddFeature(new token("heal", healItemCircle));
        }
        
    }

    void TwoAlternativePaths(List<KeyValuePair<node, connection>> routeA, List<KeyValuePair<node, connection>> routeB){
        //add monster on routeA, trap on routeB
		Debug.Log("run Alternate Paths rule");

		DangerousRoute (routeA, routeB); 																	//add monster to routeA (mirrors dangerous route)

		for (int i = 1; i < routeB.Count - 1; i++) { //add traps to each connection except first and last
			routeB [i].Value.AddFeatureToConnection (new token ("trap", trapCircle));
		}

		//getRandomUniqueConnection(routeB, routeA).AddFeatureToConnection(new token("trap", trapCircle)); 	//add a trap to connection on route B
	}

	void HiddenShortcut(List<KeyValuePair<node, connection>> shortRoute, List<KeyValuePair<node, connection>> longRoute){
		Debug.Log ("run Hidden Shortcut rule");
		//getRandomUniqueConnection(shortRoute,longRoute).AddFeatureToConnection(new token("hidden", hiddenCircle)); 
		shortRoute[0].Key.connectedNodes.Remove(shortRoute[1].Key); //soft disconnect so path isn't spawned
		shortRoute[1].Key.connectedNodes.Remove(shortRoute[0].Key); //soft disconnect so path isn't spawned
		shortRoute [1].Value.AddFeatureToConnection (new token ("hidden", hiddenCircle));
	}

	void DramaticCycle(List<KeyValuePair<node, connection>> shortRoute){
		//only ran on short routes
		Debug.Log("dramatic cycle");
		//Random.Range (0, routeAunique.Count)].Value
		dramaticCycleNodes[0] = shortRoute[0].Key; 					//start of dramatic view
		dramaticCycleNodes[1] = shortRoute[shortRoute.Count - 1].Key;	//end of dramatic view

		//foreach node on shortRoute, dissconnect
		//remove connection to first node on short route
		if (shortRoute.Count > 1) { //should always be but just in case
			for (int i = 1; i < shortRoute.Count; i++) { //start one ahead
				DisconnectNodes (shortRoute [i-1].Key, shortRoute [i].Key); //disconnect previous and this
			}
		}

		//add dramatic cycle connection
		addConnection (dramaticCycleNodes [0], dramaticCycleNodes [1], connectionDramatic);
	}

	void DangerousRoute(List<KeyValuePair<node, connection>> shortRoute,List<KeyValuePair<node, connection>> longRoute){
		//place a danger (monster) on the short route
		Debug.Log ("run DangerousRoute rule ");

		//add to every node on short route (ends up usually being just one since short routes are 3 long):
		bool addedMonster = false;
		for (int i = 1; i < shortRoute.Count-1; i++){ //(skip first and last as they are part of other routes)
			shortRoute[i].Key.AddFeature(new token("monster", monsterCircle)); 
			addedMonster = true;
		}

        //add to first proper connection as route was too short
        if (addedMonster == false)
            shortRoute[1].Value.AddFeatureToConnection(new token("monster", monsterCircle));
        
	}

	void UnknownReturn(List<KeyValuePair<node, connection>> shortRoute,List<KeyValuePair<node, connection>> longRoute){
		Debug.Log ("run Unknown return rule ");

		//get random unique connection, and add collapsing bridge type

		connection con = getRandomUniqueConnection (shortRoute, longRoute);
		con.ChangeType (ConType.collapsing, connectionCollapse);
	}

	void LockAndKey(List<KeyValuePair<node, connection>> shortRoute,List<KeyValuePair<node, connection>> longRoute){
		Debug.Log("Lock and Key cycle");

		node endNode = shortRoute [shortRoute.Count - 1].Key; 
		bool foundOutwardConnections = false; //checking if any outward connections are found to get out of this loop

		token keyToken = new token("key", keyCircle); //create key for all the locks we are about to place

		foreach (KeyValuePair<node, connection> k in endNode.connectionToNodes) {
			if (shortRoute.FindIndex(x => x.Value == k.Value) < 0 && longRoute.FindIndex(x => x.Value == k.Value) < 0) {  //if connection not in shortRoute and longRoute, lock it (stoping player advancing past 'end node')
                                                                        //this means player must explore long and short routes to find the key
				k.Value.AddFeatureToConnection(new token("lock", lockCircle, keyToken));    //add a lock
				foundOutwardConnections = true;
			}
		}
			
		//if no connections were outside of routes, then the goal node is the 'endNode', so make the goal only achivable when key is collected (eg if its a monster, keep the monster in stone till key is found)
        // (do this later when dealing with lock features)
		if (!foundOutwardConnections) {
			goalNode.AddFeature(new token("lock", lockCircle, keyToken));	//add lock onto goal node
		}

		//place key at the first node of long route, and close long route off in that direction (so player encounters lock before seeing key and has to go down long route in reverse)
		longRoute[1].Key.AddFeature(keyToken); //add key to the first node (excluding start node)

		//add enemy to guard the key
		if (longRoute.Count > 2) {
			longRoute [2].Value.AddFeatureToConnection (new token ("monster", monsterCircle));
		} else {
			longRoute [1].Key.AddFeature (new token ("monster", monsterCircle));
		}

		//longRoute[1].Value.ChangeType(ConType.blocked, connectionBlockedSpr);	//block connection to it
		DisconnectNodes(longRoute[0].Key, longRoute[1].Key);//for now, remove the connection instead of blocking it
	}

}

// utlilty classes used for this generator and other parts of the project //

public class node{
	public List<KeyValuePair<node, connection>> connectionToNodes = new List<KeyValuePair<node, connection>>(); //connects connections to connected nodes - might be able to just use this?

	public string name;
	public GameObject obj;
	public List<node> connectedNodes = new List<node>(); //not necessary?
	public List<token> features = new List<token> ();

	public node(){
		name = "null node";
		obj = new GameObject ("null node");
    }

	public node(Vector3 pos, string n, Sprite i, Transform parent){
		name = n;

		//create node in world
		obj = new GameObject(name);		
		obj.transform.position = pos;
		SpriteRenderer ren = obj.AddComponent<SpriteRenderer>();	
		ren.sprite = i;

        obj.transform.parent = parent;
        obj.layer = 15; //set camera culling layer
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
            features[i].obj.layer = 15; //set camera culling layer
            SpriteRenderer ren = features[i].obj.AddComponent<SpriteRenderer>();	
			ren.sprite = features[i].sprite;
			ren.sortingOrder = 2;  //set order in layer infront of parent
		}
	}

}

public class token{
	public string type;
    public ElementType Etype;
	public Sprite sprite;
	public GameObject obj;
	public token keyLink;

    public token() { //null type "predictible" constructor
        type = "null";
    }

	public token(string t, Sprite s){
		type = t;
		sprite = s;
	}
	public token(string t, Sprite s, token key){ //overload for connecting a key to this token
		type = t;
		sprite = s;
		keyLink = key;
	}
    public token(string t, Sprite s, ElementType ELtype) {
        type = t;
        sprite = s;
        Etype = ELtype;
    }
}

public class connection{
	public ConType type = ConType.normal; //not used, remove?
	public GameObject obj;
	public List<token> features = new List<token> ();

	public connection(){ //empty constructor = predictable null object?
        obj = new GameObject("null connection");
    }

	public connection(Vector3 pos, Sprite i, float rot, Transform parent){

		//create connection in world
		obj = new GameObject("connection");
		obj.transform.position = pos;
		obj.transform.Rotate(new Vector3(0f,0f,rot));
		SpriteRenderer ren = obj.AddComponent<SpriteRenderer>();	
		ren.sprite = i;

        obj.transform.parent = parent;
        obj.layer = 15; //set camera culling layer
    }

	public void AddFeatureToConnection(token newToken){ //for adding to connection
		newToken.obj = new GameObject(newToken.type);
		newToken.obj.transform.position = obj.transform.position;
		newToken.obj.transform.localScale = new Vector3(.5f,.5f,.5f);
		newToken.obj.transform.SetParent (obj.transform); //set parent to connection
        newToken.obj.layer = 15; //set camera culling layer
        SpriteRenderer ren = newToken.obj.AddComponent<SpriteRenderer>();	
		ren.sprite = newToken.sprite;
		ren.sortingOrder = 2;  							//set order in layer infront of parent
		features.Add (newToken); 				//add new token to list of connection features
	}

	public void ChangeType(ConType t, Sprite spr){
		type = t;
		obj.GetComponent<SpriteRenderer> ().sprite = spr;
	}
		
}
	
//never used in the end - remove?
public enum ConType{
	normal, dramatic, collapsing, blocked
}