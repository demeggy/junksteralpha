using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootChest : MonoBehaviour
{
    public GameObject loot;

    //Drop loot when damaged
    public void DropLoot()
    {
        //Instantiate(loot, transform.position, Quaternion.identity);
        //add itemobject to instantiated loot based on ship type
        FindObjectOfType<InventoryController>().SpawnLoot(gameObject);
        Destroy(gameObject);
    }

}
