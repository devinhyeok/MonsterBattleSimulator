using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit_Larva : Unit
{
    public GameObject SpawnObject;

    public override void UseSkill()
    {
        base.UseSkill();
        Unit spawnUnit = Instantiate(SpawnObject, transform.position - direction, Quaternion.identity).GetComponent<Unit>();
        spawnUnit.team = team;
        spawnUnit.aiState = AIState.idle;
        spawnUnit.Level = Level;
    }
}
