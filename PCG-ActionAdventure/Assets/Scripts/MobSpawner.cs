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
		new option ("monster","none", 12, 2),
		new option ("monster","none", 18, 3),
		new option ("monster","none", 24, 4),
		new option ("monster","none", 30, 5)
	};

	List<option> statOptions = new List<option> (){ //each stat costs 2 points
		new option("mod", "str",    3, 1),
		new option("mod", "def",    3, 1),
		new option("mod", "speed",  3, 1),
		new option("mod", "hp",     3, 20),
		
	};

    /* //old options:
        new option("mod", "str", 	7, 2),
		new option("mod", "def", 	7, 2),
		new option("mod", "speed", 	7, 2),
		new option("mod", "hp",		7, 30),
		new option("mod", "str",    12, 3),
		new option("mod", "def",    12, 3),
		new option("mod", "speed",  12, 3),
		new option("mod", "hp",     12, 45) 
    */

    //levelPointsValueModifier example: - formula is: 3(2*mod - 1) + 3 * (taskNo - 1)
    //eg if value modifier is 1:
    //1st task = 3, 2nd task = 6, 3rd task = 9
    //if 2:
    //1st task = 9, 2nd task = 12, 3rd task = 15
    //if 3:
    //1st task = 15, 2nd task = 18, 3rd task = 21, 
    //21, 24, 27

    //this means a levelPointsValueModifier of 1 guarantees no lvl2 in the first 3,
    //value of 2 guarantees oppertunity for lvl2 in 2nd task
    //value of 3 guarantees oppertunity for lvl3 in 2nd task

    public void createStack(int levelPointsValueModifier, List<Vector3> locations, int bossNumber){
        // task is a number of enemies to overcome

        Debug.Log("---  CREATE STACK ---");

        taskLocations = locations;

		List<int> enemyValueArray = new List<int>(); 	//each value array is points assigned for each task (first task player encounters to last)
		for (int i = 1; i <= taskLocations.Count; i++) {
			enemyValueArray.Add(3 * (2 * levelPointsValueModifier - 1) + 3 * (i - 1)); 			//each task points to scale by the stage in the game (early enemies are easyer) - uses above formula
		}

		if (bossNumber != -1){ //-1 = no boss
			if (enemyValueArray[bossNumber] < 30){
				enemyValueArray [bossNumber] = 30; 		//give task number with boss at least 30 points so it can spawn a boss (boss is 30 points min to spawn)
			}
		}

		string output = "";
		foreach (int i in enemyValueArray) {
			output += i + ",";
		}

		//now generate mobs for each task
		for (int i = 0; i < enemyValueArray.Count; i++){ 
			int currPointsLeft = enemyValueArray[i];
            Debug.Log(" -- task " + (i+1) + " -- " + "points for this task: " + currPointsLeft + " --");
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

                if (currTaskStack.FindAll(n => n.type == "monster").Count > 3) { //if more than 3 monsters already, remove monsters as a choice as we all reaaady have too many for player to handle
                    possibleOptions.RemoveAll(item => item.type == "monster");
                }


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
        //TaskStack = a set of options to spawn at a LOCATION

        List<EnemyStates> enemiesInTask = new List<EnemyStates>();

		foreach (option o in TaskStack) {
			
			Debug.Log(o.type + "," + o.modType + ", " + o.value);

			//spawn all the enemies for the task first
			if (o.type == "monster"){
				GameObject newEnemy = Instantiate(enemyPrefabs [Random.Range (0, enemyPrefabs.Length)], taskLocations[locationNo], Quaternion.identity);
                //newEnemy.transform.localScale = new Vector3(1f, 1f, 1f) * (1f + (o.value * 2f / 10f)); //use level to set enemy scale

                //set new enemy stats:
                EnemyStates enemy = newEnemy.GetComponent<EnemyStates> ();
				enemy.attackRange = 2f + o.value / 2f; //set attack range by level (bigger enemys need more)
                enemy.str = o.value; //starting stats = enemy level
                enemy.def = o.value;
                enemy.speed = o.value;
                enemy.startHP = 80 + (o.value * 20);
                enemy.level = o.value;

                if (o.value == 5) { //if boss
                    enemy.attackSpeed = 1.5f;
                    enemy.startHP = 300.0f; //manual set starthp for bosses
                }

                float bonusHealth = enemy.startHP - 100;
                newEnemy.transform.localScale = new Vector3(1f, 1f, 1f) * (1 + ( bonusHealth / 200)); //use bonus health (health over 100) to scale size

                enemiesInTask.Add (enemy);
			}
		}

		foreach (option o in TaskStack) { //now add modifiers to random enemies in stack (so its ditributied randomly)
			int i = Random.Range(0, enemiesInTask.Count);
			if (o.type == "mod") {
				switch (o.modType){
				case "str":
					enemiesInTask [i].str += o.value;
					break;
				case "def":
					enemiesInTask [i].def += o.value;
					break;
				case "speed":
					enemiesInTask [i].speed += o.value;
					break;
				case "hp":
					enemiesInTask [i].startHP += o.value;
					break;
				default:
					break;
				}
			}
		}


        // give weapons:
        foreach (EnemyStates enemy in enemiesInTask) {

            int level = enemy.level;
            List<SwordType> possibleChoices = checkMods((enemy.hp - (80 + (level * 20))), enemy.str - level, enemy.speed - level, enemy.def - level);

            int weaponDamage = 5;
            if (enemy.level == 5) {
                weaponDamage = 20;
                if (possibleChoices.Contains(SwordType.broadsword))
                    possibleChoices.Remove(SwordType.broadsword);
                if (possibleChoices.Count == 0)
                    possibleChoices.Add(SwordType.katana); // if only weapon left is standard broadsword, add katana instead
            }

            //pick weapon randomly from choices
            EnemyManager.singleton.weaponManager.GiveWeapon(enemy, ElementType.none, weaponDamage, possibleChoices[Random.Range(0, possibleChoices.Count)]);
        }

    }

    //str, def, speed. hp

    //broadsword = the base sword / hp sword
    //katana = the str + speed sword
    //longsword = the str + def sword
    //rapier = the speed sword
    //sabre = the def sword
    //scimitar = the speed + def sword
    //ulfberht = the str sword

    //function to determine list of options for enemy weapon
    List<SwordType> checkMods(float hp, float str, float speed, float def) {
        List<SwordType> possibleChoices = new List<SwordType>();

        if (str > 0)
            possibleChoices.Add(SwordType.ulfberht);
        if (speed > 0)
            possibleChoices.Add(SwordType.rapier);
        if (def > 0)
            possibleChoices.Add(SwordType.scimitar);
        if (str > 0 && speed > 0) 
            possibleChoices.Add(SwordType.katana);
        if (str > 0 && def > 0) 
            possibleChoices.Add(SwordType.longsword);
        if (speed > 0 && def > 0) 
            possibleChoices.Add(SwordType.sabre);

        //modifications to this:
        //if containing any of the combination weapons:
        if (possibleChoices.Contains(SwordType.katana) || possibleChoices.Contains(SwordType.longsword) || possibleChoices.Contains(SwordType.sabre)) {
            possibleChoices.Remove(SwordType.ulfberht);
            possibleChoices.Remove(SwordType.rapier);
            possibleChoices.Remove(SwordType.scimitar);
        }
            
        //use braodsword as default
        if (possibleChoices.Count == 0)
            possibleChoices.Add(SwordType.broadsword);

        return possibleChoices;
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
