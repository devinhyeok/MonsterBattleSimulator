using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit_WildBoar : Unit
{
    [Header("Unit_WildBoar")]
    public float moveStack;

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void MoveToMovePoint()
    {
        // 이동시 이동 스텍이 쌓임
        base.MoveToMovePoint();
        moveStack += rigidbody.velocity.magnitude;
    }

    public override IEnumerator PlayAttackAnim(float cooltime)
    {
        // 이동 스택에 따라 다른 공격 실행
        if (moveStack > 100)
        {            
            return base.PlaySkillAnim(cooltime);
        }
        else
        {
            return base.PlayAttackAnim(cooltime);
        }        
    }

    public override void UseSkill()
    {
        if (moveStack > 100)
        {
            // 이동 스택에 따라 추가 데미지
            if (target)
            {                                
                Damage damage = new Damage();
                damage.sourceGameObject = gameObject;
                damage.normalDamage = currentAttackPower + moveStack;
                damage.onHit = true;                
                target.ApplyDamage(damage);
                CurrentMana += 10 * currentManaRegen / 100;
                moveStack = 0;
            }
        }
        else
        {
            // 이동 스택가 없으면 넉백 시전
            if (target)
            {
                base.UseSkill();
                Damage damage = new Damage();
                damage.sourceGameObject = gameObject;
                damage.trueDamage = 100f * currentSpellPower / 100;                
                target.ApplyDamage(damage);

                Damage bumpDamage = new Damage();
                bumpDamage.sourceGameObject = gameObject;
                bumpDamage.trueDamage = 100f * currentSpellPower / 100;
                bumpDamage.buffList.Add(new BuffData(BuffType.stun, 2f * currentSpellPower / 100));
                bumpDamage.forcedVelocity = direction * 10f;
                target.ApplyPhysicalDamage(bumpDamage.forcedVelocity, bumpDamage);
            }
        }        
    }
}
