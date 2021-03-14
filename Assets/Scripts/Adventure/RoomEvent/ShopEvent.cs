using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopEvent : RoomEvent
{
    public bool isFixed;
    public List<ItemData> itemDataRandomPool;
    public ItemData[] itemDataFixed = new ItemData[3];

    public override void EnterRoom()
    {
        Debug.Log(string.Format("{0}방 입장", gameObject));
    }

    private void Awake()
    {
        if (isFixed)
        {
            int index = 0;
            foreach (Transform child in transform)
            {
                if (child.gameObject.tag == "DisplayGoods")
                {
                    DisplayGoods displayGoods = child.gameObject.GetComponent<DisplayGoods>();
                    displayGoods.ItemData = itemDataFixed[index];
                    index++;
                }
            }
        }
        else
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject.tag == "DisplayGoods")
                {
                    DisplayGoods displayGoods = child.gameObject.GetComponent<DisplayGoods>();
                    displayGoods.ItemData = itemDataRandomPool[Random.Range(0, itemDataRandomPool.Count)];
                }
            }            
        }
    }
}
