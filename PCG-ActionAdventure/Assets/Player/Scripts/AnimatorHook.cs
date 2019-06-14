using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorHook : MonoBehaviour {

	Animator anim;
	InputHandler inputHan;
	EnemyStates eStates; //if on a enemy
	Rigidbody rigid;

	public float rm_multi; //root motion multiplier
    public float rollSpeed; //roll speed multiplier

	public void Init(InputHandler ih, EnemyStates eSt){
		inputHan = ih;
		eStates = eSt;
		if (ih != null) {
			anim = ih.anim;
			rigid = ih.rigid;
		}
		if (eSt != null) {
			anim = eSt.anim;
			rigid = eSt.rigid;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnAnimatorMove(){
		if (inputHan == null && eStates == null) 
			return;

		if (rigid == null) //needs a rigid body
			return;

		if (inputHan != null){
			if (inputHan.canMove)
				return;
		}

		if (eStates != null){
			if (eStates.canMove)
				return;
		}

		rigid.drag = 0; //set drag to zero since we are using root motion

		if (rm_multi == 0)
			rm_multi = 1;

		Vector3 delta = anim.deltaPosition;
		delta.y = 0;
		Vector3 v = (delta * rm_multi) / Time.deltaTime;
		rigid.velocity = v;

        //set roll speed (set it here incase it changes (slowing effects etc))
        anim.SetFloat("rollSpeed", rollSpeed);

	}

	public void OpenDamageColliders(){ //only open colliders when we want to hit something
		if (inputHan == null) { //if you have no inputHandler, then this is on an enemy
			eStates.weaponScript.OpenDamageColliders ();
			return;
		}
		

		inputHan.inventoryManager.curWeapon.OpenDamageColliders();
	}

	public void CloseDamageColliders(){
		if (inputHan == null) {
			eStates.weaponScript.CloseDamageColliders ();
			return;
		}
		
		inputHan.inventoryManager.curWeapon.CloseDamageColliders();
	}
}
