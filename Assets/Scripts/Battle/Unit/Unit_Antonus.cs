using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit_Antonus : Unit
{
    [Header("비상속값")]
    public GameObject skillObject;

    public override void Attack()
    {
        base.Attack();
    }
    public override void UseSkill()
    {
        base.UseSkill(); // 
        
        Damage damage = new Damage(); // 전달할 데미지 정보 생성
        damage.sourceGameObject = gameObject; // 공격 가해자 정보
        damage.onHit = false; // 데미지 온힛 여부
        damage.normalDamage = 0; // 데미지 일반데미지
        damage.trueDamage = 100; // 데미지 트루데미지
        damage.increaseHp = 0; // 대상이 회복할 체력량
        damage.increaseMp = 0; // 대상이 회복할 마나량

        damage.buffList.Add(new BuffData(BuffType.stun, 2f)); // 적용할 버프 추가        
        target.ApplyDamage(damage); // 적에게 데미지 전달
        /*
        damage = new Damage(); // 전달할 데미지 정보 초기화
        ApplyDamage(damage); // 자신에게 데미지를 주고 싶으면 타겟을 빼면됨
        */

        // 데미지 정보 정하기
        //Damage damage = new Damage(); // 전달할 데미지 정보 생성
        //damage.sourceGameObject = gameObject; // 공격 가해자 정보
        //damage.onHit = false; // 데미지 온힛 여부
        //damage.normalDamage = 0; // 데미지 일반데미지
        //damage.trueDamage = 100; // 데미지 트루데미지
        //damage.increaseHp = 0; // 대상이 회복할 체력량
        //damage.increaseMp = 0; // 대상이 회복할 마나량
        //damage.buffSecondDictionary.Add(BuffType.stun, 3f); // 적용할 버프 추가
        //damage.buffSecondDictionary.Add(BuffType.silence, 3f); // 적용할 버프 추가

        // 스킬 위치 방향 정하기 (4개 중 하나 선택해 입력)
        //float angle = Quaternion.FromToRotation(Vector3.right, direction).eulerAngles.z; // 회전 기본값 가져오기
        //GameObject tempObject = Instantiate(skillObject, transform.position, Quaternion.Euler(0, 0, angle)); // 스킬 방향이 보는 방향이 일치해야하며, 내 위치에서 생성될 때

        //GameObject tempObject = Instantiate(skillObject, transform.position, Quaternion.identity); // 스킬 방향이 보는 방향이 일치하지 않으며, 내 위치에서 생성될 때

        //float angle = Quaternion.FromToRotation(Vector3.right, direction).eulerAngles.z; // 회전 기본값 가져오기
        //GameObject tempObject = Instantiate(skillObject, target.transform.position, Quaternion.Euler(0, 0, angle); // 스킬 방향이 보는 방향이 일치해야하며, 타겟 위치에서 생성될 때

        //GameObject tempObject = Instantiate(skillObject, target.transform.position, Quaternion.identity); // 스킬 방향이 보는 방향이 일치하지 않으며, 타겟 위치에서 생성될 때

        // 스킬 정보 설정
        //Skill skill = tempObject.GetComponent<Skill>();
        //skill.team = team; // 팀 구별 색인자
        //skill.damage = damage; // 범위내 적에게 줄 데미지 정보, 틱데미지일경우 한틱당 데미지
        //skill.Play(); // 스킬 콜리전 검사 시작

        //// 스킬 이동 실행(이동을 할 경우)
        //if(tempObject.GetComponent<SkillMovement>()!=null)
        //{
        //    SkillMovement skillMovement = tempObject.GetComponent<SkillMovement>();
        //    skillMovement.Play();
        //}        
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
