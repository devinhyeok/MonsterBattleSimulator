using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayGoods : MonoBehaviour, IClickObject
{    
    [Header("참조값")]
    public SpriteRenderer spriteRenderer;
    public TextMeshProUGUI textMeshProUGUI;
    private ItemData itemData;
    public ItemData ItemData
    {
        get { return itemData; }
        set 
        {
            itemData = value;
            if (itemData)
            {        
                spriteRenderer.sprite = itemData.thumbnail;
                textMeshProUGUI.text = "$" + itemData.buyGold.ToString();
            }                
        }
    }

    // 아이템 클릭시
    public void Click()
    {
        AdventurePlayerController playerController = AdventureModeManager.Instance.playerController;
        // 판매 상품 데이터 없을때
        if (!ItemData)
        {
            Debug.LogWarning("상품 오브젝트에 아이템 정보가 없습니다");
            return;
        }
        // 돈이 부족할 때
        if (ItemData.buyGold > playerController.Gold)
        {
            Debug.LogWarning("아이템을 구매할 돈이 부족합니다.");
            return;
        }
        // 돈이 충분할 때
        Debug.Log(string.Format("아이템 구매 {0}", ItemData));
        playerController.Gold -= ItemData.buyGold;
        playerController.collectInventory.Add(new ItemSlotData(ItemData));
        gameObject.SetActive(false);
    }
}