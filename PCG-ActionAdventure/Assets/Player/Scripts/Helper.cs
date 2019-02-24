using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//helps access animator

public class Helper : MonoBehaviour {

	//components for player movement in directions: (vertical is forward, backward and horizontal is left, right)
	[Range(-1,1)]
	public float vertical;
	[Range(-1,1)]
	public float horizontal; //horizontal is strafe component

	public bool playAnim;

	public string[] oh_attacks;
	public string[] th_attacks;

	public bool TwoHanded;
	public bool enableRM;
	public bool interacting;
	public bool pickup;
	public bool lockon;

	Animator anim;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {

		enableRM = !anim.GetBool ("canMove"); //enable root motion only if you can't move
		anim.applyRootMotion = enableRM;

		interacting = anim.GetBool ("interacting");

		if (enableRM)
			return;

		anim.SetBool ("TwoHanded", TwoHanded);
		anim.SetBool ("lockon", lockon);

		if (!lockon) { //if no lockon enabled, dont need to strafe and player doesn't need to go back pedal (so clamp vertical)
			horizontal = 0;
			vertical = Mathf.Clamp01 (vertical);
		}

		if (pickup) { //if pickup is true, play the animation once and reset bool
			anim.Play ("pick_up");
			pickup = false;
			playAnim = false;
			vertical = Mathf.Clamp (vertical, 0, 0.5f); //don't let player move when they are picking up
		}

		if (interacting) { //player is currently interacting so don't let player play animation 
			playAnim = false;
			vertical = Mathf.Clamp (vertical, 0, 0.5f); //don't let player move when they are picking up
		}
			
		if (playAnim) {
			
			string targetAnim;
			if (!TwoHanded) { //pick random attack from list of attacks
				targetAnim = oh_attacks [Random.Range (0, oh_attacks.Length)];
			} else {
				targetAnim = th_attacks [Random.Range (0, th_attacks.Length)];
			}

			vertical = 0;
			anim.CrossFade (targetAnim, 0.2f); //always fade into the animation that is currently set
			playAnim = false;
		}

		anim.SetFloat ("vertical", vertical);
		anim.SetFloat ("horizontal", horizontal);

	}
}
