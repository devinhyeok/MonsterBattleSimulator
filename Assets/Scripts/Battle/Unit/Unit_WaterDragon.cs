using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit_WaterDragon : Unit
{
    [Header("WaterDragon")]    
    public GameObject skillObject;
    public Transform muzzle;
    public Transform hitPoint;

    GameObject beforeTarget;
    float attackStack = 0;
    
    public override void Awake()
    {
        base.Awake();
        skillObject = Instantiate(skillObject, transform);
        skillObject.transform.localPosition = muzzle.localPosition;
    }

    public override void Update()
    {
        base.Update();
        if (isRigid || aiState != AIState.attack)
            animator.SetBool("Attack", false);    

        // 공격 포인트 설정
        if (target)
        {
            hitPoint.position = target.transform.position - (target.transform.position - transform.position).normalized * target.GetComponent<CircleCollider2D>().radius;
            skillObject.GetComponent<LaserSprite>().hitPoint = hitPoint;
        }
        else
        {
            skillObject.GetComponent<LaserSprite>().hitPoint = null;
        }

        // 보는 방향에 따라 총구 위치 조절
        int flipX;
        float positionX = Mathf.Abs(muzzle.localPosition.x);
        flipX = (spriteRenderer.flipX) ? 1 : -1;
        muzzle.localPosition = new Vector3(flipX * positionX, muzzle.localPosition.y);
        skillObject.transform.localPosition = muzzle.localPosition;

        if (animator.GetBool("Attack") && target && skillObject.GetComponent<LaserSprite>().hitPoint != null)
            skillObject.SetActive(true);
        else
            skillObject.SetActive(false);        
    }

    public override IEnumerator PlayAttackAnim(float cooltime)
    {
        // 타겟이 있으면 모션 재생
        if (target)
        {
            direction = (target.transform.position - transform.position).normalized;
            beforeTarget = target.gameObject;
        }            
        isAction = true;
        
        // 대상에게 누적딜 주기
        while (target && aiState == AIState.attack && !isRigid)
        {            
            if (!beforeTarget)            
                break;
            if (target.gameObject != beforeTarget)
                break;

            // 공격 모션 출력
            animator.SetBool("Attack", true);

            // 데미지 주기
            Damage damage = new Damage();
            damage.sourceGameObject = gameObject;
            damage.normalDamage = currentAttackPower * (1 + attackStack);
            damage.onHit = true;
            target.ApplyDamage(damage);
            
            // 스텍 증가
            attackStack += 0.2f;

            yield return new WaitForSeconds(cooltime);
        }
        attackStack = 0;
        animator.SetBool("Attack", false);        
        isAction = false;        
    }
}
