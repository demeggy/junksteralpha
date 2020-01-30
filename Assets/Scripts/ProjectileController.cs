using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public float speed;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //Damage enemies
        if (other.gameObject.tag == "Enemy")
        {
            other.gameObject.GetComponent<EnemyController>().TakeDamage();
        }

        //Open loot
        if (other.transform.gameObject.tag == "Loot")
        {
            other.gameObject.GetComponent<LootChest>().DropLoot();
        }

        //Always destroy projectile when it hits something
        if (other.transform.gameObject.tag != "Item")
        {
            Destroy(gameObject);
        }            
    }

}
