using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_Eyeclops : Unit
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
            damage.normalDamage = target.maxHp * 0.05f * currentAbilityPower / 100;
            target.ApplyDamage(damage);

            Damage bumpDamage = new Damage();
            bumpDamage.sourceGameObject = gameObject;
            bumpDamage.normalDamage = target.maxHp * 0.25f * currentAbilityPower / 100;
            bumpDamage.buffList.Add(new BuffData(BuffType.stun, 2f));
            Vector2 velocity = target.transform.position - transform.position;
            target.ApplyPhysicalDamage(velocity, bumpDamage);
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
