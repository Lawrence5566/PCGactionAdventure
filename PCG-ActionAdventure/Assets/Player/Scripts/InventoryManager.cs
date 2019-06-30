using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour {

	public Weapon curWeapon;
	public List<token> keys = new List<token>();

	public void Init(){
		curWeapon.CloseDamageColliders (); //close damage collider initially of current weapon
	}

    public void GiveWeapon(Weapon weapon) {

    }

}
