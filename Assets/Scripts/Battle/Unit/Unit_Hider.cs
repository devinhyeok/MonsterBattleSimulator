using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_Hider : Unit
{
    public GameObject skillObject;

    public override void UseSkill()
    {
        if (target)
        {
            base.UseSkill();
            target = GetFarEnemy();
            Vector3 tempDirection = (target.gameObject.transform.position - transform.position).normalized;
            transform.position = target.gameObject.transform.position + tempDirection;

            // 스킬 데미지 설정
            Damage damage = new Damage();
            damage.sourceGameObject = gameObject;
            damage.normalDamage = 250 * currentSpellPower / 100;

            // 스킬 생성
            float angle = Quaternion.FromToRotation(Vector3.right, direction).eulerAngles.z; // 회전 기본값 가져오기
            GameObject tempObject = Instantiate(skillObject, transform.position, Quaternion.Euler(0, 0, angle)); // 스킬 방향이 보는 방향이 일치해야하며, 내 위치에서 생성될 때

            // 스킬 정보 설정
            Skill skill = tempObject.GetComponent<Skill>();
            skill.team = team;
            skill.damage = damage;
        }        
    }
}
