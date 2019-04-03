using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))] //needs to be where collider is for collsion events
public class TrapActivator : MonoBehaviour
{
	float time = 0;
	float timer = 0;
	Collider thisCollider;
	//place on every trap prefab
	//if distance to player is less than 5 meters, activate trap
	//if palyer collides with trap collider (make one for each trap and attach to this script )
	//deal 50% of players max hp (so if they are under 50 they will just die)

    // Start is called before the first frame update
    void Start()
    {
		thisCollider = GetComponentInChildren<Collider> ();
    }

    // Update is called once per frame
    void Update()
    {
		time += Time.deltaTime;
    }

	void OnCollisionEnter(){
		if (time >= timer) { //if its been at least 2 seconds since last damage
			//apply damage to player
			timer = time + 2;//reset timer to 2 seconds in the future
			Debug.Log ("apply damage on " + this.gameObject.name);
		}
	}

}
