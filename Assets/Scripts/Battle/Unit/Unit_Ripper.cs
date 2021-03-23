using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_Ripper : Unit
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
            damage.normalDamage = 225f * currentAbilityPower / 100;
            damage.buffList.Add(new BuffData(BuffType.hurt, 3f * currentAbilityPower / 100));
            target = GetFarEnemy();
            Vector3 tempDirection = (target.gameObject.transform.position - transform.position).normalized;
            transform.position = target.gameObject.transform.position + tempDirection;
            direction = -tempDirection;
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
