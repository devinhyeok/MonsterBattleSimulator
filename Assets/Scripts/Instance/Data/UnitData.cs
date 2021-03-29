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
}
