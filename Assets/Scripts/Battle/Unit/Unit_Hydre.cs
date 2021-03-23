using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_Hydre : Unit
{
    public override void Attack()
    {
        base.Attack();
    }
    public override void UseSkill()
    {        
        if (target)
        {
            base.UseSkill();
            Damage damage = new Damage();
            damage.sourceGameObject = gameObject;
            damage.normalDamage = 400f * currentAbilityPower / 100;
            target.ApplyDamage(damage);
        }        
    }
    public override void Dead()
    {
        base.Dead();
    }
    public override void ApplyDamage(Damage damage)
    {
        base.ApplyDamage(damage);
    }
}
