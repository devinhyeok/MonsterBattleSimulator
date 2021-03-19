using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillCollider_Dot : SkillCollider
{
    public float waitSecond;
    public float dotSecond;

    private void Start()
    {
        StartCoroutine(TickApplyDamageAll(dotSecond));
    }

    // 범위내 모든 대상에게 데미지 주고 사라지기
    IEnumerator TickApplyDamageAll(float dotSecond)
    {
        yield return new WaitForSeconds(waitSecond); // waitSecond만큼 기달리고 실행

        while (dotSecond > 0)
        {
            foreach (GameObject tempGameObject in overlapObjectList)
            {                
                // 유닛이 아닌 경우 제외
                if (tempGameObject.GetComponent<Unit>() == null)
                    break;
                // 알맞은 대상이 아닌 경우 제외
                if ((tempGameObject.GetComponent<Unit>().team == team) != forFriend)
                    break;
                tempGameObject.GetComponent<Unit>().ApplyDamage(damage);
                
            }
            yield return new WaitForSeconds(1f);
            dotSecond -= 1;
            
        }
        Destroy(gameObject);
    }
}
