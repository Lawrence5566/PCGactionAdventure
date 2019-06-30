using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pickup : MonoBehaviour
{
    public enum pickupType { health, item}

    public pickupType myPickupType;
    public float healthValue = 30.0f; //for if this is a health pickup

    void OnCollisionEnter(Collision collision) {

        InventoryManager playerInv = collision.gameObject.GetComponent<InventoryManager>();

        if (playerInv) { //if hit player
            if (myPickupType == pickupType.health) {
                collision.gameObject.GetComponent<PlayerStats>().GiveHealth(healthValue); //give health         
            }

            if (myPickupType == pickupType.item) {

                //if it's a weapon, add appropraite weapon script when you spawn the prefab
                Weapon weapon = GetComponentInChildren<Weapon>();
                if (weapon) {
                    WeaponManager.singleton.GiveWeapon(playerInv, weapon.type, weapon.damage, weapon.swordType);
                }
                
            }

            gameObject.SetActive(false); //turn off object
            Object.Destroy(this.gameObject); //destroy object

        }

    }
}

