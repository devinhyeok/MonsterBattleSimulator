﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillMovement_Target : SkillMovement
{
    public GameObject target;
    public float moveWaitSecond;
    public float destroyWaitSecond;

    public override void Play()
    {
        if (target)
        {
            StartCoroutine(MoveToTarget());
        }        
    }

    IEnumerator MoveToTarget()
    {        
        yield return new WaitForSeconds(moveWaitSecond);        
        while ((transform.position - target.transform.position).magnitude > 0f && target)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, Time.deltaTime * moveSpeed);
            yield return new WaitForEndOfFrame();
        }                    
        yield return new WaitForSeconds(destroyWaitSecond);        
        Destroy(gameObject);
    }
}