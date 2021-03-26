using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit_Agares : Unit
{
    public override void UseSkill()
    {
        if (target)
        {
            base.UseSkill();
            Damage damage = new Damage();
            damage.sourceGameObject = gameObject;
            damage.normalDamage = 250f * currentSpellPower / 100;
            damage.buffList.Add(new BuffData(BuffType.stun, 1.5f * currentSpellPower / 100));
            target = GetFarEnemy();
            Vector3 tempDirection = (target.gameObject.transform.position - transform.position).normalized;
            transform.position = target.gameObject.transform.position + tempDirection;
            direction = -tempDirection;
            target.ApplyDamage(damage);
        } 
    }
}
