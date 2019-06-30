using System.Collections;
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
		
	}
		
	public void GiveWeapon(EnemyStates enemy, ElementType type, int dmg, SwordType swordType){
		GameObject newWeapon = new GameObject();
		Destroy (newWeapon); //temporary fix to spawning new game object (not sure how to set empty newWeapons) 

		switch (swordType) {
		case SwordType.broadsword:
			newWeapon = Instantiate(broadswordPrefab, enemy.rightHand); //creates weapon in world
			break;
		case SwordType.katana:
			newWeapon = Instantiate(katanaPrefab, enemy.rightHand);
			break;
		case SwordType.longsword:
			newWeapon = Instantiate(longswordPrefab, enemy.rightHand);
			break;
		case SwordType.rapier:
			newWeapon = Instantiate(rapierPrefab, enemy.rightHand);
			break;
		case SwordType.sabre:
			newWeapon = Instantiate(sabrePrefab, enemy.rightHand);
			break;
		case SwordType.scimitar:
			newWeapon = Instantiate(scimitarPrefab, enemy.rightHand);
			break;
		case SwordType.ulfberht:
			newWeapon = Instantiate(ulfberhtPrefab, enemy.rightHand);
			break;
		}

		Destroy (enemy.weapon); //destroy old weapon
		enemy.weapon = newWeapon;
			
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
	}

    public void GiveWeapon(InventoryManager playerInv, ElementType type, float dmg, SwordType swordType) { //overload for giving a weapon to the player
        GameObject newWeapon = new GameObject();
        Destroy(newWeapon); //temporary fix to spawning new game object (not sure how to set empty newWeapons) 

        switch (swordType) {
            case SwordType.broadsword:
                newWeapon = Instantiate(broadswordPrefab, playerInv.rightHand); //creates weapon in world, oj player's righthand
                break;
            case SwordType.katana:
                newWeapon = Instantiate(katanaPrefab, playerInv.rightHand);
                break;
            case SwordType.longsword:
                newWeapon = Instantiate(longswordPrefab, playerInv.rightHand);
                break;
            case SwordType.rapier:
                newWeapon = Instantiate(rapierPrefab, playerInv.rightHand);
                break;
            case SwordType.sabre:
                newWeapon = Instantiate(sabrePrefab, playerInv.rightHand);
                break;
            case SwordType.scimitar:
                newWeapon = Instantiate(scimitarPrefab, playerInv.rightHand);
                break;
            case SwordType.ulfberht:
                newWeapon = Instantiate(ulfberhtPrefab, playerInv.rightHand);
                break;
        }

        Destroy(playerInv.weapon.gameObject); //destroy old weapon
        Destroy(playerInv.weapon); //destroy old weapon script

        Weapon WeaponScript = newWeapon.GetComponent<Weapon>(); //get its script from prefab
        WeaponScript.CloseDamageColliders(); //make sure damage colliders are off before giving to enemy (otherwise it hurts the enemy)

        playerInv.weapon = WeaponScript;

        //set weapon type, if it has one
        switch (type) {
            case ElementType.fire:
                WeaponScript.type = ElementType.fire;
                WeaponScript.GetComponent<MeshRenderer>().material = fireMat;
                break;
            case ElementType.water:
                WeaponScript.type = ElementType.water;
                WeaponScript.GetComponent<MeshRenderer>().material = waterMat;
                break;
            case ElementType.earth:
                WeaponScript.type = ElementType.earth;
                WeaponScript.GetComponent<MeshRenderer>().material = earthMat;
                break;
            case ElementType.air:
                WeaponScript.type = ElementType.air;
                WeaponScript.GetComponent<MeshRenderer>().material = airMat;
                break;
        }

        //set damage
        WeaponScript.damage = dmg;

        //giving new weapon to palyer, so update actions
        playerInv.GetComponent<ActionManager>().UpdateActionsWithCurrentWeapon();
    }
}

public enum SwordType{
	broadsword, katana, longsword, rapier, sabre, scimitar, ulfberht
}

public enum ElementType{
	none, fire, water, earth, air
}
