using System;
using System.Collections;
using UnityEngine;

//cellular automa
//source: https://www.youtube.com/watch?v=v7yyZZjF1z4 Sebastian Lague

public class RoomGenerator {

	private int width;
	private int height;

	public string seed; //room seed
	public bool useRandomSeed = true; //set default for random seed

	[Range(0,100)]
	private int randomFillPercent; //% of the room to be filled

	int[,] map;

	public int[,] GenerateRoom(int w, int h, int fill){
		width = w;
		height = h;
		randomFillPercent = fill;

		GenerateMap ();

		return map;
	}

	void GenerateMap(){
		map = new int[width, height];
		RandomFillMap ();

		for (int i = 0; i < 5; i++) { //smooth map 5 times
			SmoothMap ();
		}

		int borderSize = 1; //setting the border of the map (so that it always has walls)
		int[,] borderedMap = new int[width + borderSize*2, height + borderSize*2];

		for (int x = 0; x < borderedMap.GetLength(0); x++) {
			for (int y = 0; y < borderedMap.GetLength(1); y++) {
				if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize) {
					borderedMap [x, y] = map [x - borderSize, y - borderSize]; 
				} else {	//not inside map
					borderedMap[x,y] = 1; //add wall to bordered map
				}
			}
		}
		map = borderedMap; //set new bordered map

	}

	void RandomFillMap(){
		if (useRandomSeed) {
			seed = Time.time.ToString ();
		}

		System.Random pseudoRandom = new System.Random (seed.GetHashCode ());

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (x == 0 || x == width - 1 || y == 0 || y == height - 1) {
					map [x, y] = 1; //put walls on edges
				} else{
					map [x, y] = (pseudoRandom.Next (0, 100) < randomFillPercent) ? 1 : 0; //if less than fill% map[x,y] = 1 else 0
				}
			}
		}

	}

	void SmoothMap(){
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				//how many neighbouring tiles are walls?
				int neighbourWallTiles = GetSurroundingWallCount(x,y);

				//add some simple rules to smooth - if more that 4 walls surrounding a tile, make it a wall
				if (neighbourWallTiles > 4) 
					map [x, y] = 1;
				else if (neighbourWallTiles < 4)
					map[x,y] = 0;
				// leave 4 wall tiles surrounding the same

			}
		}
	}

	int GetSurroundingWallCount (int gridX, int gridY){ //gridx&y are position of tile
		int WallCount = 0;
		for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++) {
			for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++) {
				if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height) { //make sure we are not looking outside map
					if (neighbourX != gridX || neighbourY != gridY) { //(skip original tile)
						WallCount += map [neighbourX, neighbourY];
					}
				} else {
					WallCount++; //border of map so add 1
				}

			}
		}

		return WallCount;
	}
}
