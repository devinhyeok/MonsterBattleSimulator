using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    [Header("참조용")]
    public Image panel;
    public Image thumbnail;
    public Image healthBar;    
    public TMP_Text costText;
    public Text stackText;
    public Outline outline;

    [Header("읽기용")]
    private ItemSlotData itemSlotData; // 슬롯 아이템 정보
    public ItemSlotData ItemSlotData
    {
        get
        {
            return itemSlotData;
        }
        set
        {
            itemSlotData = value;
            RefreshSlot();
        }
    }

    private bool select;
    public bool Select
    {
        get
        {
            return select;
        }
        set
        {
            select = value;
            if (select) 
            { 
                outline.enabled = true; 
            } 
            else
            {
                outline.enabled = false;
            }
            
        }
    }

    private void Awake()
    {

    }

    private void Update()
    {
        if (itemSlotData.SpawnUnit != null)
        {
            itemSlotData.Health = itemSlotData.SpawnUnit.CurrentHealth;
            RefreshHp();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        InventoryCategory invetoryCategory = AdventureModeManager.Instance.playerController.invetoryCategory;
        Canvas canvas = AdventureModeManager.Instance.playerController.canvas;

        Debug.Log(string.Format("{0}의 {1}번 슬롯 드래깅 시작", invetoryCategory, itemSlotData.index, itemSlotData.itemData.key));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        InventoryCategory invetoryCategory = AdventureModeManager.Instance.playerController.invetoryCategory;
        Canvas canvas = AdventureModeManager.Instance.playerController.canvas;

        Debug.Log(string.Format("{0}의 {1}번 슬롯 드랍", invetoryCategory, itemSlotData.index, itemSlotData.itemData.key));
    }

    // 슬롯 새로고침
    public void RefreshSlot()
    {
        RefreshThumbnail();
        RefreshHp();
        RefreshStack();
        RefreshCanUse();
        RefreshCost();
    }

    private void RefreshThumbnail()
    {
        // 슬롯 썸네일 업데이트
        if (itemSlotData.itemData.thumbnail != null)
        {
            thumbnail.enabled = true;
            thumbnail.sprite = itemSlotData.itemData.thumbnail;
            thumbnail.SetNativeSize();
        }
    }

    private void RefreshHp()
    {
        // HP정보, 스택정보 업데이트
        if (itemSlotData.itemData.stackType == StackType.useHp)
        {
            healthBar.enabled = false;
            stackText.enabled = false;
            healthBar.fillAmount = itemSlotData.Health / itemSlotData.MaxHealth;
        }
    }

    private void RefreshStack()
    {
        if (itemSlotData.itemData.stackType == StackType.useStack)
        {
            healthBar.enabled = false;
            stackText.enabled = false;
            stackText.text = itemSlotData.Stack.ToString();
        }
    }

    private void RefreshCost()
    {
        costText.text = (itemSlotData.deltaCost + itemSlotData.itemData.cost).ToString();
    }

    private void RefreshCanUse()
    {
        // 비활성화 업데이트
        if (itemSlotData.IsActive)
        {
            panel.color = new Color32(100, 100, 100, 255);     
        }
        else
        {
            panel.color = new Color32(50, 50, 50, 255);
        }
    }
}