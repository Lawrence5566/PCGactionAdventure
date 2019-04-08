using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobSpawner : MonoBehaviour {
	//public GameObject DiaEnemyPrefab;
	//public Material[] DiaMaterials;

	//int minPoints = 3; //must be changed manually

	public GameObject[] enemyPrefabs;

	List<Vector3> taskLocations = new List<Vector3>();

	List<option> mobOptions = new List<option> (){
		new option ("monster","none", 3, 1),
		new option ("monster","none", 7, 2),
		new option ("monster","none", 12, 3),
		new option ("monster","none", 18, 4),
		new option ("monster","none", 30, 5)
	};

	List<option> statOptions = new List<option> (){
		new option("mod", "str",    3, 1),
		new option("mod", "def",    3, 1),
		new option("mod", "speed",  3, 1),
		new option("mod", "hp",     3, 15),
		new option("mod", "str", 	7, 2),
		new option("mod", "def", 	7, 2),
		new option("mod", "speed", 	7, 2),
		new option("mod", "hp",		7, 30),
		new option("mod", "str",    12, 3),
		new option("mod", "def",    12, 3),
		new option("mod", "speed",  12, 3),
		new option("mod", "hp",     12, 45)
	};
		
	//levelPointsValueModifier example:
	//if value modifier is 3:
	//1st task = 3, 2nd task = 6, 3rd task = 9

	public void createStack(int levelPointsValueModifier, List<Vector3> locations, int bossNumber){
		// task is a number of enemies to overcome

		taskLocations = locations;

		List<int> enemyValueArray = new List<int>(); 	//each value array is points assigned for each task (first task player encounters to last)
		for (int i = 1; i <= taskLocations.Count; i++) {
			enemyValueArray.Add(levelPointsValueModifier * i); 			//each task gets 'levelPointsValueModifier' points multiplied by the stage in the game (early enemies are easyer)
		}

		if (bossNumber != -1){ //-1 = no boss
			if (enemyValueArray[bossNumber] < 30){
				enemyValueArray [bossNumber] = 30; 		//give task number with boss at least 30 points so it can spawn a boss (boss is 30 points min to spawn)
			}
		}

		/*
		foreach (int i in enemyValueArray) { //test output
			Debug.Log (i);
		}
		*/

		//now generate mobs for each task
		for (int i = 0; i < enemyValueArray.Count; i++){ 
			int currPointsLeft = enemyValueArray[i];
			List<option> currTaskStack = new List<option>();

			do { //keep selecting things for the task till we run out of points
				if (bossNumber == i){ //this task has a boss so add one first
					currTaskStack.Add(mobOptions[4]);										//boss is last item in mobOptions 
					currPointsLeft = currPointsLeft - 30;
				}
			
				//create new possible options list to be filtered giving it all the current possibles
				List<option> possibleOptions = new List<option>(mobOptions); //by copy, so original mobOptions not altered

				if (currTaskStack.Count > 0){ 	//if stack > 0 a monster has already been selected, so allow stat options
					possibleOptions.AddRange(statOptions);
				}								//else continue without stats

				//filter options:
				possibleOptions.RemoveAll(item => item.pointsCost > currPointsLeft); //remove all items where pointsCost is > points left (filter out too expensive options)

				//now pick choice from possible options
				if (possibleOptions.Count > 0){  //is there any options left to pick?
					option choice = possibleOptions[Random.Range(0, possibleOptions.Count)]; //get choice string by selecting randomly from options
					currTaskStack.Add(choice);
					currPointsLeft = currPointsLeft - choice.pointsCost; //subtract points from total
				} else{
					//Debug.Log("no options, points left: " + currPointsLeft);
				}
					
			} while(currPointsLeft > 2); //lowest item = 3

			//now we are left with a set of options, that need to be translated into monsters + stat buffs and added to the game:
			AddToStack(currTaskStack, i);

			currTaskStack.Clear ();
		}

	}

	void AddToStack(List<option> TaskStack, int locationNo){
		//TaskStack = a set of options to spawn at a location
		List<EnemyStates> enemiesInTask = new List<EnemyStates>();

		foreach (option o in TaskStack) {
			//for testing:
			Debug.Log(o.type + "," + o.modType + ", " + o.value);


			//spawn all the enemies for the task
			if (o.type == "monster"){
				GameObject newEnemy = Instantiate(enemyPrefabs [Random.Range (0, enemyPrefabs.Length)], taskLocations[locationNo], Quaternion.identity);
				newEnemy.transform.localScale = new Vector3 (1f, 1f, 1f) * (1f + (o.value*2f/10f)); //use value (or level in this case) to set enemy scale

				EnemyStates enemy = newEnemy.GetComponent<EnemyStates> ();
				enemy.attackRange = 2f + o.value / 2f; //set attack range by level (bigger enemys need more)

				if (o.value >= 3) { //if larger monster, give different weapon, change attack speed
					EnemyManager.singleton.weaponManager.GiveWeapon (enemy, ElementType.fire, 20, SwordType.katana);

					if (o.value == 5){ //if boss
						enemy.attackSpeed = 1.5f;
						enemy.health += 100f;
						enemy.startHP = enemy.health; //reset start hp
					}

				} else {
					EnemyManager.singleton.weaponManager.GiveWeapon (enemy, ElementType.fire, 5, SwordType.broadsword); //give other enemys normal weapons
				}

				//Debug.Log(EnemyManager.singleton.weaponManager);

				enemiesInTask.Add (newEnemy.GetComponent<EnemyStates> ());


			}
		}

		foreach (option o in TaskStack) { //add modifiers to random enemies in stack (so its ditributied randomly)
			int i = Random.RandomRange(0, enemiesInTask.Count);
			if (o.type == "mod") {
				switch (o.modType){
				case "str":
					enemiesInTask [i].str += o.value;
					break;
				case "def":
					enemiesInTask [i].def += o.value;
					break;
				case "speed":
					enemiesInTask [i].speed += o.value/2;
					break;
				case "hp":
					enemiesInTask [i].health += o.value;
					enemiesInTask [i].startHP += o.value;
					break;
				default:
					break;

				}
			}
		}

	}
}

class option{
	public string type; //'monster' or 'mod'
	public string modType; // if mod - hp,str,def,speed,
	public int pointsCost;
	public int value; //if its a monster its level, if its a mod, its mod value

	public option(string t, string mod, int p, int v){
		type = t;
		modType = mod;
		pointsCost = p;
		value = v;
	}
}
