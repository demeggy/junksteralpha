using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]

public class ItemObject : ScriptableObject
{
    public string item;
    public string description;
    public int stack;
    public int value;
}
