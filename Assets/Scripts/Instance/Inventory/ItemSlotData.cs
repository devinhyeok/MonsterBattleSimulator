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

    // 프로퍼티
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

    [SerializeField]
    private float health; // 슬롯 생명력
    public float Health
    {
        get
        {
            if (spawnUnit == null)
                return health;
            else
                return Mathf.Clamp(spawnUnit.CurrentHp, 0, MaxHealth);
        }
        set
        {
            if (itemData == null)
                health = 0;
            else
                health = Mathf.Clamp(value, 0, MaxHealth);

            if (itemSlotUI != null)
                itemSlotUI.RefreshSlot();
        }
    }

    [SerializeField]
    private float maxHealth = 0;
    public float MaxHealth
    {
        get
        {
            if (UnitData.GetData(itemData.key))
            {
                maxHealth = UnitData.GetData(itemData.key).hp;
            }
            return maxHealth;
        }
    }

    // 생성자
    public ItemSlotData(ItemData itemData)
    {
        this.itemData = itemData;
        if (itemData.stackType == StackType.useHp)
        {
            this.Health = MaxHealth;
            stack = 1;
        }
        else if (itemData.stackType == StackType.useStack)
        {
            this.Health = 0;
            stack = 1;
        }
    }
    public ItemSlotData(ItemData itemData, int stack)
    {
        this.itemData = itemData;
        this.Health = 0;
        this.Stack = stack;
    }

    public ItemSlotData(ItemData itemData, float health)
    {
        this.itemData = itemData;
        this.Health = health;
        this.Stack = 1;
    }


}
