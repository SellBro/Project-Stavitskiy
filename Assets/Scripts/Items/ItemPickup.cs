﻿using Player;
using UnityEngine;

namespace Items
{
    public class ItemPickup : MonoBehaviour
    {
        [SerializeField] private ItemConfig config = null;
        [SerializeField] private int healthToRestore = 0;

        private void Start()
        {
           if(config != null) config.ResetArmour(); 
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                Debug.Log("Pick");
                Pickup(other.gameObject);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                gameObject.GetComponent<BoxCollider2D>().isTrigger = false;
            } 
        }

        private void Pickup(GameObject subject)
        {
            if (healthToRestore > 0)
            {
                subject.GetComponent<PlayerUnit>().Heal(healthToRestore);
            }
            else if (config != null)
            {
                subject.GetComponent<PlayerMovementController>().EquipItem(config, transform.position);
            }
            
            Destroy(gameObject);
        }
    }
}
