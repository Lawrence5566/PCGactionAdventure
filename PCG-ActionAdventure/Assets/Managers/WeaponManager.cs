﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour {
	[Header("Testing settings")]
	public EnemyStates temp_Enemy; //for testing
	public ElementType temp_Type;
	public int temp_Dmg;
	public SwordType temp_SwordType;

	public GameObject broadswordPrefab;
	public GameObject katanaPrefab;
	public GameObject longswordPrefab;
	public GameObject rapierPrefab;
	public GameObject sabrePrefab;
	public GameObject scimitarPrefab;
	public GameObject ulfberhtPrefab;

	public Material fireMat;
	public Material waterMat;
	public Material earthMat;
	public Material airMat;

	public static WeaponManager singleton;
	void Awake(){
		singleton = this;
	}

	void Start(){
		GiveWeapon (temp_Enemy, temp_Type, temp_Dmg, temp_SwordType);
	}
		
	public void GiveWeapon(EnemyStates enemy, ElementType type, int dmg, SwordType swordType){
		GameObject newWeapon = new GameObject();
		Destroy (newWeapon); //temporary fix to spawning new game object (not sure how to set empty newWeapons) 

		switch (swordType) {
		case SwordType.broadsword:
			newWeapon = broadswordPrefab; //creates weapon in world
			break;
		case SwordType.katana:
			newWeapon = katanaPrefab;
			break;
		case SwordType.longsword:
			newWeapon = longswordPrefab;
			break;
		case SwordType.rapier:
			newWeapon = rapierPrefab;
			break;
		case SwordType.sabre:
			newWeapon = sabrePrefab;
			break;
		case SwordType.scimitar:
			newWeapon = scimitarPrefab;
			break;
		case SwordType.ulfberht:
			newWeapon = ulfberhtPrefab;
			break;
		}
			
		Weapon WeaponScript = newWeapon.GetComponent<Weapon>(); //get its script from prefab
		WeaponScript.CloseDamageColliders(); //make sure damage colliders are off before giving to enemy (otherwise it hurts the enemy)

		//set weapon type, if it has one
		switch (type) {
		case ElementType.fire:
			WeaponScript.type = ElementType.fire;
			WeaponScript.GetComponent<MeshRenderer> ().material = fireMat;
			break;
		case ElementType.water:
			WeaponScript.type = ElementType.water;
			WeaponScript.GetComponent<MeshRenderer> ().material = waterMat;
			break;
		case ElementType.earth:
			WeaponScript.type = ElementType.earth;
			WeaponScript.GetComponent<MeshRenderer> ().material = earthMat;
			break;
		case ElementType.air:
			WeaponScript.type = ElementType.air;
			WeaponScript.GetComponent<MeshRenderer> ().material = airMat;
			break;
		}

		//set damage
		WeaponScript.damage = dmg;

		enemy.SetWeapon (newWeapon); //give to enemy
	}
}

public enum SwordType{
	broadsword, katana, longsword, rapier, sabre, scimitar, ulfberht
}

public enum ElementType{
	none, fire, water, earth, air
}
