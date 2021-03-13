using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillMovement : MonoBehaviour
{
    public float moveSpeed = 1f;
    public List<Vector2> vectorList; // 이동 경로 설정
    public Stack<Vector2> vectorStack;    

    private void Awake()
    {
        vectorList.Reverse();
        vectorStack = new Stack<Vector2>(vectorList);
    }

    public virtual void Play()
    {
        Vector2 vector = vectorStack.Pop();
        StartCoroutine(PathMove(vector));
    }

    // 경로 이동 시작
    IEnumerator PathMove(Vector2 forcedVelocity)
    {
        Vector3 targetPosition = transform.position + (Vector3)forcedVelocity.Rotate(transform.eulerAngles.z);

        while ((transform.position - targetPosition).magnitude > 0f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * moveSpeed);
            yield return new WaitForEndOfFrame();
        }
        transform.position = targetPosition;

        if (vectorStack.Count > 0)
        {
            Vector2 vector = vectorStack.Pop();
            StartCoroutine(PathMove(vector));
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
