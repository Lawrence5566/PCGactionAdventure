using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//set actions, isTwohanded etc on prefab
public class Weapon : MonoBehaviour {
	public List<Action> actions;
	public bool isTwoHanded;
	public GameObject weaponModel;
	public GameObject damageCollider; //have a list of these if you want multiple colliders per weapon
	public ElementType type;
	public float damage;

	public void OpenDamageColliders(){ //WeaponHook
		damageCollider.SetActive (true);
	}

	public void CloseDamageColliders(){ //WeaponHook
		damageCollider.SetActive (false);
	}
}
