using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

	public InputHandler inputHan;

	public bool lockon;
	public float followSpeed = 7;
	public float mouseSpeed = 2;
	public float controllerSpeed = 5;

	public Transform playerTarget;
	public Transform lockonTarget;

	public Transform camTrans;
	public Transform pivot;

	public float turnSmoothing = .1f;
	public float minAngle = -35;
	public float maxAngle = 35;

	public float smoothX;
	public float smoothY;
	public float smoothXVelocity;
	public float smoothYVelocity;
	public float lookAngle;
	public float tiltAngle;

	bool changeTargetLeft;
	bool changeTargetRight;

	public void Init(InputHandler ih){
		inputHan = ih;
		playerTarget = ih.transform;

		camTrans = Camera.main.transform;
		pivot = camTrans.parent;
	}

	public void Tick(){
		//mouse input:
		float h = Input.GetAxis ("Mouse X");
		float v = Input.GetAxis ("Mouse Y");

		//controller input:
		float c_h = Input.GetAxis ("RightAxis X");
		float c_v = Input.GetAxis ("RightAxis Y");

		float targetSpeed = mouseSpeed;

		changeTargetLeft = inputHan.rb_input; //switch tab targets using bumpers
		changeTargetRight = inputHan.lb_input;

		if (c_h != 0 || c_v != 0) { //if controller has any input, overide mouse
			h = c_h;
			v = c_v;
		}

		if(lockonTarget != null){
			if (changeTargetLeft || changeTargetRight) {
				//lockonTransform = lockonTarget
			}
		}

		FollowTarget ();
		HandleRotations (v, h, targetSpeed);
	}

	void FollowTarget(){
		float speed = Time.deltaTime * followSpeed;
		Vector3 targetPosition = Vector3.Lerp (transform.position, playerTarget.position, speed);
		transform.position = targetPosition;
	}

	void HandleRotations (float v, float h, float targetSpeed){
		if (turnSmoothing > 0) {
			smoothX = Mathf.SmoothDamp (smoothX, h, ref smoothXVelocity, turnSmoothing);
			smoothY = Mathf.SmoothDamp (smoothY, v, ref smoothYVelocity, turnSmoothing);
		} else {
			smoothX = h;
			smoothY = v;
		}

		tiltAngle += smoothY * targetSpeed; //change this to -= and it will invert the look up/down right stick control
		tiltAngle = Mathf.Clamp (tiltAngle, minAngle, maxAngle);
		pivot.localRotation = Quaternion.Euler (tiltAngle, 0, 0);

		if (lockon && lockonTarget != null) { //lockon cam is different
			Vector3 targetDir = lockonTarget.position - transform.position;
			targetDir.Normalize ();
			//targetDir.y = 0;
			if (targetDir == Vector3.zero) 
				targetDir = transform.forward;
			Quaternion targetRot = Quaternion.LookRotation (targetDir);
			transform.rotation = Quaternion.Slerp (transform.rotation, targetRot, Time.deltaTime * 9);
			lookAngle = transform.eulerAngles.y; //follow our own y if we are locked on
			return;
		}
			
		lookAngle += smoothX * targetSpeed;
		transform.rotation = Quaternion.Euler (0, lookAngle, 0);

	}

	public static CameraManager singleton;
	void Awake(){
		singleton = this;
	}
}
