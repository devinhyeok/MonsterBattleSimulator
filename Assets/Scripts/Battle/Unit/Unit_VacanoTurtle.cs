using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit_VacanoTurtle : Unit
{
    [Header("VacanoTurtle")]
    public SkillCollider_Dot skillCollider;    

    public override void Update()
    {        
        base.Update();
        if (aiState != AIState.none)
        {
            skillCollider.team = team;
            skillCollider.damage.magicDamage = 20 * currentSpellPower / 100;
            skillCollider.gameObject.SetActive(true);
        }
        else
        {
            skillCollider.gameObject.SetActive(false);
        }
        
    }

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
