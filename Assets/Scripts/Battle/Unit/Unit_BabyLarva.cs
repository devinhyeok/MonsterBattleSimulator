using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit_BabyLarva : Unit
{
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
}
