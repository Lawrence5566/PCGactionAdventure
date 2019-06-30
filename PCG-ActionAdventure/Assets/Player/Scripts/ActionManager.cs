using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionManager : MonoBehaviour {

	[HideInInspector]
	public List<Action> actionSlots = new List<Action> ();

	public ItemAction consumableItem;

	InputHandler inputHandler;

	public void Init(InputHandler ih){
		inputHandler = ih;

		UpdateActionsWithCurrentWeapon ();
	}

	public void UpdateActionsWithCurrentWeapon(){
		EmptyAllSlots (); //clear all actions first

		Weapon w = inputHandler.inventoryManager.weapon;
		for (int i = 0; i < w.actions.Count; i++) { //loop through all actions in weapon

			Action a = GetAction(w.actions[i].input); //assign animations using weapon actions
			a.targetAnim = w.actions [i].targetAnim;
		}

		inputHandler.isTwoHanded = w.isTwoHanded; //set twoHanded mode on inputhandler to mirror the weapon
	}

	void EmptyAllSlots(){
		for(int i = 0; i < 2; i++){
			Action a = GetAction((ActionInput)i);
			a.targetAnim = null; //make all actions null
		}
	}

	ActionManager(){
		for(int i = 0; i < 2; i++){ //setup actions list
			Action a = new Action();
			a.input = (ActionInput)i;
			actionSlots.Add (a);
		}
	}

	public Action GetActionSlot(InputHandler ih){
		ActionInput a_input = GetActionInput (ih);
		return GetAction (a_input);
	}

	Action GetAction(ActionInput inp){
		for (int i = 0; i < actionSlots.Count; i++) {
			if (actionSlots [i].input == inp)
				return actionSlots [i];
		}

		return null;
	}

	public ActionInput GetActionInput(InputHandler ih){

		if (ih.x_input) //if x button pressed
			return ActionInput.x;
		if (ih.y_input)	//if y button pressed
			return ActionInput.y;
		return ActionInput.x; //return x as default
		
	}


}

public enum ActionInput{
	x,y,rb
}

[System.Serializable]
public class Action{
	public ActionInput input;
	public string targetAnim;
}

[System.Serializable]
public class ItemAction{
	public string targetAnim;
	public string item_id;
}
