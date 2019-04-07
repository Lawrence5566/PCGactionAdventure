using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
	public Vector3 teleLocation = Vector3.zero;
	PlayerStats player;

	void Start(){
		player = FindObjectOfType<PlayerStats> ();
	}

	void OnCollisionEnter(Collision collision){
		if (player.gameObject == collision.gameObject) {
			player.transform.position = teleLocation;
		}
	}
}
