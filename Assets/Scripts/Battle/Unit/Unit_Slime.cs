using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit_Slime : Unit
{
    public GameObject SpawnObject;

    public override void Attack()
    {
        base.Attack();
        CurrentMana = 0;
    }

    public override void ApplyDamage(Damage damage)
    {
        base.ApplyDamage(damage);
        CurrentMana = 0;
    }

    public override void Dead()
    {
        base.Dead();
        for(int i = -1; i <= 1; i++)
        {
            Unit spawnUnit = Instantiate(SpawnObject, transform.position + direction * i, Quaternion.identity).GetComponent<Unit>();            
            spawnUnit.team = team;
            spawnUnit.aiState = AIState.idle;
            spawnUnit.Level = Level;
        }        
    }
}
