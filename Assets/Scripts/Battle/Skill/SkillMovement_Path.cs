using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillMovement_Path : SkillMovement
{        
    public List<Vector2> vectorList; // 이동 경로 설정
    public List<Vector2> VectorList
    {
        get
        {
            return vectorList;
        }
        set
        {
            vectorList = value;
            vectorList.Reverse();
            vectorStack = new Stack<Vector2>(vectorList);
        }
    }
    public Stack<Vector2> vectorStack;
    public GameObject endSpawnSkillObject;
    public Damage endSpawnSkillDamage;

    private void Awake()
    {
        VectorList = vectorList;
    }

    public override void Play()
    {
        if (vectorStack.Count > 0)
        {
            Vector2 vector = vectorStack.Pop();
            StartCoroutine(PathMove(vector));
        }
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
            if (endSpawnSkillObject)
            {                
                if (GetComponent<Skill>().damage.sourceGameObject)
                {
                    endSpawnSkillDamage.sourceGameObject = GetComponent<Skill>().damage.sourceGameObject;
                    Instantiate(endSpawnSkillObject, transform.position, Quaternion.identity);
                    endSpawnSkillObject.GetComponent<Skill>().team = GetComponent<Skill>().team;
                    endSpawnSkillObject.GetComponent<Skill>().damage = endSpawnSkillDamage;
                }                                                
            }                
            Destroy(gameObject);
        }
    }
}
