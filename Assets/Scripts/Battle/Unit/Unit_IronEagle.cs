using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit_IronEagle : Unit
{
    [Header("IronEagle")]
    public GameObject autoAttackObject;
    public GameObject skillObject;

    public override void Attack()
    {
        // 데미지 설정
        Damage damage = new Damage();
        damage.normalDamage = currentAttackPower;
        damage.sourceGameObject = gameObject;

        // 스킬 콜리전 생성
        float angle = Quaternion.FromToRotation(Vector3.right, direction).eulerAngles.z; // 회전 기본값 가져오기        
        GameObject tempAutoAttackObject = Instantiate(autoAttackObject, transform.position, Quaternion.Euler(0, 0, angle));
        Skill skill = tempAutoAttackObject.GetComponent<Skill>();
        SkillMovement_Target skillMovement = tempAutoAttackObject.GetComponent<SkillMovement_Target>();
        SpriteRenderer SkillSpriteRenderer = tempAutoAttackObject.transform.Find("Sprite").GetComponent<SpriteRenderer>();

        // 스킬 콜리전 설정
        skill.team = team; // 팀 구별 색인자
        skill.damage = damage; // 범위내 적에게 줄 데미지 정보, 틱데미지일경우 한틱당 데미지

        // 타겟 설정
        (skill as SkillProjectile_Once).target = target.gameObject;
        skillMovement.target = target.gameObject;

        // 스킬 활성화
        skillMovement.Play();
        CurrentMana += 10 * currentManaRegen / 100;
    }

    public override void UseSkill()
    {
        base.UseSkill();

        for(int i = -2; i <= 2; i++)
        {
            // 데미지 설정
            Damage damage = new Damage();
            damage.normalDamage = currentAttackPower;
            damage.sourceGameObject = gameObject;

            // 스킬 콜리전 생성
            float angle = Quaternion.FromToRotation(Vector3.right, direction).eulerAngles.z; // 회전 기본값 가져오기        
            GameObject tempSkillObject = Instantiate(skillObject, transform.position, Quaternion.Euler(0, 0, angle + i * 10));
            Skill skill = tempSkillObject.GetComponent<Skill>();
            SkillMovement_Path skillMovement = tempSkillObject.GetComponent<SkillMovement_Path>();
            SpriteRenderer SkillSpriteRenderer = tempSkillObject.transform.Find("Sprite").GetComponent<SpriteRenderer>();

            // 스킬 콜리전 설정
            skill.team = team; // 팀 구별 색인자
            skill.damage = damage; // 범위내 적에게 줄 데미지 정보, 틱데미지일경우 한틱당 데미지

            // 스킬 활성화
            skillMovement.Play();
        }        
    }
}
