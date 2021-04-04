using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New UnitData", menuName = "Data/New UnitData")]
public class UnitData : ScriptableObject
{
    public List<UnitStatus> statusList = new List<UnitStatus>();
    [TextArea]
    public string englishTooltip;
    [TextArea]
    public string koreanTooltip;

    static public UnitData Get(string key)
    {
        UnitData unitData = Resources.Load<UnitData>(string.Format("Data/UnitData/{0}", key));

        if (unitData)
        {
            return unitData;
        }
        else
        {           
            Debug.LogWarning(string.Format("해당 이름의 유닛을 찾을 수 없습니다: {0}", key));
            return null;
        }
    }
}
