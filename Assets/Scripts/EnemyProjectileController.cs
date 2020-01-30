using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectileController : MonoBehaviour
{
    public float speed;
    private Rigidbody rb;
    public int damage;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //Damage enemies
        if (other.gameObject.tag == "Player")
        {
            other.gameObject.GetComponent<PlayerController>().TakeDamage(damage);
            Destroy(gameObject);
        }

        //Only destroy projectile if it hitsa wall
        if (other.transform.gameObject.tag == "Wall")
        {
            Destroy(gameObject);
        }
    }
}
