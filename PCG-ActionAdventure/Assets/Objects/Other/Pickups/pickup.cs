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
                gameObject.SetActive(false);    //turn off object
                Object.Destroy(this.gameObject); //destroy object
            }

            if (myPickupType == pickupType.item) {
                //playerInv.Values.Add(keyToken);   //give player this item
                //gameObject.SetActive(false);  
                //destroy object
            }

        }

    }
}

