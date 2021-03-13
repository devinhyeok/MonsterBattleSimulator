using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New RewardData", menuName = "Data/New RewardData")]
public class RewardData : ScriptableObject
{
    public List<ItemData> itemDataList; // 보상 받을 아이템
    public int increaseHp; // 회득할 HP
    public int increaseGold; // 회득할 Gold
}
