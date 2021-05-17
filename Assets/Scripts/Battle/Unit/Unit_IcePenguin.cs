using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit_IcePenguin : Unit
{
    [Header("Unit_IcePenguin")]
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
        damage.magicDamage = 200 * currentSpellPower / 100;
        damage.buffList.Add(new BuffData(BuffType.ice, 2f * currentSpellPower / 100));

        // 스킬 생성
        GameObject _skillObject = Instantiate(skillObject, transform); // 스킬 방향이 보는 방향이 일치해야하며, 내 위치에서 생성될 때

        // 스킬 정보 설정
        Skill skill = _skillObject.GetComponent<Skill>();
        skill.team = team;
        skill.damage = damage;

        // 충돌을 이동 스킬 상태 설정하기
        aiState = AIState.none;
        gameObject.layer = LayerMask.NameToLayer("UsingMovementSkill");
        animator.SetBool("UseSkill", true); // 혹시 나중에 날아가는 애니메이션 따로 제작 시 이부분 바꾸기
        isAction = true;

        // 타겟을 향해 날라가기
        Vector3 targetPosition = target.transform.position;
        direction = (targetPosition - transform.position).normalized;
        Vector3 startPosition = transform.position;
        Vector3 goalPosition = targetPosition + (targetPosition - transform.position).normalized;
        float distance = (startPosition - goalPosition).magnitude;
        goalPosition += (targetPosition - transform.position).normalized * 0.1f;

        while ((transform.position - startPosition).magnitude < distance)
        {
            Debug.Log(goalPosition);
            transform.position = Vector3.MoveTowards(transform.position, goalPosition, moveSpeed / 100 * Time.timeScale);
            yield return new WaitForEndOfFrame();
        }
        rigidbody.velocity = Vector2.zero;

        animator.SetBool("UseSkill", false); // 혹시 나중에 날아가는 애니메이션 따로 제작 시 이부분 바꾸기
        isAction = false;
        Destroy(_skillObject);

        // 충돌을 이동 스킬 상태로 설정        
        gameObject.layer = LayerMask.NameToLayer("BattleUnit");
        aiState = AIState.idle;
    }
}
