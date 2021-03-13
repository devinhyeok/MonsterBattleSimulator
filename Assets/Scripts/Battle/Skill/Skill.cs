using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill : MonoBehaviour
{
    public int team;
    public bool forEnemy = false;
    public Damage damage = new Damage();

    public virtual void Play()
    {

    }
}
