﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Unit_Cobra : Unit
{
    [Header("Cobra")]
    public GameObject autoAttackObject;

    public override void Attack()
    {
        // 타겟이 없으면 생략
        if (!target)
            return;

        // 데미지 설정
        Damage damage = new Damage();
        float totalDamage = currentAttackPower * currentSpellPower / 100;
        damage.normalDamage = totalDamage / 2;
        damage.magicDamage = totalDamage / 2;
        damage.sourceGameObject = gameObject;

        // 스킬 콜리전 생성                
        GameObject tempAutoAttackObject = Instantiate(autoAttackObject, transform.position, Quaternion.identity);
        Skill skill = tempAutoAttackObject.GetComponent<Skill>();
        SkillMovement_Target skillMovement = tempAutoAttackObject.GetComponent<SkillMovement_Target>();        

        // 스킬 콜리전 설정
        skill.team = team; // 팀 구별 색인자
        skill.damage = damage; // 범위내 적에게 줄 데미지 정보, 틱데미지일경우 한틱당 데미지

        // 타겟 설정
        (skill as SkillProjectile_Once).target = target.gameObject;
        skillMovement.target = target.gameObject;

        // 스킬 활성화
        skillMovement.Play();
    }
}
