using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    private GameObject player;
    NavMeshAgent navmeshAgent;

    public enum Enemy_Type
    {
        chaser,
        patroller,
        turret
    }
    public Enemy_Type enemyType;
    public float Hp;
    public float Speed;
    public enum Attack_Type
    {
        melee,
        ranged
    }
    public Attack_Type attackType;
    public float AttackRate;
    public int AttackDamage;
    public int ProjectileSpeed;
    public float AggroRadius;
    public List<GameObject> PatrolWaypoints;
    private Vector3 nextTarget;
    public GameObject Projectile;
    public GameObject Loot;
    public List<DropLootSlot> DropLootList; //= new List<DropLootSlot>();

    private float attackTimer;

    //------------------------------------------------------ Setup and runtime function calls  ------------------------------------------------------
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        //If the enemy is a turret enemy, then don't look for the NavMeshAgent
        if(enemyType != Enemy_Type.turret)
        {
            navmeshAgent = this.GetComponent<NavMeshAgent>();
            navmeshAgent.speed = Speed;
        }
    }

    void Update()
    {
        if(enemyType == Enemy_Type.chaser)
        {
            Chase();
        }
        else if (enemyType == Enemy_Type.patroller)
        {
            Patrol();
        }

        Attack();
        Shoot();
    }

    //------------------------------------------------------ Enemy AI (Pathfinding, Ranged and Melee Attacking)  ------------------------------------------------------

    //Move towards player
    public void Chase()
    {
        Vector3 targetVector = player.transform.position;
        navmeshAgent.SetDestination(targetVector);
    }

    //Patrol along waypoints
    public void Patrol()
    {
        
        Vector3 targetVector = nextTarget;
        navmeshAgent.SetDestination(targetVector);
        float distRemaining = Vector3.Distance(transform.position, targetVector);

        if (distRemaining < 1)
        {
            Debug.Log("Destination reached, getting next vector");
            NextWaypoint();
        }
    }

    //returns the next waypoint
    public void NextWaypoint()
    {
        //set the nextTarget waypoint
        nextTarget = PatrolWaypoints[0].transform.position;
    }

    //Fire at player
    public void Shoot()
    {
        //Shoot at player if theyre in the turrets radius
        if (Vector3.Distance(transform.position, player.transform.position) < AggroRadius)
        {
            //track player if in range
            Vector3 lookVector = player.transform.position - transform.position;
            lookVector.y = transform.position.y;
            Quaternion rot = Quaternion.LookRotation(lookVector);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 1);

            //fire at player every X seconds based on FireRate
            if (attackTimer < AttackRate)
            {
                attackTimer += 1 * Time.deltaTime;
            }
            else
            {
                GameObject proj = Instantiate(Projectile, transform.position, transform.rotation);
                proj.GetComponent<Rigidbody>().velocity = transform.TransformDirection(Vector3.forward * ProjectileSpeed);
                //set damage int on projectile from AttackDamage
                proj.GetComponent<EnemyProjectileController>().damage = AttackDamage;
                attackTimer = 0;
            }

        }

    }

    //Attack player
    public void Attack()
    {
        if (attackType == Attack_Type.melee)
        {
            if (Vector3.Distance(transform.position, player.transform.position) < navmeshAgent.stoppingDistance)
            {

                if(attackTimer < AttackRate)
                {
                    attackTimer += 1 * Time.deltaTime;
                }
                else
                {
                    player.GetComponent<PlayerController>().TakeDamage(AttackDamage);
                    attackTimer = 0;
                }
            }
        }


    }

    //------------------------------------------------------ Enemy Stats (Taking damage, dropping loot etc)  ------------------------------------------------------

    //Take damage from player
    public void TakeDamage()
    {
        if (Hp > 1)
        {
            Hp -= 1;
            DamageShader();
        }
        else
        {
            Destroy(gameObject);
            DropLoot();
            //Instantiate explosion particles
        }
    }

    //Apply damage shader to signify player hit
    void DamageShader()
    {
        //Get the original material color
        Color originalColor = gameObject.GetComponent<MeshRenderer>().material.color;

        //Change it to white for a fraction of a second
        gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
        StartCoroutine(ClearShader(originalColor, 0.025f));
    }

    //Clear DamageShader
    IEnumerator ClearShader(Color original, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        gameObject.GetComponent<MeshRenderer>().material.color = original;
    }

    //Drop Random loot from DropLoot list on death
    public void DropLoot()
    {
        //Loop through the DropLoot list, and randomise the chance of spawning X amount of loot
        for (int i = 0; i < DropLootList.Count; i++)
        {
            //Check random chance of loot type dropping
            //float chanceToDrop = Random.Range(0, 1);
            //if (chanceToDrop > DropLootList[i].dropChance)
            //{
                //randomise amount to drop for that item
                int drop = Random.Range(0, DropLootList[i].maxDrop);

                if (drop > 0)
                {
                    Debug.Log(drop + " of " + DropLootList[i].item.item + " dropped");

                    for (int ii = 0; ii < drop; ii++)
                    {
                        //Instantiate item at random coordinate within x radius of self
                        GameObject spawned = Instantiate(Loot, transform.position, Quaternion.identity);

                        //Set item and amount
                        spawned.GetComponent<Item>().itemObj = DropLootList[i].item;
                        spawned.GetComponent<Item>().dropAmount = 1;
                    }
                }
            //}            
        }
    }

}

//Controls the Items and max quantity that can be dropped by this enemy on death
[System.Serializable]
public class DropLootSlot
{
    //Variables held against the Inventory slot
    public ItemObject item;
    public int maxDrop;
    public float dropChance;

    public DropLootSlot(ItemObject _item, int _maxDrop, float _dropChance)
    {
        item = _item;
        maxDrop = _maxDrop;
        dropChance = _dropChance;
    }

}
