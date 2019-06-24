using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//works as State manager and input handler
public class InputHandler : MonoBehaviour {

    PlayerStats stats;

    [Header("Inputs")]
	public float vertical;
	public float horizontal;
	public float moveAmount;
	public Vector3 moveDir;
	public bool b_input; 	//runInput
	public bool a_Input;
	public bool x_input;
	public bool y_input;
	public bool rb_input; 	//right bumper
	public float rt_axis;	//right trigger axis
	public bool rt_input;  //right trigger
	public bool lb_input;
	public bool lt_input;
	public float lt_axis;
	public bool rollInput;
	public bool itemInput;

	bool leftAxis_down;
	bool rightAxis_down;

	float b_timer; //timer for run/roll button

	[Header("Stats")]
	public float moveSpeed = 5; 
	public float rotateSpeed = 8;
	public float toGround = .5f;
	public float rollDistance = 1f; //changes roll speed velocity/distance (increasing this makes character lurch further forward)
    public float rollSpeed = 1f; //changes roll speed (heavy characters should roll slower)
    public float runMulti = 1.6f;

    [Header("States")]
	public bool run;
	public bool onGround; 
	public bool lockon;
	public bool inAction;
	public bool canMove;
	public bool isTwoHanded;
	public bool usingItem;

	[Header("Other")]
	public EnemyStates lockOnTarget;

	public GameObject activeModel;
	[HideInInspector]
	public Animator anim;
	[HideInInspector]
	public Rigidbody rigid;
	[HideInInspector]
	AnimatorHook a_hook; //to access animator 'on animate' calls
	[HideInInspector]
	public ActionManager actionManager;
	[HideInInspector]
	public InventoryManager inventoryManager;


	private CameraManager camManager;

	public LayerMask ignoreLayers;

	float actionDelay;


	// Use this for initialization / Init
	void Start () {
		Setup ();

        stats = GetComponent<PlayerStats>();

        //set up rigid constraints
        rigid.angularDrag = 999;
		rigid.drag = 4;
		rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

		//initialise scripts
		inventoryManager = GetComponent<InventoryManager> (); //initialise before action manager
		inventoryManager.Init ();

		actionManager = GetComponent<ActionManager> ();
		actionManager.Init (this);

		a_hook = activeModel.GetComponent<AnimatorHook> ();
		if (a_hook == null) //if no hook, add new
			a_hook = activeModel.AddComponent<AnimatorHook> ();
		a_hook.Init (this, null);

		camManager = CameraManager.singleton;
		camManager.Init (this); //initialise camera manager and set its target to this transform

		//set object layer
		gameObject.layer = 8;
		ignoreLayers = ~(1 << 9);

	}

	void Setup(){
		if (activeModel == null) {
			Debug.Log ("no active model assinged");
			return;
		} else {
			anim = activeModel.GetComponent<Animator> ();
			if (anim == null) {
				Debug.Log ("no Animator found on model");
				return;
			}
		}

		rigid = GetComponent<Rigidbody> ();
		if (rigid == null) {
			Debug.Log ("no rigidbody found");
			return;
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () { //acts as fixed function
		GetInput ();
		UpdateStates ();
		camManager.Tick ();  //update camera in fixed update to keep things smooth

	}

	void Update(){ //acts as Tick function
		onGround = OnGround ();
		anim.SetBool (StaticStrings.onGround, onGround); 
		ResetInputStates ();
	}

	void ResetInputStates(){
		if (!b_input) 
			b_timer = 0;
		if (rollInput) //make sure roll input only runs for one frame (only one roll)
			rollInput = false;
		if (run) //reset run every frame to make sure it doesn't stick after we lift up run button
			run = false;
	}
		

	void GetInput(){//get inputs
		vertical = Input.GetAxis (StaticStrings.Vertical);
		horizontal = Input.GetAxis (StaticStrings.Horizontal);
		b_input = Input.GetButton (StaticStrings.B);
		x_input = Input.GetButton (StaticStrings.X);
		y_input = Input.GetButton (StaticStrings.Y);
		a_Input = Input.GetButton (StaticStrings.A);
		rt_input = Input.GetButton (StaticStrings.RT);
		rt_axis = Input.GetAxis (StaticStrings.RT);
		rb_input = Input.GetButton (StaticStrings.RB);
		lt_input = Input.GetButton (StaticStrings.LT);
		lt_axis = Input.GetAxis (StaticStrings.LT);
		lb_input = Input.GetButton (StaticStrings.LB);
		rightAxis_down = Input.GetButtonUp (StaticStrings.RightStick);

		//reset input states
		if(rt_axis != 0)
			rt_input = true;
		if(lt_axis != 0)
			lt_input = true;
		if(b_input) //if pressing b button(run/roll button) start timer
			b_timer += Time.deltaTime;
			
	}

	void UpdateStates(){ //updateStates
		//update states
		Vector3 v = vertical * camManager.transform.forward;
		Vector3 h = horizontal * camManager.transform.right;

        if (lockon) {
            if (horizontal > 0.85f) //clamp horizontal("lock-on sidestep") if locked on so player can't run around enemies
                horizontal = 0.85f;
            if (horizontal < -0.85f)
                horizontal = -0.85f;
        }


		moveDir = (v + h).normalized;
		moveAmount = Mathf.Clamp01(Mathf.Abs (horizontal) + Mathf.Abs (vertical));

		itemInput = rb_input;

		if (itemInput)
			b_input = false;

		if (b_input && b_timer > 0.5f) { //if run button pressed and held for more than .5s
			run = (moveAmount > 0); //start running
		} 

		if (b_input == false && b_timer > 0 && b_timer < 0.5f){ //if roll button pressed and held for less that 0.5s
			rollInput = true;
		}

		anim.SetBool (StaticStrings.TwoHanded, isTwoHanded); //make sure two handed mirrors isTwoHanded state

		//fixed tick

		usingItem = anim.GetBool (StaticStrings.interacting); //set using item with animation

		DetectItemAction ();//check if use item button pressed
		DetectAction(); 	//check if attack action buttons pressed

		inventoryManager.curWeapon.weaponModel.SetActive (!usingItem); //make sure weapon isn't hidden

		if (inAction){ //skip rest of function if character is in action (it can't move whilest inAction)
			anim.applyRootMotion = true; //we are in an action so use root motion
			actionDelay += Time.deltaTime;

			if (actionDelay > 0.3f) {
				inAction = false; //stop action after animation finished
				actionDelay = 0;
			} else {
				return; 
			}
		}

		if (lockOnTarget != null) {
			if (lockOnTarget.isDead) { //if target dies (if enemyStates becomes unActive)
				lockon = false;
				lockOnTarget = null;
				camManager.lockon = false;
				camManager.lockonTarget = null;
			}
		}

		if (rightAxis_down) { //if they click the right stick in/press lock on button
			lockon = !lockon;

			lockOnTarget = EnemyManager.singleton.GetEnemy (transform.position); //get new enemy to lock on
			if (lockOnTarget == null) { //not found a target, don't lock on
				lockon = false;
				camManager.lockonTarget = null; 				//set camera lockon target to null
			} else {
				camManager.lockonTarget = lockOnTarget.transform; //set camera target
			}
				
			camManager.lockon = lockon;

		}

		canMove = anim.GetBool (StaticStrings.canMove);
		if (!canMove) { //skip function if character can't move
			return;
		}

		a_hook.rm_multi = 1; //reset root motion multiplier
        stats.invincible = false; // turn off invincibility from rolling if it's on
		HandleRolls (); //check for rolling

		//if we got this far, turn off root motion cus we are moving
		anim.applyRootMotion = false;
		rigid.drag = (moveAmount > 0 || onGround == false) ? 0 : 4; //only drag down if falling or not moving

		float targetSpeed = moveSpeed;
		if (usingItem) {
			run = false; //stop player running if using an item
			//moveAmount = Mathf.Clamp(moveAmount, 0 , 0.5f); //reduce speed to half if using item
		}

		if (run)
			targetSpeed = moveSpeed * runMulti;

        if (onGround)
			rigid.velocity = moveDir * (targetSpeed * moveAmount);

        if (run)
            lockon = false;//if running, unlock


        Vector3 targetDir = (lockon == false)? moveDir 
			: lockOnTarget.transform.position - transform.position; //decide what to rotate towards
	
		targetDir.y = 0; //remove Y incase we rotate upwards
		if (targetDir == Vector3.zero)
			targetDir = transform.forward;
		Quaternion targetRot = Quaternion.LookRotation (targetDir);	 	//create rotation towards target
		targetRot = Quaternion.Slerp (transform.rotation, targetRot, Time.deltaTime * moveAmount * rotateSpeed);	//slerp rotation from current rotation
		transform.rotation = targetRot;

		//update animation
		anim.SetBool (StaticStrings.lockon, lockon);

		if (!lockon) {
			HandleMovementAnimations ();
		} else {
			HandleLockOnAnimations (moveDir);
		}

	}

	void DetectItemAction(){
		if (!canMove || usingItem) //don't try action if charcter cannot move or is already using item
			return;

		if (!itemInput)
			return;

		ItemAction slot = actionManager.consumableItem;
		if (slot == null)
			return;
		string targetAnim = slot.targetAnim;

		if (string.IsNullOrEmpty (targetAnim))
			return;
		
		usingItem = true;
		anim.Play (targetAnim);
	}

	void DetectAction(){
		if (!canMove || usingItem) //don't try action if charcter cannot move
			return;

		//rb_input == false && rt_input == false && lt_input == false && lb_input == false (if you need bumbers and trigger actions example)
		if (x_input == false && y_input == false) {
			return; //no action buttons pressed, just return
		}

		string targetAnim = null;

		Action slot = actionManager.GetActionSlot (this); //get required action from action manager
		if (slot == null) //should never be null
			return;
		targetAnim = slot.targetAnim;

		if (string.IsNullOrEmpty (targetAnim))
			return;

		canMove = false;
		inAction = true; 
		anim.CrossFade (targetAnim, 0.2f);
		//rigid.velocity = Vector3.zero; //stop character moving when in Action
	}

	void HandleRolls(){
		if (!rollInput || usingItem)  //cant roll whilest using item
			return;

		float v = vertical;
		float h = horizontal;

		if (!lockon) { //always make sure you can only roll forward when no lockon
			v = (moveAmount > 0.3f)? 1 : 0;
			h = 0; 
		} else{
			if (Mathf.Abs (v) < 0.3f) //eliminate small amounts of input
				v = 0;
			if (Mathf.Abs (h) < 0.3f)
				h = 0;
		}

        /*
		v = (moveAmount > 0.3f)? 1 : 0; //no blend tree rolling version (char turns and looks in direction to roll)
		h = 0;
		if (v != 0)
			if (moveDir == Vector3.zero)
				moveDir = transform.forward;
			Quaternion targetRot = Quaternion.LookRotation (moveDir);
			transform.rotation = targetRot; 
        */

        a_hook.rm_multi = rollDistance;
        a_hook.rollSpeed = rollSpeed;

		anim.SetFloat (StaticStrings.vertical, v);
		anim.SetFloat (StaticStrings.horizontal, h);

		canMove = false; //disable action (since we are rolling)
        stats.invincible = true;                  //make invincible for 1 frame while rolling
        inAction = true; 
		anim.CrossFade ("Rolls", 0.2f);

	} 

	void HandleMovementAnimations(){
		anim.SetBool (StaticStrings.running, run);
		anim.SetFloat (StaticStrings.vertical, moveAmount, 0.4f, Time.deltaTime); //connect moveAmount to animation vertical
	}

	void HandleLockOnAnimations(Vector3 moveDir){
		Vector3 realtiveDir = transform.InverseTransformDirection(moveDir);
		float v = realtiveDir.z;
		float h = realtiveDir.x;

		anim.SetFloat (StaticStrings.vertical, v, 0.2f, Time.deltaTime);
		anim.SetFloat (StaticStrings.horizontal, h, 0.2f, Time.deltaTime);
		
	}

	public bool OnGround(){
		bool r = false;

		Vector3 origin = transform.position + (Vector3.up * toGround);
		Vector3 dir = -Vector3.up;
		float dis = toGround*2 + 0.3f ; //.3 to ray cast slightly further
		RaycastHit hit;
		if (Physics.Raycast (new Vector3(origin.x, origin.y + toGround, origin.z), dir, out hit, dis, ignoreLayers)) { //if we hit the ground (ignore the layers given)
			r = true;
			Vector3 targetPosition = hit.point;
			transform.position = targetPosition;
		}

		return r;
	}

}
