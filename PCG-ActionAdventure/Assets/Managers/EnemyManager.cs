using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour {
	float minLockDistance = 20f;
	
	public List<EnemyStates> enemyTargets = new List<EnemyStates> (); //stores list of all enemys

	public EnemyStates GetEnemy(Vector3 from){ //get closest enemy when switching targets
		
		EnemyStates r = null;
		float minDist = float.MaxValue;
		for (int i = 0; i < enemyTargets.Count; i++) {
			float tDist = Vector3.Distance (from, enemyTargets[i].transform.position);
			if (tDist < minDist && tDist < minLockDistance) {
				minDist = tDist;
				r = enemyTargets [i];
			}
		}

		return r;
	}

	public static EnemyManager singleton;
	void Awake(){
		singleton = this;
	}
}
