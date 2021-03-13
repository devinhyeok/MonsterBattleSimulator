using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragSlotUI : MonoBehaviour
{
    [Header("참조용")]
    public Image thumbnail;
    public Text stackText;

    [Header("읽기용")]
    private ItemSlotData itemSlotData; // 슬롯 아이템 정보
    public ItemSlotData ItemSlotData
    {
        set
        {
            itemSlotData = value;
            RefreshSlot();
        }
        get
        {
            return itemSlotData;
        }
    }

    private void Awake()
    {
        RefreshSlot();
    }

    // 슬롯 새로고침
    void RefreshSlot()
    {
        if (itemSlotData == null)
        {
            thumbnail.enabled = false;
            stackText.enabled = false;
        }
        else if (itemSlotData.itemData.thumbnail != null)
        {
            thumbnail.enabled = true;
            thumbnail.sprite = itemSlotData.itemData.thumbnail;
            thumbnail.SetNativeSize();

            if (itemSlotData.itemData.stackType == StackType.useHp)
            {
                stackText.enabled = false;
            }
            else if (itemSlotData.itemData.stackType == StackType.useStack)
            {
                stackText.enabled = true;
                stackText.text = itemSlotData.Stack.ToString();
            }
        }
    }
}
