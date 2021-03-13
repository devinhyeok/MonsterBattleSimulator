using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillProjectile_Once : Skill
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        damage.normalDamage = 100;
        if (collision.gameObject.GetComponent<Unit>() == null)
            return;
        if ((team == collision.gameObject.GetComponent<Unit>().team) == forEnemy)
            return;
        collision.gameObject.GetComponent<Unit>().ApplyDamage(damage);
        Destroy(gameObject);
    }
}
