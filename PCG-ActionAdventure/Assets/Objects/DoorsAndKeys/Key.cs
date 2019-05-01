using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
	public token keyToken;

	void OnCollisionEnter(Collision collision){

		InventoryManager playerInv = collision.gameObject.GetComponent<InventoryManager> ();

		if (playerInv){ //if hit player
			playerInv.keys.Add(keyToken); 	//give player the key
			gameObject.SetActive(false); 	//turn off object
		}

	}
}
