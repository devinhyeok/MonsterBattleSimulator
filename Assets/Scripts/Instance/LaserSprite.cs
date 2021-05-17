using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserSprite : MonoBehaviour
{
    public GameObject laserStart;
    public GameObject laserMiddle;
    public GameObject laserEnd;
    public float maxLaserSize;
    public float minLaserSize;
    public Transform hitPoint;

    [Header("읽기용")]
    public float currentLaserSize;

    private GameObject start;
    private GameObject middle;
    private GameObject end;
    private float startSpriteWidth;
    private float middleSpriteWidth;
    private float endSpriteWidth;

    void Update()
    {        
        // 레이저가 없으면 생성
        if (start == null)
        {
            start = Instantiate(laserStart);
            start.transform.parent = transform;
            start.transform.localPosition = Vector2.zero;
            startSpriteWidth = start.GetComponent<SpriteRenderer>().bounds.size.x;
        }
        if (middle == null)
        {
            middle = Instantiate(laserMiddle);
            middle.transform.parent = transform;
            middle.transform.localPosition = Vector2.zero;
            middleSpriteWidth = middle.GetComponent<SpriteRenderer>().bounds.size.x;
        }
        if (end == null)
        {
            end = Instantiate(laserEnd);
            end.transform.parent = transform;
            end.transform.localPosition = Vector2.zero;
            endSpriteWidth = end.GetComponent<SpriteRenderer>().bounds.size.x;
        }

        // 레이저 방향 길이 조절
        if (hitPoint)
        {
            currentLaserSize = Vector2.Distance(hitPoint.position, transform.position);
            Vector2 direction = (hitPoint.position - transform.position);
            transform.rotation = Quaternion.Euler(0f, 0f, direction.GetAngle());
            end.gameObject.SetActive(true);
        }
        else
        {
            currentLaserSize = maxLaserSize;
            end.gameObject.SetActive(false);
        }

        // 최소 길이 이하이면 비활성화
        if (currentLaserSize <= minLaserSize)
        {
            start.SetActive(false);
            middle.SetActive(false);
            end.SetActive(false);
        }
        else
        {
            start.SetActive(true);
            middle.SetActive(true);
            end.SetActive(true);
        }

        // 시작 스프라이트 트렌스폼 조절
        if (start != null)
        {
            start.transform.rotation = transform.rotation;
        }
        // 중간 스프라이트 트렌스폼 조절
        if (middle != null)
        {
            float middleSize = (currentLaserSize - endSpriteWidth) / middleSpriteWidth;
            middle.transform.localScale = new Vector3(middleSize, middle.transform.localScale.y, middle.transform.localScale.z);
            middle.transform.localPosition = new Vector2((currentLaserSize - startSpriteWidth / 2) / 2, 0f);
            middle.transform.rotation = transform.rotation;
        }
        // 끝점 스프라이트 트렌스폼 조절
        if (end != null)
        {
            end.transform.localPosition = new Vector2(currentLaserSize - endSpriteWidth / 2, 0f);
            end.transform.rotation = transform.rotation;
        }
            
    }
}