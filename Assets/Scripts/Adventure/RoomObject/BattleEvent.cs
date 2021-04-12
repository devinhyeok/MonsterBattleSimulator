using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEvent : RoomEvent
{
    [Header("편집값")]
    public int rewardGold;
    public GameObject unitShop;
    public GameObject grid;

    [Header("읽기값")]
    public List<GameObject> units;

    bool onEvent = true;

    public override void EnterRoom()
    {
        if (onEvent)
        {
            onEvent = false;
            AdventureModeManager.Instance.InitBattle(gameObject.GetComponent<BattleEvent>());
        }
    }

    private void Awake()
    {
        grid.SetActive(false);
        // 이 배틀 이벤트에 속해있는 유닛 모두 가져오기
        foreach (Transform child in transform)
        {
            if (child.gameObject.tag == "Unit")
            {
                units.Add(child.gameObject);
                child.gameObject.SetActive(false);
            }                
        }
    }
}
