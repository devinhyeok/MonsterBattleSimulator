using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New UnitData",menuName = "Data/New UnitData")]
public class UnitData : ScriptableObject
{
    public string key;
    public float hp;
    public float mp;
    public float attack;
    public float defense;
    public float attackSpeed;
    public float manaRegen;
    public float healthRegen;
    public float abilityPower;
    public float walkSpeed;
    public float attackDistance;

    public static UnitData GetData(string key)
    {
        UnitData unitData = Resources.Load<UnitData>(string.Format("Data/Unit/{0}", key));

        if (unitData == null)
            Debug.LogError(string.Format("{0}키를 가진 유닛 데이터를 찾을 수 없습니다.", key));

        return unitData;
    }
}
