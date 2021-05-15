using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit_Reaper : Unit
{
    public override void UseSkill()
    {
        if (target)
        {
            base.UseSkill();
            Damage damage = new Damage();
            damage.sourceGameObject = gameObject;
            damage.trueDamage = 200f * currentSpellPower / 100;
            damage.buffList.Add(new BuffData(BuffType.hurt, 3f * currentSpellPower / 100));
            target = GetWeakEnemy();
            Vector3 tempDirection = (target.gameObject.transform.position - transform.position).normalized;
            transform.position = target.gameObject.transform.position + tempDirection;
            direction = -tempDirection;
            target.ApplyDamage(damage);
        }
    }
}
