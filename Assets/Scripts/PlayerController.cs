using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{

    public float speed;
    public float jumpHeight;
    private Rigidbody rb;

    private int hp;
    public int maxHp;
    public int energy;

    public Text UI_Hp;

    private Vector3 moveInput;
    private Vector3 moveVelocity;

    private Camera mainCamera;

    public GameObject projectile;
    public GameObject projectileSpawner;

    public float meleeTimer;
    public float rangedTimer;

    public int MaxInvSlots;

    public List<InvSlot> InvContainer = new List<InvSlot>();

    public List<GameObject> UI_Slot;
    public GameObject UI_PlayerInv;

    //------------------------------------------------------ Setup and runtime function calls  ------------------------------------------------------
    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = FindObjectOfType<Camera>();
        hp = maxHp;
    }

    public void Update()
    {
        Status();
        Movement();
        Jump();
        Attack();
        Debug.Log(Input.GetAxisRaw("Horizontal"));
        //PickupItem();
        OpenInventory();
    }

    public void FixedUpdate()
    {
        rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);
    }

    public void OnTriggerEnter(Collider other)
    {
        var loot = other.GetComponent<Item>();
        if (loot)
        {
            PickupItem(other.gameObject, loot.itemObj.item, loot.itemObj.description, loot.itemObj.stack, loot.itemObj.value, loot.dropAmount);
            //Destroy(other.gameObject);
        }
    }

    //------------------------------------------------------ Player Input (Movement, combat, UI navigation etc)  ------------------------------------------------------

    //Player status (Health, energy etc)
    public void Status()
    {
        //Update UI HP value
        UI_Hp.text = hp + "/" + maxHp;
    }

    public void TakeDamage(int amount)
    {
        if(hp > hp - amount)
        {
            hp -= amount;
            DamageShader();
        }
        else
        {
            //KillPlayer
            //Instantiate PlayerLoot item, move all items into its container
        }        
    }

    //Moves the player
    public void Movement()
    {
        moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        moveVelocity = moveInput * speed;
    }

    //Jumps over obstacles
    public void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(new Vector3(0, jumpHeight, 0), ForceMode.Impulse);
        }
    }

    //Radial Melee and Ranged Combat with mouse tracking
    public void Attack()
    {
        //Make player face mouse position
        Ray cameraRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayLength;

        if (groundPlane.Raycast(cameraRay, out rayLength))
        {
            Vector3 pointToLook = cameraRay.GetPoint(rayLength);
            Debug.DrawLine(cameraRay.origin, pointToLook, Color.green);

            transform.LookAt(new Vector3(pointToLook.x, transform.position.y, pointToLook.z));
        }

        //Make player fire ranged weapon
        if (Input.GetMouseButtonDown(0))
        {
            GameObject proj = Instantiate(projectile, projectileSpawner.transform.position, projectileSpawner.transform.rotation);
            proj.GetComponent<Rigidbody>().velocity = transform.TransformDirection(Vector3.forward * 10);
        }

        //Make player attack with melee weapon
        if (Input.GetMouseButtonDown(1))
        {
            Collider[] hit = Physics.OverlapSphere(transform.position, 0.75f);
            foreach (Collider go in hit)
            {
                //Damage enemies
                if (go.transform.gameObject.tag == "Enemy")
                {
                    go.gameObject.GetComponent<EnemyController>().TakeDamage();
                }

                //Open loot
                if (go.transform.gameObject.tag == "Loot")
                {
                    go.gameObject.GetComponent<LootChest>().DropLoot();
                }
            }
        }
    }

    //Apply damage shader to signify damage
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

    //------------------------------------------------------ Player Inventory Management ------------------------------------------------------

    //Add a target item to InvContainer
    public void PickupItem(GameObject loot, string item, string desc, int stack, int value, int amount)
    {
        bool hasItem = false;

        //Loop through each items in the InvContainer
        for (int i = 0; i < InvContainer.Count; i++)
        {
            //If item already exists in the InvContainer
            if (InvContainer[i].slot_item == item && InvContainer[i].slot_amount < InvContainer[i].slot_stack)
            {
                //If the existing slot amount + the item amount is less than the stack maximum, add it to the existing stack
                if (InvContainer[i].slot_amount + amount <= InvContainer[i].slot_stack)
                {
                    InvContainer[i].addAmount(amount);
                    Destroy(loot);
                    hasItem = true;
                    break;
                }
                //Else get the difference, and add the difference to the existing stack, then if there is a spare slot, create a new stack with the remaining amount
                else
                {

                    int diff = InvContainer[i].slot_stack - InvContainer[i].slot_amount;
                    int amountRemaining = amount - diff;

                    InvContainer[i].addAmount(diff);
                    if (InvContainer.Count < MaxInvSlots)
                    {
                        InvContainer.Add(new InvSlot(item, desc, stack, value, amountRemaining));
                        Destroy(loot);
                    }
                    else
                    {
                        //modify the amount value of the loot item so you leave behind the amount you cannot pick up
                        loot.GetComponent<Item>().dropAmount = amountRemaining;
                    }
                    hasItem = true;
                    break;
                }
            }
        }

        //If the item doesnt currently exist in the InvContainer, and there are spare slots, add the item with the drop amount
        if (!hasItem)
        {
            if(InvContainer.Count < MaxInvSlots)
            {
                InvContainer.Add(new InvSlot(item, desc, stack, value, amount));
                Destroy(loot);
            }            
        }

        RefreshInventory();
    }

    //Toggle UI_PlayerInv on/off
    public void OpenInventory()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!UI_PlayerInv.activeSelf)
            {
                UI_PlayerInv.SetActive(true);
            }
            else
            {
                UI_PlayerInv.SetActive(false);
            }
        }
    }

    //Refresh UI_Slot[x] values
    public void RefreshInventory()
    {
        //Populate slots with text from the container
        for (int i = 0; i < InvContainer.Count; i++)
        {
            if (InvContainer[i] != null)
            {
                UI_Slot[i].GetComponentInChildren<Text>().text = InvContainer[i].slot_amount + " x " + InvContainer[i].slot_item;
            }
        }

        //Clear any empty slots
        for (int i = InvContainer.Count; i < UI_Slot.Count; i++)
        {
            UI_Slot[i].GetComponentInChildren<Text>().text = "";
        }
    }

    //Deletes a slot from the inventory (called from UI button)
    public void DeleteInventory(GameObject Slot)
    {
        //get slotIndex from button clicked
        int slotIndex = UI_Slot.IndexOf(Slot);

        Debug.Log(InvContainer[slotIndex].slot_item + " x " + InvContainer[slotIndex].slot_amount);

        //remove InvContainer from list
        InvContainer.RemoveAt(slotIndex);
        RefreshInventory();
    }

}

//------------------------------------------------------ InvSlot class for storing item pickups ------------------------------------------------------
[System.Serializable]
public class InvSlot
{
    //Variables held against the Inventory slot
	public string slot_item;
    public string slot_description;
    public int slot_stack;
    public int slot_value;
    public int slot_amount;

    //Variables passed into the inventory slot
    public InvSlot(string item_text, string item_desc, int stack, int value, int amount)
    {
        slot_item = item_text;
        slot_description = item_desc;
        slot_stack = stack;
        slot_value = value;
        slot_amount = amount;
    }

    public void addAmount(int value)
    {
        slot_amount += value;
    }
}