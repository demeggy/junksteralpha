using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "NPCs/Enemy")]

public class EnemyObject : ScriptableObject
{
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
    public int AttackStrength;
    public float AggroRadius;
    public GameObject Projectile;
    public List<DropLootSlot> DropLoot;

}
