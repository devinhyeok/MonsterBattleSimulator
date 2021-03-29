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
                return Mathf.Clamp(spawnUnit.CurrentHealth, 0, MaxHealth);
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
            if (itemData.filter != Filter.unit)
            {
                Debug.LogWarning(string.Format("{0} 아이템은 유닛이 아닙니다.", itemData.key));
                return 0;
            }
            if (level <= 0)
            {
                Debug.LogWarning(string.Format("{0} 유닛의 레벨이 0 이하입니다.", itemData.key));
                return 0;
            }
            if (!Unit.GetData(itemData.key))
            {
                Debug.LogWarning(string.Format("{0} 유닛 프립팹 데이터가 없습니다.", itemData.key));
                return 0;
            }
            if (!Unit.GetData(itemData.key).unitData)
            {
                Debug.LogWarning(string.Format("{0} 유닛 프립팹에 유닛 데이터가 없습니다.", itemData.key));
                return 0;
            }
            if (Unit.GetData(itemData.key).unitData.statusList == null)
            {
                Debug.LogWarning(string.Format("{0} 유닛 스텟 데이터가 없습니다.", itemData.key));
                return 0;
            }
            maxHealth = Unit.GetData(itemData.key).unitData.statusList[level - 1].health;                
            return maxHealth;
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

    // 생성자
    public ItemSlotData(ItemData itemData)
    {
        this.itemData = itemData;
        if (itemData.stackType == StackType.useHp)
        {            
            this.Stack = 1;
            this.Level = 1;
            this.Health = MaxHealth;
        }
        else if (itemData.stackType == StackType.useStack)
        {
            this.Stack = 1;
        }
    }
    public ItemSlotData(ItemData itemData, int stack)
    {
        this.itemData = itemData;
        this.Stack = stack;

        if (itemData.stackType == StackType.useHp)
        {
            this.Stack = 1;
            this.Level = 1;
            this.Health = MaxHealth;
        }
    }
    public ItemSlotData(ItemData itemData, int stack, int level)
    {
        this.itemData = itemData;
        this.Stack = stack;
        this.Level = level;
        this.Health = MaxHealth;
    }
    public ItemSlotData(ItemData itemData, int stack, int level, float health)
    {
        this.itemData = itemData;
        this.Stack = stack;
        this.Level = level;
        this.Health = health;
    }

    public ItemSlotData(string itemName)
    {
        this.itemData = ItemData.GetData(itemName);
        if (itemData.stackType == StackType.useHp)
        {
            this.Health = MaxHealth;
            stack = 1;
        }
        else if (itemData.stackType == StackType.useStack)
        {
            stack = 1;
        }
    }
    public ItemSlotData(string itemName, int stack)
    {
        this.itemData = ItemData.GetData(itemName);
        this.Stack = stack;
        if (itemData.stackType == StackType.useHp)
        {
            this.Stack = 1;
            this.Level = 1;
            this.Health = MaxHealth;
        }
    }
    public ItemSlotData(string itemName, int stack, int level)
    {
        this.itemData = ItemData.GetData(itemName);
        this.Stack = stack;
        this.Level = level;
        this.Health = MaxHealth;
    }
    public ItemSlotData(string itemName, int stack, int level, float health)
    {
        this.itemData = ItemData.GetData(itemName);
        this.Stack = stack;
        this.Level = level;
        this.Health = health;
    }
}
