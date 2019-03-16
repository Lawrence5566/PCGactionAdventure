﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//converts node graph to map
public class GraphToMapConverter : MonoBehaviour {

	int[,] Map;
	int width;
	int height;
	//int maxRoomNumber = 12;
	int maxRoomSize = 55; //4 extra since borders of rooms are 2 squares big each side, and 1 extra to prevent out of range errors
	int minRoomSize = 30;
	int nodeArrayXsize = 3;
	int nodeArrayYsize = 4;

	List<Room> roList =  new List<Room>(); 							//for gizmo testing

	public int[,] CreateMap(node[] nodeArray, node[] dramaticCycleNodes ){  					//takes node array, converts to rooms and combines rooms into one map

		List<Room> roomsList = new List<Room> ();
		RoomGenerator roomGenerator = new RoomGenerator ();

		width = maxRoomSize*nodeArrayXsize;
		height = maxRoomSize*nodeArrayYsize;
		Map = new int[width, height]; 	//create map of max size

		int currentX = 0; 				//keeps track of where we are on the map
		int currentY = 0;

		int[,] currRoomMap = new int[maxRoomSize, maxRoomSize]; //using max sizes as a base for now, create new room

		for (int n = 0; n < nodeArray.Length; n++) {
			List<Coord> newRegion = new List<Coord>();
			int RoomWidth = UnityEngine.Random.Range(minRoomSize,50);//generate random size for room within boundaries
			int RoomHeight = UnityEngine.Random.Range(minRoomSize,50);

			if (nodeArray[n].connectedNodes.Count != 0) { //if node is connected
				//create a room
				currRoomMap = roomGenerator.GenerateRoom (RoomWidth, RoomHeight, 35); // we use 35 as fill percent, to get the most realistic looking cave rooms

			} else{// create empty room? make this more efficent?
				currRoomMap = roomGenerator.GenerateRoom (RoomWidth, RoomHeight, 100);
				//continue;
			}

			//add room to map

			if (n < 4) { //picking cursor location
				currentX = 0;
				currentY = maxRoomSize * n;
			} else if (n < 8) {
				currentX = maxRoomSize;
				currentY = maxRoomSize * (n - 4);
			} else if (n < 12) {
				currentX = maxRoomSize * 2;
				currentY = maxRoomSize * (n - 8);
			}

			int starty = currentY; //need to reset Y at the end of every X loop so store it here

			for (int x = 0; x < maxRoomSize; x++) { //dont use width or height here to account for full box or borders
				currentY = starty;
				for (int y = 0; y < maxRoomSize; y++) {
					if (x < currRoomMap.GetLength (0) && y < currRoomMap.GetLength (1)) { //part of room
						Map[currentX,currentY] = currRoomMap[x,y];
						if (Map [currentX, currentY] == 0) {
							newRegion.Add (new Coord (currentX, currentY)); //add these coordinates to new Region if they are space(and not wall)
						}
					} else { //empty space
						Map[currentX,currentY] = 1;
					}
					currentY++;
				}
				currentX++;
			}
				
			Room currRoom = new Room (newRegion, Map);  //create room out of region
			currRoom.node = nodeArray[n];
			roomsList.Add (currRoom);					//add to list of rooms

		}

		roList = roomsList; //for testing

		ConnectRooms(roomsList);

		if (dramaticCycleNodes [0] != null || dramaticCycleNodes [1] != null) {
			//get all rooms equal to dynamic view nodes (should only be 2)
			List<Room> dynamicViewRooms = roomsList.FindAll (x => x.node == dramaticCycleNodes [0] || x.node == dramaticCycleNodes [1]);
			Debug.Log ("dynamicview rooms size: " + dynamicViewRooms.Count );
			if (dynamicViewRooms.Count > 1) {
				CreateDynamicView (dynamicViewRooms);
				Debug.Log (dynamicViewRooms[0].node.name +","+ dynamicViewRooms[1].node.name );
				Debug.Log ("creating dynamic view");
			}
		}

		return Map;

	}

	void ConnectRooms(List<Room> allRooms){
		//use their nodes to connect them

		foreach(Room roomA in allRooms){
			foreach (Room roomB in allRooms) {
				if (roomA == roomB || roomA.IsConnected (roomB)) { //skip if they are the same room, or are already connected
					continue;
				}

				if (roomA.node.connectedNodes.Contains (roomB.node)) { //if roomA node should connected to roomB
					Coord bestTileA = new Coord ();
					Coord bestTileB = new Coord ();
					FindClosestTiles(roomA, roomB, out bestTileA, out bestTileB);
					CreatePassage (roomA, roomB, bestTileA, bestTileB, 1, 0);
				}
			}
		}
	}

	void FindClosestTiles(Room roomA, Room roomB, out Coord bestTileA, out Coord bestTileB){ //returns closest 2 tiles between rooms
		int bestDistance = 0;
		bestTileA = new Coord ();
		bestTileB = new Coord ();
		bool possibleConnectionFound = false;

		for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++) {
			for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++) {
				Coord tileA = roomA.edgeTiles [tileIndexA];
				Coord tileB = roomB.edgeTiles [tileIndexB];
				int distanceBetweenRooms = (int) (Mathf.Pow (tileA.tileX - tileB.tileX, 2) + Mathf.Pow (tileA.tileY - tileB.tileY, 2));

				if (distanceBetweenRooms < bestDistance || !possibleConnectionFound) { //found new best connection or not yet found a possible connection
					bestDistance = distanceBetweenRooms;
					possibleConnectionFound = true;
					bestTileA = tileA;
					bestTileB = tileB;

				}
			}
		}
	}
		
	void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB, int r, int tiletype){ //r = passage radius
		Room.ConnectRooms (roomA, roomB);//let rooms know they are now physically connected
		//Debug.DrawLine (CoordToWorldPoint (tileA), CoordToWorldPoint (tileB), Color.green, 300); //check room connections

		List<Coord> line = GetLine (tileA, tileB);
		foreach (Coord c in line) {
			DrawCircle(c,r,tiletype); //around each point in line draw a circle (3 is radius and therfore passage thickness)
		}
	}

	void CreateDynamicView(List<Room> dynamicViewRooms){
		Coord bestTileA;
		Coord bestTileB;
		FindClosestTiles (dynamicViewRooms [0], dynamicViewRooms [1], out bestTileA, out bestTileB);
		CreatePassage (dynamicViewRooms [0], dynamicViewRooms [0], bestTileA, bestTileB, 10, 2);

	}

	void DrawCircle(Coord c, int r, int tiletype) {
		for (int x = -r; x <= r; x++) {
			for (int y = -r; y <= r; y++) {
				if (x*x + y*y <= r*r) {
					int drawX = c.tileX + x;
					int drawY = c.tileY + y;
					if (drawX >= 2 && drawX < width && drawY >= 2 && drawY < height) { //if inside map
						Map[drawX,drawY] = tiletype;
					}
				}
			}
		}
	}

	List<Coord> GetLine(Coord from, Coord to){
		List<Coord> line = new List<Coord> ();
		int x = from.tileX;
		int y = from.tileY;

		int dx = to.tileX - from.tileX;
		int dy = to.tileY - from.tileY;

		bool inverted = false;
		int step = Math.Sign (dx);
		int gradientStep = Math.Sign (dy);

		int longest = Mathf.Abs (dx);
		int shortest = Mathf.Abs (dy);

		if (longest < shortest) { //swap
			inverted = true;
			longest = Mathf.Abs (dy);
			shortest = Mathf.Abs (dx);

			step = Math.Sign (dy);
			gradientStep = Math.Sign (dx);
		}

		int gradientAccumulation = longest / 2;
		for (int i = 0; i < longest; i++) {
			line.Add (new Coord (x, y));
			if (inverted) {
				y += step;
			} else {
				x += step;
			}

			gradientAccumulation += shortest;
			if (gradientAccumulation >= longest) {
				if (inverted) {
					x += gradientStep;
				} else {
					y += gradientStep;
				}
				gradientAccumulation -= longest;
			}

		}

		return line;
	}

	Vector3 CoordToWorldPoint(Coord tile) { //convert Coord object to a world point
		return new Vector3 (-width + .5f + tile.tileX, 2, -height + .5f + tile.tileY);
	}

	struct Coord{ //use to store tile locations in the map
		public int tileX;
		public int tileY;

		public Coord(int x, int y){
			tileX = x;
			tileY = y;
		}

	}

	class Room {
		public List<Coord> tiles;
		public List<Coord> edgeTiles;
		public List<Room> connectedRooms;
		public node node;
		public int roomSize;

		public Room() {
		}

		public Room(List<Coord> roomTiles, int[,] map) {
			tiles = roomTiles;
			roomSize = tiles.Count;
			connectedRooms = new List<Room>();

			edgeTiles = new List<Coord>();

			foreach (Coord tile in tiles) { //find tiles on the edge of the room
				for (int x = tile.tileX-1; x <= tile.tileX+1; x++) {
					for (int y = tile.tileY-1; y <= tile.tileY+1; y++) {
						if (x == tile.tileX || y == tile.tileY) {
							if (map[x,y] == 1) {
								edgeTiles.Add(tile);
							} 
						} 
					}
				}
			}

		}

		public static void ConnectRooms(Room roomA, Room roomB) { //add to connected rooms list
			roomA.connectedRooms.Add (roomB);
			roomB.connectedRooms.Add (roomA);
		}

		public bool IsConnected(Room otherRoom) { //check if its connected to this room
			return connectedRooms.Contains(otherRoom);
		}
	}
		
	/*
	void OnDrawGizmos() { //for testing
		if (Map != null) {
			for (int x = 0; x < Map.GetLength(0); x ++) {
				for (int y = 0; y < Map.GetLength(1); y ++) {
					Gizmos.color = (Map[x,y] == 1)?Color.black:Color.white;
					if (Map [x, y] == 2)
						Gizmos.color = Color.blue; //for fissures
					Vector3 pos = new Vector3(-width + x + .5f,0, -height + y + .5f);
                    Gizmos.DrawCube(pos,Vector3.one);
                }
            }

			foreach (Room r in roList) {
				foreach (Coord c in r.edgeTiles) {
					Gizmos.color = Color.red;
					Vector3 pos = new Vector3 (-width + c.tileX + .5f, 0, -height + c.tileY + .5f);// + new Vector3(200f,0f,0f);
					Gizmos.DrawCube(pos,Vector3.one);
				}
			}
				
        }
	}*/
}
