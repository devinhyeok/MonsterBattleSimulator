using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_FogMushroom : Unit
{
    public GameObject skillObject;

    public override void Attack()
    {
        base.Attack();
    }
    public override void UseSkill()
    {
        base.UseSkill();
        GameObject tempObject = Instantiate(skillObject, transform.position, Quaternion.identity); // 스킬 방향이 보는 방향이 일치하지 않으며, 타겟 위치에서 생성될 때
        Skill skill = tempObject.GetComponent<Skill>();
        Damage damage = new Damage();
        damage.buffList.Add(new BuffData(BuffType.blind, 2f * currentSpellPower / 100));
        damage.normalDamage = 225 * currentSpellPower / 100;
        skill.team = team; // 팀 구별 색인자
        skill.damage = damage; // 범위내 적에게 줄 데미지 정보, 틱데미지일경우 한틱당 데미지
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
