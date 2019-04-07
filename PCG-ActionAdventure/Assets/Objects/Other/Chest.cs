using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{

	public token keyToken;
	InventoryManager playerInv;

    // Start is called before the first frame update
    void Start()
    {
		playerInv = FindObjectOfType<InventoryManager>();
    }

    // Update is called once per frame
    void Update()
    {
		//check if player is less than 3 meters away and has the key
		if(Vector3.Distance(this.transform.position, playerInv.transform.position) < 3 && playerInv.keys.Contains(keyToken)){
			Debug.Log ("chest open");			//can open the chest!
		} 
    }
}
