using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockedEnemyAnalyser : MonoBehaviour
{
	public InputHandler playerInput;
	EnemyStates currTarget;

	public Text level, hp, str, def, speed, attSpeed;

	void Start(){
		if (!playerInput)
			playerInput = FindObjectOfType<InputHandler> ();	
	}
		
    void FixedUpdate()
    {
		currTarget = playerInput.lockOnTarget;
		if (currTarget) { //if we have a target
			level.text = currTarget.level.ToString();
			hp.text = currTarget.hp.ToString();
			str.text = currTarget.str.ToString();
			def.text = currTarget.def.ToString();
			speed.text = currTarget.speed.ToString();
			attSpeed.text = currTarget.attackSpeed.ToString();
		} else {
			level.text = "";
			hp.text = "";
			str.text = "";
			def.text = "";
			speed.text = "";
			attSpeed.text = "";
		}
    }


}
