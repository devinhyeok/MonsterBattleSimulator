using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New ItemData", menuName = "Data/New ItemData")]
public class ItemData : ScriptableObject
{
    // 아이템 데이터
    public string key;
    public Sprite thumbnail;
    public Filter filter;
    public StackType stackType;
    public int maxStack;
    public int sellGold;
    public int sellGem;
    public int buyGold;
    public int buyGem;    

    // 로컬라이징 데이터
    public string englishName;
    public string koreanName;
    [TextArea]
    public string englishTooltip;    
    [TextArea]
    public string koreanTooltip;

    // 아이템 사용 효과
    public GameObject spawnObject;

    public static ItemData GetData(string key)
    {
        ItemData itemData = null;
        if (Resources.Load<ItemData>(string.Format("Data/ItemData/UnitItemData/{0}", key)))
            itemData = Resources.Load<ItemData>(string.Format("Data/ItemData/UnitItemData/{0}", key));
        else if (Resources.Load<ItemData>(string.Format("Data/ItemData/EquipItemData/{0}", key)))
            itemData = Resources.Load<ItemData>(string.Format("Data/ItemData/EquipItemData/{0}", key));
        else if (Resources.Load<ItemData>(string.Format("Data/ItemData/BattleItemData/{0}", key)))
            itemData = Resources.Load<ItemData>(string.Format("Data/ItemData/BattleItemData/{0}", key));
        else
            Debug.LogError(string.Format("{0}키를 가진 아이템 데이터를 찾을 수 없습니다.", key));

        return itemData;
    }
}
