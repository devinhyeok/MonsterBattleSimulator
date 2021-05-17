using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit_FireWyvern : Unit
{
    [Header("FireWyvern")]
    public GameObject autoAttackObject;
    public GameObject skillObject;

    public override void Attack()
    {
        // 타겟이 없으면 생략
        if (!target)
            return;

        // 데미지 설정
        Damage damage = new Damage();
        damage.normalDamage = currentAttackPower;
        damage.sourceGameObject = gameObject;

        // 스킬 콜리전 생성             
        GameObject tempAutoAttackObject = Instantiate(autoAttackObject, transform.position, Quaternion.identity);
        Skill skill = tempAutoAttackObject.GetComponent<Skill>();
        SkillMovement_Target skillMovement = tempAutoAttackObject.GetComponent<SkillMovement_Target>();        

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
        // 타겟이 없으면 생략
        if (!target)
            return;

        base.UseSkill();

        // 스킬 생성
        float angle = Quaternion.FromToRotation(Vector3.right, direction).eulerAngles.z; // 회전 기본값 가져오기
        GameObject tempObject = Instantiate(skillObject, transform.position, Quaternion.Euler(0, 0, angle)); // 스킬 방향이 보는 방향이 일치해야하며, 내 위치에서 생성될 때

        // 스킬 정보 설정
        Skill skill = tempObject.GetComponent<Skill>();
        skill.team = team; // 팀 구별 색인자
        Damage tempDamage = new Damage();
        tempDamage.sourceGameObject = gameObject;
        skill.damage = tempDamage;

        // 스킬 이동 실행(이동을 할 경우)
        if (tempObject.GetComponent<SkillMovement_Path>() != null)
        {
            SkillMovement_Path skillMovement = tempObject.GetComponent<SkillMovement_Path>();
            float x = (target.transform.position - transform.position).magnitude;
            List<Vector2> pathVectors = new List<Vector2>();
            pathVectors.Add(new Vector2(x, 0));
            skillMovement.VectorList = pathVectors;
            Damage damage = new Damage();
            damage.sourceGameObject = gameObject;
            damage.magicDamage = 200 * currentSpellPower / 100;
            skillMovement.endSpawnSkillDamage = damage;
            skillMovement.Play();
        }
    }
}
