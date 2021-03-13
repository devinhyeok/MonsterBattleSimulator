using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillCollider_Once : SkillCollider
{
    public override void Play()
    {
        StartCoroutine(ApplyDamageAll());
    }

    // 범위내 모든 대상에게 데미지 주고 사라지기
    IEnumerator ApplyDamageAll()
    {
        yield return new WaitForSeconds(0.1f); // 간혹 딜 씹히는 경우가 있어 0.1초 기달려줌
        foreach (GameObject tempGameObject in overlapObjectList)
        {
            // 유닛이 아닌 경우 제외
            if (tempGameObject.GetComponent<Unit>() == null)
                break;
            // 알맞은 대상이 아닌 경우 제외
            if ((tempGameObject.GetComponent<Unit>().team == team) == forEnemy)
                break;
            
            tempGameObject.GetComponent<Unit>().ApplyDamage(damage);
        }
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
