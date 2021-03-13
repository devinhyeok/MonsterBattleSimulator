using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New ItemData", menuName = "Data/New ItemData")]
public class ItemData : ScriptableObject
{
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
    public string englishTooltip;
    public string koreanName;
    public string koreanTooltip;

    public static ItemData GetData(string key)
    {
        ItemData itemData = null;
        if (Resources.Load<ItemData>(string.Format("Data/Item/Unit/{0}", key)))
            itemData = Resources.Load<ItemData>(string.Format("Data/Item/Unit/{0}", key));
        else if (Resources.Load<ItemData>(string.Format("Data/Item/Equip/{0}", key)))
            itemData = Resources.Load<ItemData>(string.Format("Data/Item/Equip/{0}", key));
        else if (Resources.Load<ItemData>(string.Format("Data/Item/Battle/{0}", key)))
            itemData = Resources.Load<ItemData>(string.Format("Data/Item/Battle/{0}", key));
        else
            Debug.LogError(string.Format("{0}키를 가진 아이템 데이터를 찾을 수 없습니다.", key));

        return itemData;
    }
}
