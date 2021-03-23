using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillCollider : Skill
{
    public Shape shape;
    public float angle;
    //[HideInInspector]
    public List<GameObject> overlapObjectList;
    Vector2 forwardVector;

    public virtual void Update()
    {
        if (shape == Shape.sector)
        {
            forwardVector = (Vector2)(transform.TransformDirection(Vector3.right));
            float radius = GetComponent<CircleCollider2D>().radius;
            Debug.DrawRay(transform.position, (forwardVector * radius).Rotate(angle), Color.green);
            Debug.DrawRay(transform.position, (forwardVector * radius).Rotate(-angle), Color.green);
        }        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(shape == Shape.sector)
        {
            float tempRadius = collision.gameObject.GetComponent<CircleCollider2D>().radius;
            Vector3 tempVector1 = collision.transform.position - transform.position + (Vector3)(forwardVector.Rotate(90f) * tempRadius);
            Vector3 tempVector2 = collision.transform.position - transform.position - (Vector3)(forwardVector.Rotate(90f) * tempRadius);
            float angle1 = Vector2.Angle(tempVector1, forwardVector);
            float angle2 = Vector2.Angle(tempVector2, forwardVector);
            float closeAngle = (angle1 < angle2) ? angle1 : angle2;
            if (closeAngle > angle)
            {
                overlapObjectList.Remove(collision.gameObject);
                return;
            }  
        }
        overlapObjectList.Add(collision.gameObject);
        overlapObjectList = overlapObjectList.Distinct().ToList(); // 중복 제거
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        overlapObjectList.Remove(collision.gameObject);
    }
}
