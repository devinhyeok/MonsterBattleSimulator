using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_VolcanoTurtle : Unit
{
    [Header("Unit_VolcanoTurtle")]
    public GameObject skillObject;

    public override void UseSkill()
    {
        if (target)
        {
            base.UseSkill();
            // 데미지 설정
            Damage damage = new Damage();
            damage.normalDamage = 100 * currentSpellPower / 100;
            damage.sourceGameObject = gameObject;           

            // 스킬 생성
            float angle = Quaternion.FromToRotation(Vector3.right, direction).eulerAngles.z; // 회전 기본값 가져오기
            GameObject tempObject = Instantiate(skillObject, transform.position, Quaternion.Euler(0, 0, angle)); // 스킬 방향이 보는 방향이 일치해야하며, 내 위치에서 생성될 때

            // 스킬 정보 설정
            Skill skill = tempObject.GetComponent<Skill>();
            skill.team = team; // 팀 구별 색인자
            skill.damage = damage; // 범위내 적에게 줄 데미지 정보, 틱데미지일경우 한틱당 데미지

            // 스킬 이동 실행(이동을 할 경우)
            if (tempObject.GetComponent<SkillMovement_Path>() != null)
            {
                SkillMovement_Path skillMovement = tempObject.GetComponent<SkillMovement_Path>();
                float x = (target.transform.position - transform.position).magnitude;
                List<Vector2> pathVectors = new List<Vector2>();
                pathVectors.Add(new Vector2(x, 0));
                skillMovement.VectorList = pathVectors;
                skillMovement.endSpawnSkillDamage = damage;
                skillMovement.Play();
            }
        }        
    }
}
