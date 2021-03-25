using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitInfoUI : MonoBehaviour
{
    [Header("참조값")]
    public Text nameText;
    public Text healthText;    
    public Text mpText;
    public Text healthRegenText;
    public Text manaRegenText;
    public Text attackText;
    public Text attackSpeedText;
    public Text attackDistanceText;
    public Text defenseText;
    public Text abilityPowerText;
    public Text walkSpeedText;
    public Image image;
    public Transform panel3;
    public GameObject buffIcon;

    [Header("읽기용")]
    public Unit unit;

    private Dictionary<BuffType, GameObject> buffIconDictionary = new Dictionary<BuffType, GameObject>();

    private void Awake()
    {
        // 버프 아이콘 배열 풀 미리 만들어 두기
        foreach(BuffType buffType in Enum.GetValues(typeof(BuffType)))
        {
            // 버프 아이콘 생성 후 버프 타입 설정
            GameObject tempBuffIcon =Instantiate(buffIcon, panel3);
            tempBuffIcon.GetComponent<BuffIcon>().buffType = buffType;

            // 버프 아이콘 썸네일 가져오기
            GameObject tempBuffGameObject = buffType.ToString().GetBuffPrefab();
            Buff tempBuff = tempBuffGameObject.GetComponent<Buff>();
            tempBuffIcon.GetComponent<BuffIcon>().thumbnail.sprite = tempBuff.thumbnail;
            
            // 딕셔너리에 버프 아이콘 오브젝트 저장
            buffIconDictionary.Add(buffType, tempBuffIcon);
        }
    }

    private void Update()
    {
        if (unit)
        {
            // 유닛 키로 유닛 아이템 썸네일 가져오기
            image.sprite = ItemData.GetData(unit.key).thumbnail;
            image.SetNativeSize();
            if (image.sprite != null)
            {
                image.gameObject.SetActive(true);
            }

            // 유닛 정보 읽은 후 설정해주기
            nameText.text = ItemData.GetData(unit.key).koreanName;
            healthText.text = ((int)unit.CurrentHealth).ToString() + "/" + ((int)unit.maxHealth).ToString();
            healthRegenText.text = ((int)unit.currentHealthRegen).ToString();
            mpText.text = ((int)unit.CurrentMp).ToString() + "/" + ((int)unit.maxMana).ToString();
            manaRegenText.text = ((int)unit.currentManaRegen).ToString();
            attackText.text = ((int)unit.currentAttackPower).ToString();
            attackSpeedText.text = ((int)unit.CurrentAttackSpeed).ToString();
            attackDistanceText.text = ((int)unit.currentAttackDistance).ToString();
            defenseText.text = ((int)unit.currentAttackArmor).ToString();
            abilityPowerText.text = ((int)unit.currentSpellPower).ToString();          
            walkSpeedText.text = ((int)unit.currentWalkSpeed).ToString();
            
            // 버프 정보 읽어온 후 버프 아이콘 설정하기
            foreach (BuffType buffType in Enum.GetValues(typeof(BuffType)))
            {
                if (unit.buffDictionary[buffType].currentSecond <= 0)
                {
                    buffIconDictionary[buffType].GetComponent<BuffIcon>().percent = 1;
                    buffIconDictionary[buffType].SetActive(false);
                }
                else
                {
                    float percent = unit.buffDictionary[buffType].currentSecond / unit.buffDictionary[buffType].maxSecond;
                    buffIconDictionary[buffType].GetComponent<BuffIcon>().percent = percent;
                    buffIconDictionary[buffType].SetActive(true);
                }
            }
        }     
    }
}
