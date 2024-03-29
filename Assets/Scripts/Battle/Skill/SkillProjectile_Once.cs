﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillProjectile_Once : Skill
{
    public GameObject target;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Unit>() == null)
            return;
        if ((team == collision.gameObject.GetComponent<Unit>().team) != forFriend)
            return;
        if (target)
        {
            if (target != collision.gameObject)
                return;
        }
        collision.gameObject.GetComponent<Unit>().ApplyDamage(damage);
        Destroy(gameObject);
    }
}
