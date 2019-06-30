using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour {

	public List<token> keys = new List<token>();
    public Transform rightHand; // to connect weapons
    public Weapon weapon;

    public void Init(){
        if (!rightHand) //if no righthand linked
            transform.Find("RightHand");
        if (!weapon)
            GetComponentInChildren<Weapon>();

        weapon.CloseDamageColliders (); //close damage collider initially of current weapon
	}

}
