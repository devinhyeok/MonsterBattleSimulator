using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillMovement_Target : SkillMovement
{
    public GameObject target;
    public float moveWaitSecond;
    public float destroyWaitSecond;
    public SpriteRenderer spriteRenderer;
    public bool updateFlipY;

    public override void Play()
    {
        if (target)
            StartCoroutine(MoveToTarget());                
    }

    private void Update()
    {
        if (target && spriteRenderer)
        {
            Vector2 direction = target.transform.position - transform.position;
            float angle = direction.GetAngle();
            spriteRenderer.gameObject.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, angle));

            // 위아래 있으면 뒤집어서 위쪽이 위를 보게하기
            if (updateFlipY)
            {
                if (90 <= angle && angle < 270)
                    spriteRenderer.flipY = true;
                else
                    spriteRenderer.flipY = false;
            }              
        }
        
    }

    IEnumerator MoveToTarget()
    {        
        yield return new WaitForSeconds(moveWaitSecond);
        while (target != null)
        {                        
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, Time.deltaTime * moveSpeed);
            if ((transform.position - target.transform.position).magnitude <= 0f)
                break;            
            yield return new WaitForEndOfFrame();
        }                    
        yield return new WaitForSeconds(destroyWaitSecond);
        Destroy(gameObject);    
    }
}