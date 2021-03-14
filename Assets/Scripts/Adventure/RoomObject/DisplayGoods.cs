using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayGoods : MonoBehaviour
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
}