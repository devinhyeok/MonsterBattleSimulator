using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillCollider_Dot : SkillCollider
{
    [Header("SkillCollider_Dot")]
    public float damageWaitSecond;
    public float destroyWaitSecond;
    public float dotSecond;

    private void Start()
    {
        StartCoroutine(TickApplyDamageAll(dotSecond));
    }

    // 범위내 모든 대상에게 데미지 주고 사라지기
    IEnumerator TickApplyDamageAll(float dotSecond)
    {
        //Debug.Log("시작");
        yield return new WaitForSeconds(damageWaitSecond + 0.05f); // waitSecond만큼 기달리고 실행

        while (dotSecond > 0)
        {            
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
            yield return new WaitForSeconds(1f);
            dotSecond -= 1;
            
        }
        yield return new WaitForSeconds(destroyWaitSecond + 0.05f);
        Destroy(gameObject);
    }
}
