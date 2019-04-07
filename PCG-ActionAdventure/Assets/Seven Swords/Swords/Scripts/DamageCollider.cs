using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCollider : MonoBehaviour {
	//handles weapon dealing damage, this script is on the weapon

	PlayerStats stats;
	EnemyStates enemy;

	void Start(){
		stats = GetComponentInParent<PlayerStats> ();
		enemy = GetComponentInParent<EnemyStates> ();
	}

	void OnTriggerEnter(Collider other){
		if (stats != null) { //if this is a player's weapon
			EnemyStates eStates = other.transform.GetComponentInParent<EnemyStates> (); //get enemy script of object you hit

			if (eStates == null) //didnt hit a enemy
				return;

			//do damage
			float dmg = stats.str * 10;
			Weapon weapon = GetComponentInParent<Weapon> ();

			if (weapon) //add weapon damage
				dmg += weapon.damage;
			
			eStates.DoDamage (dmg);
		}

		if (enemy) {
			PlayerStats player = other.transform.GetComponentInParent<PlayerStats> (); //get script of player if there is one

			if (player != null) { //if hit player
				//do damage
				player.DamagePlayer(enemy.str * 10 + GetComponentInParent<Weapon> ().damage, false);
			}
		}
	}
}
