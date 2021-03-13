using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 저장할 정보
[System.Serializable]
public class VillagePlayerCharaterData 
{
    
}

public class VillagePlayerCharater : MonoBehaviour
{
    //[Header("참조")]

    [Header("편집")]
    public VillagePlayerCharaterData data;
    public float moveSpeed;

    [Header("읽기")]
    public Vector2 axis;
    public Vector2 direction = new Vector2(0, -1);
    public GameObject interactGameObject;        

    // 참조
    new Rigidbody2D rigidbody2D;
    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // 키 값 받아오기
        float axisX = Input.GetAxisRaw("Horizontal");
        float axisY = Input.GetAxisRaw("Vertical");
        axis = new Vector2(axisX, axisY).normalized;
    }

    private void FixedUpdate()
    {
        // 이동
        rigidbody2D.velocity = axis * moveSpeed;
        if (axis.magnitude > 0)
        {
            direction = axis;
            if (Mathf.Abs(axis.x) > 0)
                spriteRenderer.flipX = 0 < axis.x ? true : false;
        }
            

        // 상호작용 검사
        Debug.DrawRay(rigidbody2D.position, direction, new Color(0, 1, 0));
        RaycastHit2D raycastHit = Physics2D.Raycast(rigidbody2D.position, direction, 1f, LayerMask.GetMask("Charater"));

        if (raycastHit.collider != null)
        {
            interactGameObject = raycastHit.collider.gameObject;
        }
        else
        {
            interactGameObject = null;
        }
    }
}
