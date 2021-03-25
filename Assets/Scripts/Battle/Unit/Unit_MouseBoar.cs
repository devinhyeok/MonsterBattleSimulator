using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_MouseBoar : Unit
{
    public override void Attack()
    {
        base.Attack();
    }
    public override void UseSkill()
    {
        base.UseSkill();
        Damage damage = new Damage();
        damage.sourceGameObject = gameObject;
        damage.increaseHp = 300 * currentSpellPower / 100;
        ApplyDamage(damage);
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
