using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    [Header("참조용")]
    public SlotType slotType;
    public Image panel;
    public Image thumbnail;
    public TMP_Text costText;
    public TMP_Text levelText;   
    public TMP_Text indexText;
    public TMP_Text stackText;
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
        RefreshSlot();
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
        // 아이템 데이터 업데이트
        if (itemSlotData != null)
        {
            // 텍스트 설정
            costText.enabled = true;
            levelText.enabled = true;
            indexText.enabled = true;
            stackText.enabled = true;
            costText.text = (itemSlotData.deltaCost + itemSlotData.itemData.cost).ToString();
            levelText.text = (itemSlotData.Level).ToString();
            indexText.text = (itemSlotData.index).ToString();            
            stackText.text = itemSlotData.Stack.ToString();

            // 슬롯 썸네일 업데이트
            if (itemSlotData.itemData.thumbnail != null)
            {
                thumbnail.enabled = true;
                thumbnail.sprite = itemSlotData.itemData.thumbnail;
                thumbnail.SetNativeSize();                
            }

            // 사용가능 여부 업데이트
            if (itemSlotData.IsActive)
                panel.color = new Color32(100, 100, 100, 255);
            else
                panel.color = new Color32(50, 50, 50, 255);

            // 인벤토리 원산지 표시
            if (itemSlotData.fromSlotType == SlotType.battleSlot)
                indexText.color = new Color32(255, 0, 0, 255);
            else if (itemSlotData.fromSlotType == SlotType.collectSlot)
                indexText.color = new Color32(0, 255, 0, 255);
        }
        else
        {
            costText.enabled = false;
            levelText.enabled = false;
            indexText.enabled = false;
            stackText.enabled = false;

            thumbnail.enabled = false;
            thumbnail.sprite = null;            
            panel.color = new Color32(100, 100, 100, 255);
        }             
    }
}