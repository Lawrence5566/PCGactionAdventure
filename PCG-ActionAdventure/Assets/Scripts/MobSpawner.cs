using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobSpawner : MonoBehaviour {
	public GameObject DiaEnemyPrefab;
	public Material[] DiaMaterials;

	public List<GameObject> monsterStack = new List<GameObject>();

	//int minPoints = 3; //must be changed manually

	List<KeyValuePair<int, string>> mobOptions = new List<KeyValuePair<int, string>> (){
		new KeyValuePair<int, string> (3, "lvl1mob"),
		new KeyValuePair<int, string> (7, "lvl2mob"),
		new KeyValuePair<int, string> (12,"lvl3mob"),
		new KeyValuePair<int, string> (18,"lvl4mob"),
		new KeyValuePair<int, string> (30,"lvl5mob")
	};

	List<KeyValuePair<int, string>> statOptions = new List<KeyValuePair<int, string>> (){
		new KeyValuePair<int, string> (3,  "+1str"),
		new KeyValuePair<int, string> (3,  "+1mag"),
		new KeyValuePair<int, string> (3,  "+1def"),
		new KeyValuePair<int, string> (3,  "+1mdef"),
		new KeyValuePair<int, string> (3,  "+5hp"),
		new KeyValuePair<int, string> (7,  "+2str"),
		new KeyValuePair<int, string> (7,  "+2mag"),
		new KeyValuePair<int, string> (7,  "+2def"),
		new KeyValuePair<int, string> (7,  "+2mdef"),
		new KeyValuePair<int, string> (7,  "+10hp"),
		new KeyValuePair<int, string> (12, "+3str"),
		new KeyValuePair<int, string> (12, "+3mag"),
		new KeyValuePair<int, string> (12, "+3def"),
		new KeyValuePair<int, string> (12, "+3mdef"),
		new KeyValuePair<int, string> (12, "+14hp")
	};
	/*
	void Start(){ //for testing 
		createStack(3,3,0); //create a stack of 3 points value, 3 monster tasks, no boss 
		
	}*/
		
	//levelPointsValueModifier example:
	//if level 1: 3, 1st task = 3, 2nd task = 6, 3rd task = 9

	public void createStack(int levelPointsValueModifier, int taskNumber, int bossNumber){
		// task is a number of enemies to overcome

		List<int> enemyValueArray = new List<int>(); 	//each value array is points assigned for each task (first task player encounters to last)
		for (int i = 1; i <= taskNumber; i++) {
			enemyValueArray.Add(levelPointsValueModifier * i); 			//each task gets 'levelPointsValueModifier' points multiplied by the stage in the game (early enemies are easyer)
		}

		if (bossNumber != 0){ //0 = no boss
			if (enemyValueArray[bossNumber] < 30){
				enemyValueArray [bossNumber] = 30; 		//give task number with boss at least 30 points so it can spawn a boss (boss is 30 points min to spawn)
			}
		}

		//now generate mobs for each task
		for (int i = 1; i <= enemyValueArray.Count; i++){ //starts at 1 so that if boss == 0 we don't spawn a boss
			int currPointsLeft = enemyValueArray[i-1];
			List<KeyValuePair<int, string>> currTaskStack = new List<KeyValuePair<int, string>>();

			do { //keep selecting things for the task till we run out of points
				if (bossNumber == i){ //this level has a boss so add one first
					currTaskStack.Add(mobOptions[4]);										//boss is last item in mobOptions 
					currPointsLeft = currPointsLeft - 30;
				}
			
				//create new possible options list to be filtered giving it all the current possibles
				List<KeyValuePair<int, string>> possibleOptions = new List<KeyValuePair<int, string>>(mobOptions); //by copy, so original mobOptions not

				if (currTaskStack.Count > 0){ 	//if stack > 0 a monster has already been selected, so allow stat options
					possibleOptions.AddRange(statOptions);
				}								//else continue without stats

				//filter options:
				possibleOptions.RemoveAll(item => item.Key > currPointsLeft); //remove all items where key is > points left (filter out too expensive options)

				//now pick choice from possible options
				if (possibleOptions.Count > 0){  //is there any options left to pick?
					KeyValuePair<int, string> choice = possibleOptions[Random.Range(0, possibleOptions.Count)]; //get choice string by selecting randomly from options
					currTaskStack.Add(choice);
					currPointsLeft = currPointsLeft - choice.Key; //subtract points from total
				} else{
					Debug.Log("no options - points left: " + currPointsLeft);
				}
					
			} while(currPointsLeft > 2); //lowest item = 3

			//now we are left with a set of options, that need to be translated into monsters + stat buffs and added to the game:
			AddToStack(currTaskStack);

			currTaskStack.Clear ();
		}

	}

	public void AddToStack(List<KeyValuePair<int, string>> TaskStack){
		//add monsters to 'monsterStack'
		Debug.Log ("Task: ");
		foreach (KeyValuePair<int, string> key in TaskStack) {
			Debug.Log (key.Key + " : " + key.Value);
		}
	}
}
	
