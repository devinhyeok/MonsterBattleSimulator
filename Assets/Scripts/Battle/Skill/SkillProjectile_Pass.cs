using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillProjectile_Pass : Skill
{
    List<GameObject> damagedObjectList;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 유닛이 아니면 제외
        if (collision.gameObject.GetComponent<Unit>() == null)
            return;
        // 적절한 대상이 아니면 제외
        if ((team == collision.gameObject.GetComponent<Unit>().team) != forFriend)
            return;
        // 이미 데미지를 주었던 대상이면 제외
        foreach(GameObject damagedObject in damagedObjectList)
        {
            if (damagedObject == collision.gameObject)
                return;
        }
        collision.gameObject.GetComponent<Unit>().ApplyDamage(damage);
        damagedObjectList.Add(collision.gameObject);
    }
}
