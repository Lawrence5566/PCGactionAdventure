using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionManager : MonoBehaviour {

	private List<Action> actionSlots = new List<Action> (); //slots for actions the player has, contains actions mapped to inputs but no target anims at start (see constructor below), keep this private (otherwise it turns to 2?)

	public ItemAction consumableItem;

	InputHandler inputHandler;

	public void Init(InputHandler ih){
		inputHandler = ih;

		UpdateActionsWithCurrentWeapon ();
	}

    ActionManager() {
        for (int i = 0; i < 3; i++) { //setup actions list, needs to be the same number as number of inputs you want (e.g x,y,rb = 3)
            Action a = new Action();
            a.input = (ActionInput)i;
            a.targetAnim = null;
            actionSlots.Add(a);
        }
    }

    public void UpdateActionsWithCurrentWeapon(){

        EmptyAllSlots (); //clear all actions first

		Weapon w = inputHandler.inventoryManager.weapon;
        for (int i = 0; i < w.actions.Count; i++) { //loop through all actions in weapon

            Action a = GetAction(w.actions[i].input); //foreach action slot the player has
            a.targetAnim = w.actions [i].targetAnim; //set target anim to be the weapons target anim of that slot
            
		}

		inputHandler.isTwoHanded = w.isTwoHanded; //set twoHanded mode on inputhandler to mirror the weapon
	}

	void EmptyAllSlots(){
		foreach(Action a in actionSlots) { //make all actions null
            a.targetAnim = null;
		}
	}

	public Action GetActionSlot(InputHandler ih){
		ActionInput a_input = GetActionInput (ih);
		return GetAction (a_input);
	}

	Action GetAction(ActionInput inp){ //return action, given actionInput button enum type
		for (int i = 0; i < actionSlots.Count; i++) { //for each actionSlot
			if (actionSlots [i].input == inp) //if the action slot has the same ActionInput as the ActionInput given, return the action in that slot
				return actionSlots [i];
		}

		return null;
	}

	public ActionInput GetActionInput(InputHandler ih){
		if (ih.x_input) //if x button pressed
			return ActionInput.x;
		if (ih.y_input)	//if y button pressed
			return ActionInput.y;
		if (ih.rb_input)
            return ActionInput.rb;
        return ActionInput.x; //reuturn x as default
		
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
