using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit_Phoenix : Unit
{
    [Header("Phoenix")]
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
        GameObject tempObject = Instantiate(skillObject, transform); // 스킬 방향이 보는 방향이 일치하지 않으며, 타겟 위치에서 생성될 때
        Skill skill = tempObject.GetComponent<Skill>();
        Damage damage = new Damage();
        damage.sourceGameObject = gameObject;
        damage.increaseHp = 30 * 100 / currentSpellPower;        
        skill.team = team; // 팀 구별 색인자
        skill.damage = damage; // 범위내 적에게 줄 데미지 정보, 틱데미지일경우 한틱당 데미지
    }
}
