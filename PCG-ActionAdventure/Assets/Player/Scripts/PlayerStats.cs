using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//only on player object
public class PlayerStats : MonoBehaviour
{
	public float hp = 100;
	public float str = 1;
    
	public void DamagePlayer(float amount, bool isPercentage){
		//percentage values given as decimals
		if (isPercentage) {
			hp -= (hp * amount); 
		} else {
			hp -= amount;
		}
	}
}
