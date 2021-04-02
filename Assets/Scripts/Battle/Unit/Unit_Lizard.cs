using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_Lizard : Unit
{
    [Header("Unit_Lizard")]
    public GameObject skillObject;
    public float moveSpeed;

    public override IEnumerator PlaySkillAnim(float cooltime)
    {
        base.UseSkill();

        // 타겟이 없으면 실행 중지
        target = GetFarEnemy();
        if (!target)
            yield break;

        // 스킬 데미지 설정
        Damage damage = new Damage();
        damage.sourceGameObject = gameObject;
        damage.normalDamage = 250 * currentSpellPower / 100;

        // 스킬 생성
        GameObject _skillObject = Instantiate(skillObject, transform); // 스킬 방향이 보는 방향이 일치해야하며, 내 위치에서 생성될 때

        // 스킬 정보 설정
        Skill skill = _skillObject.GetComponent<Skill>();
        skill.team = team;
        skill.damage = damage;

        // 충돌을 이동 스킬 상태 설정하기
        gameObject.layer = LayerMask.NameToLayer("UsingMovementSkill");
        animator.SetBool("Rigid",true); // 혹시 나중에 날아가는 애니메이션 따로 제작 시 이부분 바꾸기
        isAction = true;
        
        // 타겟을 향해 날라가기        
        direction = (target.transform.position - transform.position).normalized;
        Vector3 startPosition = transform.position;
        Vector3 goalPosition = target.transform.position + (target.transform.position - transform.position).normalized;
        float distance = (startPosition - goalPosition).magnitude;
        goalPosition += (target.transform.position - transform.position).normalized * 0.1f;

        while ((transform.position - startPosition).magnitude < distance)
        {
            transform.position = Vector3.Lerp(transform.position, goalPosition, Time.deltaTime * moveSpeed);
            yield return new WaitForEndOfFrame();
        }
        rigidbody.velocity = Vector2.zero;

        animator.SetBool("Rigid", false); // 혹시 나중에 날아가는 애니메이션 따로 제작 시 이부분 바꾸기
        isAction = false;
        Destroy(_skillObject);

        // 충돌을 이동 스킬 상태로 설정
        gameObject.layer = LayerMask.NameToLayer("BattleUnit");
    }
}
