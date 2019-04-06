using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
	public token keyToken;

	InventoryManager playerInv;

	void Start(){
		playerInv = FindObjectOfType<InventoryManager> ();
	}

	void OnCollisionEnter(Collision collision){

		if (playerInv.gameObject == collision.gameObject) { //if hit player
			playerInv.keys.Add(keyToken); 	//give player the key
			gameObject.SetActive(false); 	//turn off object
		}

	}
}
