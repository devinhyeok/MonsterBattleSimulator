using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeItemPanel : MonoBehaviour
{
    [Header("참조용")]
    public GameObject materialItemSlotPanel;
    public ItemSlotUI upgradeItemSlotUI;
    public GameObject itemSlotUI;

    [Header("읽기용")]
    public List<ItemSlotData> materialInventory;

    private ItemSlotData upgradeItemSlotData;
    public ItemSlotData UpgradeItemSlotData
    {
        get { return upgradeItemSlotData; }
        set 
        {            
            upgradeItemSlotData = value;
            upgradeItemSlotUI.ItemSlotData = upgradeItemSlotData;             
            FindMaterialItemSlot();
            Debug.Log("업글레이트 아이템 등록");
        }
    }

    public void Close()
    {
        foreach (Transform child in materialItemSlotPanel.transform)
        {
            Destroy(child.gameObject);
        }
        transform.Find("Panel").transform.Find("ItemSlotUI").GetComponent<ItemSlotUI>().ItemSlotData = null;
        gameObject.SetActive(false);
    }

    public void FindMaterialItemSlot()
    {
        Debug.Log("재료 아이템 찾기");

        // 초기화
        AdventurePlayerController playerController = AdventureModeManager.Instance.playerController;
        materialInventory.Clear();

        // 배틀 인벤토리에서 찾기
        foreach(ItemSlotData itemSlotData in playerController.battleInventory)
        {
            if (upgradeItemSlotData == itemSlotData)
                continue;
            if (upgradeItemSlotData.itemData == itemSlotData.itemData)
            {
                materialInventory.Add(itemSlotData);
            }
        }

        // 수집 인벤토리에서 찾기
        foreach (ItemSlotData itemSlotData in playerController.collectInventory)
        {
            if (upgradeItemSlotData == itemSlotData)
                continue;
            if (upgradeItemSlotData.itemData == itemSlotData.itemData)
            {
                materialInventory.Add(itemSlotData);
            }
        }
        RefreshInventory();
    }

    public void RefreshInventory()
    {
        // 재료 슬롯 리스트 출력        
        foreach (Transform child in materialItemSlotPanel.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (ItemSlotData itemSlotData in materialInventory)
        {
            ItemSlotUI _itemSlotUI = Instantiate(itemSlotUI, materialItemSlotPanel.transform).GetComponent<ItemSlotUI>();
            _itemSlotUI.ItemSlotData = itemSlotData;
            _itemSlotUI.ItemSlotData.itemSlotUI = _itemSlotUI;
            _itemSlotUI.slotType = SlotType.materialSlot;
            _itemSlotUI.RefreshSlot();
        }
    }
}
