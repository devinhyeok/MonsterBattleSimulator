using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemSlotData
{
    // 변수
    public int index; // 슬롯 인덱스
    public ItemData itemData; // 슬롯 아이템 데이터
    public ItemSlotUI itemSlotUI; // 슬롯 UI      
    public int deltaCost;
    public SlotType fromSlotType;

    // 프로퍼티
    [SerializeField]
    private bool isActive = true;
    public bool IsActive
    {
        get { return isActive; }
        set
        {
            isActive = value;
            if (itemSlotUI != null)
                itemSlotUI.RefreshSlot();
        }
    }
    [SerializeField]
    private int stack; // 슬롯 스텍
    public int Stack
    {
        get { return stack; }
        set
        {
            if (itemData == null)
                stack = 0;
            else
                stack = Mathf.Clamp(value, 0, itemData.maxStack);

            if (itemSlotUI != null)
                itemSlotUI.RefreshSlot();
        }
    }
    private int level = 1;
    public int Level
    {
        get
        {
            return Mathf.Clamp(level, 1, int.MaxValue);
        }
        set
        {
            level = Mathf.Clamp(value, 1, int.MaxValue);
            if (itemSlotUI != null)
                itemSlotUI.RefreshSlot();
        }
    }
    private Unit spawnUnit;
    public Unit SpawnUnit
    {
        get
        {
            return spawnUnit;
        }
        set
        {
            spawnUnit = value;
            if (itemSlotUI != null)
                itemSlotUI.RefreshSlot();
        }
    }

    // 생성자
    public ItemSlotData(ItemData itemData)
    {
        this.itemData = itemData;
        this.Stack = 1;
        this.Level = 1;
    }
    public ItemSlotData(ItemData itemData, int level)
    {
        this.itemData = itemData;
        this.Stack = 1;
        this.Level = level;
    }
}
