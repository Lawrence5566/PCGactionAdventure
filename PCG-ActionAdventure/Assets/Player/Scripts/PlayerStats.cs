using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//only on player object
public class PlayerStats : MonoBehaviour
{
	public float hp = 100;
	private float startHp;
	public float str = 2f;
	float damageTimer;

	public healthbarController healthBar;

	void Start(){
		startHp = hp;
	}

    public void GiveHealth(float amount) {
        hp += amount;

        if (hp > 100) //make sure health never goes over 100
            hp = 100;

        healthBar.SetSize(hp / startHp); //set health bar
    }
    
	public void DamagePlayer(float amount, bool isPercentage){
		if (Time.time < damageTimer) //don't damage if the player has just been damaged
			return;

		//percentage values given as decimals
		if (isPercentage) {
			hp -= (hp * amount); 
		} else {
			hp -= amount;
		}

		healthBar.SetSize (hp / startHp); //set health bar

		if (hp <= 0) {
			//player dies, this is basic atm
			GetComponentInChildren<Animator>().SetBool("dead", true);
			GetComponentInChildren<InputHandler> ().enabled = false;
		}

		GetComponentInChildren<Animator> ().CrossFade ("damage_1", 0.2f);

		//set timer
		damageTimer = Time.time + 0.75f;
	}
}
