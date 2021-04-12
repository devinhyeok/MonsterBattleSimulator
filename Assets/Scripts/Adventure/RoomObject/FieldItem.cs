using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FieldItem : MonoBehaviour, IClickObject
{
    [Header("참조값")]
    public SpriteRenderer spriteRenderer;
    public GameObject messageObject;
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
            }
        }
    }

    // 아이템 클릭시
    public void Click()
    {
        AdventurePlayerController playerController = AdventureModeManager.Instance.playerController;

        // 돈이 충분할 때
        Debug.Log(string.Format("아이템 습득 {0}", ItemData));
        playerController.collectInventory.Add(new ItemSlotData(ItemData));
        gameObject.SetActive(false);

        if (messageObject)
        {
            IMessage message = messageObject.GetComponent<IMessage>();
            if (message != null)
            {
                message.Message();
            }
        }
    }
}