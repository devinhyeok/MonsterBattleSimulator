using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New ItemData", menuName = "Data/New ItemData")]
public class ItemData : ScriptableObject
{
    // 아이템 데이터
    public string key;
    public string englishName;
    public string koreanName;
    public Sprite thumbnail;
    public int cost;
    public Filter filter;
    public StackType stackType;
    public int maxStack;    
    public int sellGold;
    public int buyGold;
    

    // 로컬라이징 데이터
    [TextArea]
    public string englishTooltip;    
    [TextArea]
    public string koreanTooltip;

    // 아이템 사용 효과
    public GameObject spawnObject;

    static public ItemData Get(string _key)
    {
        ItemData itemData = null;
        if (Resources.Load<ItemData>(string.Format("Data/ItemData/UnitItemData/{0}", _key)))
            itemData = Resources.Load<ItemData>(string.Format("Data/ItemData/UnitItemData/{0}", _key));
        else if (Resources.Load<ItemData>(string.Format("Data/ItemData/EquipItemData/{0}", _key)))
            itemData = Resources.Load<ItemData>(string.Format("Data/ItemData/EquipItemData/{0}", _key));
        else if (Resources.Load<ItemData>(string.Format("Data/ItemData/BattleItemData/{0}", _key)))
            itemData = Resources.Load<ItemData>(string.Format("Data/ItemData/BattleItemData/{0}", _key));

        if (itemData)
        {
            return itemData;
        }
        else
        {
            Debug.LogWarning(string.Format("해당 이름의 아이템을 찾을 수 없습니다: {0}", _key));
            return null;            
        }
    }
}
