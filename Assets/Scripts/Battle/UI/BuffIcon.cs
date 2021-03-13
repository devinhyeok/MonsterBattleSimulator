using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuffIcon : MonoBehaviour
{
    public BuffType buffType;
    public float percent = 1;
    public Image thumbnail; // 버프 썸네일 이미지 
    public Image fillImage; // 퍼센트 이미지

    private void Update()
    {
        fillImage.fillAmount = percent;
    }
}
