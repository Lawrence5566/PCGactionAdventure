using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStates : MonoBehaviour {

	public float health;
	public bool isInvincible; //to stop colldier doing damage more than once
	public bool canMove;
	public bool isDead;

	public ElementType type = ElementType.fire; //fire type by default

	public Animator anim;
	AnimatorHook a_hook;
	public Rigidbody rigid;

	public GameObject weapon;
	public Transform rightHand;

	public List<Rigidbody> ragdollRigids = new List<Rigidbody>();
	public List<Collider> ragdollColliders = new List<Collider>();

	void Start(){
		health = 100;

		anim = GetComponentInChildren<Animator> ();

		rigid = GetComponent<Rigidbody> (); //must be called before animator hook

		a_hook = anim.GetComponent<AnimatorHook> ();
		if (a_hook == null) //if no hook, add new one
			a_hook = anim.gameObject.AddComponent<AnimatorHook> ();
		a_hook.Init (null, this);

		InitRagdoll();

		EnemyManager.singleton.enemyTargets.Add (this); //add enemy to manager
	}

	void InitRagdoll(){
		Rigidbody[] rigs = GetComponentsInChildren<Rigidbody>();
		for (int i = 0; i < rigs.Length; i++) {
			if (rigs [i] == rigid) //skip main rigidbody
				continue;

			ragdollRigids.Add (rigs [i]);
			rigs [i].isKinematic = true;

			Collider col = rigs [i].gameObject.GetComponent<Collider> ();
			col.isTrigger = true;
			ragdollColliders.Add (col);
		}
	}

	public void EnableRagdoll(){
		for (int i = 0; i < ragdollRigids.Count; i++) {
			ragdollRigids [i].isKinematic = false;
			ragdollColliders [i].isTrigger = false;
		}

		Collider controllerCollider = rigid.gameObject.GetComponent <Collider> ();
		controllerCollider.enabled = false;
		rigid.isKinematic = true;

		StartCoroutine ("CloseAnimator");
	}

	IEnumerator CloseAnimator(){ //stop ragdoll exploding
		yield return new WaitForEndOfFrame();
		anim.enabled = false;
		this.enabled = false;
	}

	void Update(){
		canMove = anim.GetBool ("canMove");

		if (health <= 0) {
			if (!isDead) {
				isDead = true;
				EnableRagdoll ();
			}
		}

		if (isInvincible) { //wait till you can move again
			isInvincible = !canMove;
		}

		if (canMove)
			anim.applyRootMotion = false;
			
	}

	public void DoDamage(float v){
		if (isInvincible)
			return;

		health -= v;
		isInvincible = true;
		anim.Play ("damage_1");
		anim.applyRootMotion = true;
		anim.SetBool (StaticStrings.canMove, false);
	}

	/*
	public void SetWeapon(GameObject w){
		Transform rightHand = weapon.transform.parent;
		if (weapon != null) { //destroy weapon
			Destroy (weapon);
		}
		//set new weapon (its already instantiated)
		weapon = w;
		w.transform.parent = rightHand;
	}*/
}
