using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardEvent : RoomEvent
{
    public bool isRandom;
    public List<ItemData> itemDatas = new List<ItemData>();    
    public List<ItemData> itemDataRandomPool = new List<ItemData>();    

    public override void EnterRoom()
    {
        
    }

    private void Awake()
    {
        if (isRandom)
        {
            if (itemDatas.Count <= 0)
                return;
            for (int i = 0; i < itemDatas.Count; i++)
            {
                itemDatas[i] = itemDataRandomPool[Random.Range(0, itemDatas.Count)];
            }
        }
    }
}
