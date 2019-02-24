using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCollider : MonoBehaviour {

	void OnTriggerEnter(Collider other){
		EnemyStates eStates = other.transform.GetComponentInParent<EnemyStates> (); //get enemy script of object you hit

		if (eStates == null) //didnt hit a enemy
			return;

		//do damage
		float dmg = 0;
		Weapon weapon = GetComponentInParent<Weapon>();

		if (weapon)
			dmg = weapon.damage;
		
		eStates.DoDamage(dmg);
	}
}
