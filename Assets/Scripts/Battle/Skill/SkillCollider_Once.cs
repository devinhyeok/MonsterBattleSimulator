using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillCollider_Once : SkillCollider
{
    [Header("SkillCollider_Once")]
    public float damageWaitSecond;
    public float destroyWaitSecond;

    private void Start()
    {
        StartCoroutine(ApplyDamageAll());
    }

    // 범위내 모든 대상에게 데미지 주고 사라지기
    IEnumerator ApplyDamageAll()
    {
        yield return new WaitForSeconds(damageWaitSecond + 0.025f); // waitSecond만큼 기달리고 실행
        foreach (GameObject tempGameObject in overlapObjectList)
        {
            // 유닛이 아닌 경우 제외
            if (tempGameObject.GetComponent<Unit>() == null)
                continue;
            // 알맞은 대상이 아닌 경우 제외
            if ((tempGameObject.GetComponent<Unit>().team == team) != forFriend)
                continue;
            
            tempGameObject.GetComponent<Unit>().ApplyDamage(damage);
        }
        yield return new WaitForSeconds(destroyWaitSecond + 0.025f);        
        Destroy(gameObject);
    }
}
