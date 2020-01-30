using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    public GameObject spawn;
    private ItemObject itemToSpawn;

    public List<ItemObject> Items_Cargo;
    public List<ItemObject> Items_Science;
    public List<ItemObject> Items_Security;
    public List<ItemObject> Items_Alien;

    public List<Text> InvSlot;
    public GameObject UI_PlayerInv;

    private GameObject player;

    public void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    //drops loot based on type and location
    public void SpawnLoot(GameObject lootSource)
    {
        //Get a random Item from the list and attach it to this spawned loot
        itemToSpawn = Items_Cargo[Random.Range(0, Items_Cargo.Count)];
        GameObject spawned = Instantiate(spawn, lootSource.transform.position, Quaternion.identity);

        //Set item and amount
        spawned.GetComponent<Item>().itemObj = itemToSpawn;
        spawned.GetComponent<Item>().dropAmount = Random.Range(1, 5);
    }

}
