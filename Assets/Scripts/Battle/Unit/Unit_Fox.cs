using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_Fox : Unit
{
    public GameObject skillObject;

    public override void UseSkill()
    {
        base.UseSkill();

        // 데미지 설정
        Damage damage = new Damage();
        damage.normalDamage = 100 * currentAbilityPower / 100;
        damage.sourceGameObject = gameObject;

        // 스킬 생성
        float angle = Quaternion.FromToRotation(Vector3.right, direction).eulerAngles.z; // 회전 기본값 가져오기
        Vector3 tempVector = direction.Rotate(90) * 0.5f;
        GameObject spawnSkillObject1 = Instantiate(skillObject, transform.position - direction * 0.5f + tempVector, Quaternion.Euler(0, 0, angle)); // 스킬 방향이 보는 방향이 일치해야하며, 내 위치에서 생성될 때
        GameObject spawnSkillObject2 = Instantiate(skillObject, transform.position - direction * 0.5f, Quaternion.Euler(0, 0, angle)); // 스킬 방향이 보는 방향이 일치해야하며, 내 위치에서 생성될 때        
        GameObject spawnSkillObject3 = Instantiate(skillObject, transform.position - direction * 0.5f - tempVector, Quaternion.Euler(0, 0, angle)); // 스킬 방향이 보는 방향이 일치해야하며, 내 위치에서 생성될 때
        List<GameObject> spawnSkillObjects = new List<GameObject>();
        spawnSkillObjects.Add(spawnSkillObject1);
        spawnSkillObjects.Add(spawnSkillObject2);
        spawnSkillObjects.Add(spawnSkillObject3);

        // 스킬 정보 설정
        int i = 0;
        foreach(GameObject spawnSkillObject in spawnSkillObjects)
        {
            Skill skill = spawnSkillObject.GetComponent<Skill>();
            skill.team = team; // 팀 구별 색인자
            skill.damage = damage; // 범위내 적에게 줄 데미지 정보, 틱데미지일경우 한틱당 데미지
            (skill as SkillProjectile_Once).target = target.gameObject;

            SkillMovement_Target skillMovement = spawnSkillObject.GetComponent<SkillMovement_Target>();
            skillMovement.target = target.gameObject;
            skillMovement.moveWaitSecond = i * 0.5f;
            skillMovement.Play();            

            i++;
        }
    }
}
