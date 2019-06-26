using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStates : MonoBehaviour {

	public float startHP = 100; //set this in mobspawner
	public float hp;
	public healthbarController healthBar;

	public float str = 1;
	public float def = 1;
	public float speed = 1;
	public int level = 1;

	public float attackSpeed = 1;

	public bool isInvincible; //to stop colldier doing damage more than once
	public bool canMove;
	public bool isDead;
	public bool canAttack = true;
	float attackTimer;
	public float attackGap = 2; //by default, enemies have a 2 second attack gap
	public float attackRange = 2;
    private float currSpeed = 0; //value between 0-maxSpeed that increase each frame (modeling acceleration)
    public float accelerationAmount = 0.05f;

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

	public Weapon weaponScript;

	void Start(){
		hp = startHP;
		anim = GetComponentInChildren<Animator> ();

		rigid = GetComponent<Rigidbody> (); //must be called before animator hook

		a_hook = anim.GetComponent<AnimatorHook> ();
		if (a_hook == null) //if no hook, add new one
			a_hook = anim.gameObject.AddComponent<AnimatorHook> ();
		a_hook.Init (null, this);

		InitRagdoll();

		EnemyManager.singleton.enemyTargets.Add (this); //add enemy to manager

		player = FindObjectOfType<PlayerStats> ();
		//startLocation = transform.position; //not used?

		weaponScript = weapon.GetComponent<Weapon> ();
		weaponScript.CloseDamageColliders (); //initially close damage colliders
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

		if (hp <= 0) {
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

            float maxSpeed = 0.65f + speed / 10.0f;

            if (currSpeed < maxSpeed)
                currSpeed += accelerationAmount; // increase speed by each frame (acceleration)

            //if player is within attack range, attack him
            if (Vector3.Distance (this.transform.position, player.transform.position) <= attackRange){
				anim.SetBool (StaticStrings.running, false); //stop running 
				anim.SetFloat (StaticStrings.vertical, 0f, 0.4f, Time.deltaTime); //stop character
                currSpeed = 0; //stop acceleration when attacking

				//rotate towards enemy
				Vector3 targetDir = player.transform.position - transform.position; //target to rotate to

				targetDir.y = 0; //remove Y incase we rotate upwards
				if (targetDir == Vector3.zero)
					targetDir = transform.forward;
				Quaternion targetRot = Quaternion.LookRotation (targetDir);	 	//create rotation towards target
				targetRot = Quaternion.Slerp (transform.rotation, targetRot, Time.deltaTime * 50);  //when aiming, "snap" towards player
				transform.rotation = targetRot;

				//attack
				string stringAttackAnim = weaponScript.actions[Random.Range(0, weaponScript.actions.Count)].targetAnim; //pick random anim

				if (canAttack) { //if we can, attack
                    Debug.Log("can attack");
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
				targetRot = Quaternion.Slerp (transform.rotation, targetRot, Time.deltaTime * currSpeed * 15);	//slerp rotation from current rotation
				transform.rotation = targetRot;

                if (currSpeed > 1)
                    anim.SetBool (StaticStrings.running, true); //start running if vertical goes above 1 (only enemies that can run this fast run)

				anim.SetFloat (StaticStrings.vertical, currSpeed + 0.2f, 0.4f, Time.deltaTime); //set locomotion blend tree

                rigid.velocity = targetDir * (maxSpeed * currSpeed); //accelerate towards target 

			} else {
				//stop on the spot or move to start location
				anim.SetBool (StaticStrings.running, false); //stop running if we are
				anim.SetFloat (StaticStrings.vertical, -0.01f, 0.4f, Time.deltaTime); //slow character down over time
                currSpeed = currSpeed - 0.01f; //declccelerate
                //rigid.velocity = rigid.velocity - 1.0f; //declccelerate?
            }

		}
			
	}

	public void DoDamage(float v){
		if (isInvincible) //removed damaging animation and invincibility frames
			return;

        hp -= v * 1/def; //incoming damage modified by 1/def
        // 100 incoming at 1 def = 100 * 1/1 = 100
        // 100 incoming at 2 def = 100 * 1/2 = 50
        // 100 incoming at 3 def = 100 * 1/3 = 33

        isInvincible = true;
        if (!(level == 5)) { //if not a boss (bosses don't stagger)
            anim.Play("damage_1");
            anim.applyRootMotion = true;
            anim.SetBool(StaticStrings.canMove, false);
        }


		healthBar.SetSize (hp / startHP);
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
