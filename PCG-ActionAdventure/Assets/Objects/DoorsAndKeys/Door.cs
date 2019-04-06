using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
	public token keyToken; //set by game manager on creation

	InventoryManager playerInv;

	Animator anim;

	void Start(){
		playerInv = FindObjectOfType<InventoryManager>();
		anim = GetComponent<Animator> ();
	}

	void Update(){
		
		//check if player is less than 10 meters away and has the key
		if(Vector3.Distance(this.transform.position, playerInv.transform.position) < 10 && playerInv.keys.Contains(keyToken)){
			anim.SetBool ("isOpen", true);//open door!
		} 
	}
    
}
