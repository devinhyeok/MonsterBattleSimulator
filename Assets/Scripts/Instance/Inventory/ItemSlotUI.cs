using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour
{
    [Header("참조용")]
    public Image panel;
    public Image thumbnail;
    public Image hpBar;
    public Text stackText;

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

    private void Awake()
    {

    }

    private void Update()
    {
        if (itemSlotData.SpawnUnit != null)
        {
            itemSlotData.Health = itemSlotData.SpawnUnit.CurrentHp;
            RefreshHp();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Filter filter = AdventureModeManager.Instance.playerController.filter;
        Canvas canvas = AdventureModeManager.Instance.playerController.canvas;

        Debug.Log(string.Format("{0}의 {1}번 슬롯 드래깅 시작", filter, itemSlotData.index, itemSlotData.itemData.key));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Filter filter = AdventureModeManager.Instance.playerController.filter;
        Canvas canvas = AdventureModeManager.Instance.playerController.canvas;

        Debug.Log(string.Format("{0}의 {1}번 슬롯 드랍", filter, itemSlotData.index, itemSlotData.itemData.key));
    }

    // 슬롯 새로고침
    public void RefreshSlot()
    {
        RefreshThumbnail();
        RefreshHp();
        RefreshStack();
        RefreshCanUse();
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
            hpBar.enabled = true;
            stackText.enabled = false;
            hpBar.fillAmount = itemSlotData.Health / itemSlotData.MaxHealth;
        }
    }

    private void RefreshStack()
    {
        if (itemSlotData.itemData.stackType == StackType.useStack)
        {
            hpBar.enabled = false;
            stackText.enabled = true;
            stackText.text = itemSlotData.Stack.ToString();
        }
    }

    private void RefreshCanUse()
    {
        // 비활성화 업데이트
        if (itemSlotData.itemData.filter == Filter.unit)
        {
            if (itemSlotData.SpawnUnit != null || itemSlotData.Health <= 0)
            {
                panel.color = new Color32(50, 50, 50, 255);
            }
            else
            {
                panel.color = new Color32(100, 100, 100, 255);
            }
        }
    }
}