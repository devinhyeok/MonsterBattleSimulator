using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_Lizard : Unit
{
    [Header("Unit_Lizard")]
    public GameObject skillObject;

    public override IEnumerator PlaySkillAnim(float cooltime)
    {
        base.UseSkill();

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

        // 날라가기
        animator.SetBool("Rigid",true); // 혹시 나중에 날아가는 애니메이션 따로 제작 시 이부분 바꾸기
        isAction = true;
                      
        target = GetFarEnemy();
        Vector3 targetPosition = target.transform.position + (target.gameObject.transform.position - transform.position).normalized;

        float moveSpeed = 5f;
        while ((transform.position - targetPosition).magnitude > 0.1f / moveSpeed)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
            yield return new WaitForEndOfFrame();
        }
        transform.position = targetPosition;

        animator.SetBool("Rigid", false); // 혹시 나중에 날아가는 애니메이션 따로 제작 시 이부분 바꾸기
        isAction = false;
        Destroy(_skillObject);
    }
}
