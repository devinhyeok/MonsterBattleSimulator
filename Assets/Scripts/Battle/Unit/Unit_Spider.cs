using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit_Spider : Unit
{
    [Header("Spider")]
    public GameObject autoAttackObject;
    public GameObject skillObject;

    public override void Attack()
    {
        // 타겟 없으면 생략
        if (!target)
            return;

        // 데미지 설정
        Damage damage = new Damage();
        damage.normalDamage = currentAttackPower;
        damage.sourceGameObject = gameObject;
        damage.buffList.Add(new BuffData(BuffType.walkSpeedDown2, 1f));

        // 스킬 콜리전 생성
        float angle = Quaternion.FromToRotation(Vector3.right, direction).eulerAngles.z; // 회전 기본값 가져오기        
        GameObject tempAutoAttackObject = Instantiate(autoAttackObject, transform.position, Quaternion.Euler(0, 0, angle));
        Skill skill = tempAutoAttackObject.GetComponent<Skill>();
        SkillMovement_Target skillMovement = tempAutoAttackObject.GetComponent<SkillMovement_Target>();
        SpriteRenderer SkillSpriteRenderer = tempAutoAttackObject.transform.Find("Sprite").GetComponent<SpriteRenderer>();

        // 스프라이트 수정
        if (90 <= angle && angle < 270)
        {
            SkillSpriteRenderer.flipY = true;
        }

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
        Unit CloseEnemy = GetCloseEnemy();

        // 가까운 유닛없으면 생략
        if (!CloseEnemy)
            return;

        float distance = (CloseEnemy.transform.position - transform.position).magnitude;
        if (distance <= 2f)
        {
            base.UseSkill();
            // 타겟 변경
            target = CloseEnemy;

            // 데미지 설정
            Damage damage = new Damage();
            damage.magicDamage = 50 * currentSpellPower / 100;
            damage.buffList.Add(new BuffData(BuffType.root, 5f));
            damage.sourceGameObject = gameObject;

            // 스킬 생성
            float angle = Quaternion.FromToRotation(Vector3.right, direction).eulerAngles.z; // 회전 기본값 가져오기
            GameObject tempObject = Instantiate(skillObject, transform.position, Quaternion.Euler(0, 0, angle)); // 스킬 방향이 보는 방향이 일치해야하며, 내 위치에서 생성될 때

            // 스킬 정보 설정
            Skill skill = tempObject.GetComponent<Skill>();
            skill.team = team; // 팀 구별 색인자
            skill.damage = damage; // 범위내 적에게 줄 데미지 정보, 틱데미지일경우 한틱당 데미지

            ApplyPhysicalDamage(direction * -10f, new Damage()); // 반대 방향으로 이동
        }
        else
        {
            Attack();
        }
    }
}