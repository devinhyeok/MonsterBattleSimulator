using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New ShopData", menuName = "Data/New ShopData")]
public class ShopData : ScriptableObject
{
    public List<ItemData> SellUnitItemPool; // 유닛 아이템 풀
    public List<ItemData> SellEquipItemPool; // 장착 아이템 풀
    public List<ItemData> SellBattleItemPool; // 전투 아이템 풀
}