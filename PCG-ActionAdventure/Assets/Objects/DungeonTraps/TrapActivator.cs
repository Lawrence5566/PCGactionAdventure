using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))] //needs to be where collider is for collsion events
public class TrapActivator : MonoBehaviour
{
	public Animation animationComonent;

	float time = 0;
	float timer = 0;
	//Collider thisCollider;

	PlayerStats player;

	//place on every trap prefab
	//if distance to player is less than 5 meters, activate trap
	//if palyer collides with trap collider (make one for each trap and attach to this script )
	//deal 35% of players max hp (so if they are under 35 they will just die)

    // Start is called before the first frame update
    void Start()
    {
		player = FindObjectOfType<PlayerStats> ();
		//thisCollider = GetComponentInChildren<Collider> ();
		animationComonent = GetComponentInParent<Animation> ();
		animationComonent.Stop (); //by default don't start animation
    }

    // Update is called once per frame
    void Update()
    {
		time += Time.deltaTime;

		//check if player is less than 5 meters away
		if(Vector3.Distance(this.transform.position, player.transform.position) < 10){
			animationComonent.Play (); //activate trap! (only plays once, so we can keep calling this while player is close to loop)
		} 
    }

	void OnCollisionEnter(Collision collision){
		if (player.gameObject == collision.gameObject) {
			Debug.Log ("collided");
		}
		
		if (player.gameObject == collision.gameObject && time >= timer) { //if hit player and its been at least 2 seconds since last damage
			//apply damage to player
			player.DamagePlayer (30f, false);

			timer = time + 2;	//reset timer to 2 seconds in the future
			Debug.Log ("apply damage on " + this.gameObject.name);
		}

	}

}
