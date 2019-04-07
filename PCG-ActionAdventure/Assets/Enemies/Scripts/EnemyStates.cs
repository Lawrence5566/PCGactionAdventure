using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStates : MonoBehaviour {

	public float health = 100; //make health scale size of enemy?

	public float startHP;
	public healthbarController healthBar;

	public float str = 1; //these need to have an impact
	public float def = 1;
	public float speed = 1;

	public float attackSpeed = 1;

	public bool isInvincible; //to stop colldier doing damage more than once
	public bool canMove;
	public bool isDead;
	public bool canAttack = true;
	float attackTimer;
	public float attackGap = 2; //by default, enemies have a 2 second attack gap
	public float attackRange = 2;

	public ElementType type = ElementType.fire; //fire type by default

	public Animator anim;
	AnimatorHook a_hook;
	public Rigidbody rigid;

	public GameObject weapon;
	public Transform rightHand;
	string attackAction;

	public List<Rigidbody> ragdollRigids = new List<Rigidbody>();
	public List<Collider> ragdollColliders = new List<Collider>();

	PlayerStats player;
	Vector3 startLocation; //is this used?

	public Weapon weaponScript;

	void Start(){
		health += 100; //make health scale size of enemy?
		startHP += 100;
		//str = 1;
		//def = 1;
		//speed = 1;

		anim = GetComponentInChildren<Animator> ();

		rigid = GetComponent<Rigidbody> (); //must be called before animator hook

		a_hook = anim.GetComponent<AnimatorHook> ();
		if (a_hook == null) //if no hook, add new one
			a_hook = anim.gameObject.AddComponent<AnimatorHook> ();
		a_hook.Init (null, this);

		InitRagdoll();

		EnemyManager.singleton.enemyTargets.Add (this); //add enemy to manager

		//give a this a weapon
		//EnemyManager.singleton.weaponManager.GiveWeapon (this, ElementType.fire, 20, SwordType.broadsword); //for testing, give all enemys same weapon

		player = FindObjectOfType<PlayerStats> ();
		startLocation = transform.position;

		weaponScript = weapon.GetComponent<Weapon> ();
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

		if (Time.time > attackTimer) {
			canAttack = true;
		}

		if (health <= 0) {
			if (!isDead) {
				isDead = true;
				EnableRagdoll ();
			}
		}

		if (isInvincible) { //wait till you can move again
			isInvincible = !canMove;
		}

		if (canMove) {
			anim.applyRootMotion = false;


			//if player is within attack range, attack him
			if (Vector3.Distance (this.transform.position, player.transform.position) < attackRange){
				anim.SetBool (StaticStrings.running, false); //stop running 
				anim.SetFloat (StaticStrings.vertical, 0f, 0.4f, Time.deltaTime); //stop character

				//rotate towards enemy
				Vector3 targetDir = player.transform.position - transform.position; //target to rotate to

				targetDir.y = 0; //remove Y incase we rotate upwards
				if (targetDir == Vector3.zero)
					targetDir = transform.forward;
				Quaternion targetRot = Quaternion.LookRotation (targetDir);	 	//create rotation towards target
				targetRot = Quaternion.Slerp (transform.rotation, targetRot, Time.deltaTime * speed * 2);	//slerp rotation from current rotation
				transform.rotation = targetRot;

				//attack
				string stringAttackAnim = weaponScript.actions[Random.Range(0, weaponScript.actions.Count)].targetAnim;

				if (canAttack) { //if we can, attack
					anim.speed = attackSpeed;
					anim.CrossFade (stringAttackAnim, 0.2f);
					canAttack = false;
					attackTimer = attackGap + Time.time; //reset attack timer

					//turn off root motion for attack
					anim.applyRootMotion = false;
				}

			} else if (Vector3.Distance (this.transform.position, player.transform.position) < 12) { //or follow player if he is close enough
				Vector3 targetDir = player.transform.position - transform.position; //target to rotate to

				targetDir.y = 0; //remove Y incase we rotate upwards
				if (targetDir == Vector3.zero)
					targetDir = transform.forward;
				Quaternion targetRot = Quaternion.LookRotation (targetDir);	 	//create rotation towards target
				targetRot = Quaternion.Slerp (transform.rotation, targetRot, Time.deltaTime * speed * 2);	//slerp rotation from current rotation
				transform.rotation = targetRot;

				anim.SetBool (StaticStrings.running, true); //start running 
				anim.SetFloat (StaticStrings.vertical, speed / 10, 0.4f, Time.deltaTime); 

				rigid.velocity = targetDir * speed; //speed * move amount?

			} else {
				//stop on the spot or move to start location
				anim.SetBool (StaticStrings.running, false); //start running 
				anim.SetFloat (StaticStrings.vertical, -0.001f, 0.4f, Time.deltaTime); //slow character down 
			}

		}
			
	}

	public void DoDamage(float v){
		if (isInvincible)
			return;

		health -= v;
		isInvincible = true;
		anim.Play ("damage_1");
		anim.applyRootMotion = true;
		anim.SetBool (StaticStrings.canMove, false);

		healthBar.SetSize (health / startHP);
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
