using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleRoom : MonoBehaviour, IRoomEvent
{
    bool onEvent = true;
    public List<GameObject> units;

    public void EnterRoom()
    {
        if (onEvent)
        {
            onEvent = false;
            AdventureModeManager.Instance.InitBattle(units);
        }
    }

    private void Awake()
    {
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
