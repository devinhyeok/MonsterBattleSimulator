using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit_Bear : Unit
{
    public override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Damage damage = new Damage();
            damage.normalDamage = 100f;
            ApplyDamage(damage);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (CurrentHealth / maxHealth < 0.3f)
        {
            Damage damage = new Damage();
            damage.buffList.Add(new BuffData(BuffType.attackArmorUp2, 0.2f));
            damage.buffList.Add(new BuffData(BuffType.attackSpeedUp3, 0.2f));
            damage.buffList.Add(new BuffData(BuffType.attackPowerUp3, 0.2f));
            ApplyDamage(damage);
        }
    }

    public override void UseSkill()
    {
        base.UseSkill();
        if (target)
        {
            Damage damage = new Damage();
            damage.sourceGameObject = gameObject;
            damage.normalDamage = currentAttackPower * 2;
            damage.lifeSteal = 0.5f;
            target.ApplyDamage(damage);
        }                
    }

}
