using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdventureModeManager : MonoBehaviour
{
    // 인스턴스화
    private static AdventureModeManager instance;
    public AdventureGameModeStat stat = AdventureGameModeStat.loading;
    public AdventurePlayerController playerController;    
    public List<GameObject> roomList;
    public RoomEvent roomEvent;
    public List<GameObject> unitsInBattle;
    public List<Vector3> unitsPosition;    

    public static AdventureModeManager Instance
    {
        get
        {
            if (null == instance)
            {
                instance = FindObjectOfType(typeof(AdventureModeManager)) as AdventureModeManager;
                if (instance == null)
                {
                    Debug.Log("모험 모드 매니저가 없습니다.");                    
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        // 인스턴스화
        if (null == instance)
        {
            instance = this;
        }
        // 모드매니저가 중복되어 있는 경우 삭제
        else if (instance != this)
        {
            Destroy(this.gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (stat == AdventureGameModeStat.battlePlanPhase)
            {
                StartBattleRunPhase();
                return;
            }            
        }
    }

    private void FixedUpdate()
    {
        if (stat == AdventureGameModeStat.battleRunPhase)
        {
            // 아군이나 적군 둘중 하나 전멸하면 배치 페이즈 시작
            if (!IsFriendVaild() || !IsEnemyVaild())
            {
                StartCoroutine(StartBattlePlanPhase());
            }
        }        
    }

    // 전투 지역에 적 유닛이 존재하는지 검사
    private bool IsEnemyVaild()
    {
        bool isEnemyVaild = false;
        foreach(GameObject unit in unitsInBattle)
        {
            if (unit.GetComponent<Unit>().isDead)
                continue;
            if (playerController.team != unit.GetComponent<Unit>().team)
            {
                isEnemyVaild = true;
            }
        }
        return isEnemyVaild;
    }

    // 전투 지역에 적이 존재하는지 검사
    private bool IsFriendVaild()
    {
        bool isFriendVaild = false;
        foreach (GameObject unit in unitsInBattle)
        {
            if (unit.GetComponent<Unit>().isDead)
                continue;
            if (playerController.team == unit.GetComponent<Unit>().team)
            {
                isFriendVaild = true;
            }
        }
        return isFriendVaild;
    }

    // 죽은 아군 유닛 수 구하기
    private int GetDeadFriendCount()
    {
        int friendCount = 0;
        foreach (GameObject unit in unitsInBattle)
        {
            if (!unit.GetComponent<Unit>().isDead)
                continue;
            if (playerController.team != unit.GetComponent<Unit>().team)
                continue;                         
            friendCount += 1;
        }
        return friendCount;
    }

    // 플레이어가 소환할 유닛이 있는지 검사
    private bool IsUnitSlotVaild()
    {
        bool isUnitSlotVaild = false;
        foreach(ItemSlotData itemSlotData in playerController.unitInventory)
        {
            if (itemSlotData.Health > 0)
            {
                isUnitSlotVaild = true;
            }
        }
        return isUnitSlotVaild;

    }

    // 전투 세팅
    public void InitBattle(BattleEvent battleEvent)
    {        
        roomEvent = battleEvent;
        Debug.Log(string.Format("{0} 이벤트 발생", roomEvent));
        unitsInBattle.AddRange(battleEvent.units);

        stat = AdventureGameModeStat.battlePlanPhase;
        foreach (GameObject unit in unitsInBattle)
        {
            unit.SetActive(true);
        }
    }

    // 전투 해제
    public void ReleaseBattle()
    {
        stat = AdventureGameModeStat.adventure;
        
        // 전투중인 유닛 모두 없애기
        foreach (GameObject unit in unitsInBattle)
        {
            Destroy(unit);
        }
        unitsInBattle.Clear();

        // 유닛 인벤토리 초기화
        for (int i = 0; i < playerController.unitInventory.Count; i++)
        {
            playerController.unitInventory[i].Health = playerController.unitInventory[i].MaxHealth;
            playerController.unitInventory[i].SpawnUnit = null;
        }

        RoomEvent tempRoomEvent = roomEvent;
        // 유닛 상점 생성
        if ((roomEvent as BattleEvent).unitShop)
        {
            roomEvent = Instantiate((roomEvent as BattleEvent).unitShop, playerController.currentRoom.transform.position, Quaternion.identity).GetComponent<RoomEvent>();
        }
        Destroy(tempRoomEvent.gameObject);
    }

    // 전투 배치 페이즈 시작
    public IEnumerator StartBattlePlanPhase()
    {
        Debug.Log("전투 대기 페이즈 시작 !!");
        stat = AdventureGameModeStat.battleWaitPhase;

        // AI 모두 비활성화
        foreach (GameObject unit in unitsInBattle)
        {
            unit.GetComponent<Unit>().aiState = AIState.none;            
        }

        // 전투 종료 대기 시간
        yield return new WaitForSeconds(1f);

        // 죽은 아군 수 만큼 플레이어 피해 입힘
        playerController.CurrentHp -= GetDeadFriendCount();

        // 내 인벤토리와 필드에 유닛이 없거나 플레이어 HP가 0 이면 패배 처리
        if ((!IsFriendVaild() && !IsUnitSlotVaild()) || (playerController.CurrentHp <= 0))
        {
            LoseBattle();
        }
        // 아군이 존재하고 적이 모두 사망 했으면 사망 처리
        else if (IsFriendVaild() && !IsEnemyVaild())
        {
            WinBattle();
        }
        // 어느것도 해당하지 않으면 다음 라운드 실행
        else
        {
            Debug.Log("전투 배치 페이즈 시작 !!");
            stat = AdventureGameModeStat.battlePlanPhase;

            // 유닛 원위치 시키기
            int i = 0;
            foreach (GameObject unit in unitsInBattle)
            {
                if (unit.GetComponent<Unit>().isDead)
                    continue;
                unitsInBattle[i].gameObject.transform.position = unitsPosition[i];
                i++;
            }

            // 사망 유닛 제거하기
            foreach (GameObject unit in unitsInBattle)
            {
                if (!unit.GetComponent<Unit>().isDead)
                    continue;
                Destroy(unit);
            }

            // 전투 유닛 리스트 새로고침
            List<GameObject> units = new List<GameObject>();
            units.AddRange(unitsInBattle);
            unitsInBattle.Clear();
            foreach (GameObject unit in units)
            {
                if (unit.GetComponent<Unit>().isDead)
                    continue;
                unitsInBattle.Add(unit);
            }
        } 
    }

    // 전투 실행 페이즈 시작
    public void StartBattleRunPhase()
    {                  
        if (IsFriendVaild() && IsEnemyVaild())
        {
            Debug.Log("전투 실행 페이즈 시작 !!");
            stat = AdventureGameModeStat.battleRunPhase;

            // 유닛 AI 전부 켜고 위치 저장
            unitsPosition.Clear();
            foreach (GameObject unit in unitsInBattle)
            {
                unitsPosition.Add(unit.gameObject.transform.position);
                if (unit.GetComponent<Unit>().isDead)
                    continue;
                unit.GetComponent<Unit>().aiState = AIState.idle;
            }
        }
        else
        {
            Debug.Log("한쪽 진영에 유닛이 없어 전투를 실행할 수 없습니다.");
        }
    }

    // 전투 승리 처리
    public void WinBattle()
    {        
        playerController.Gold += ((BattleEvent)roomEvent).rewardGold; // 보상치 만큼 골드 증가
        Debug.Log(playerController.Gold);       
        ReleaseBattle();
    }

    // 전투 패배 처리
    public void LoseBattle()
    {
        Debug.Log("전투 패배 !!");
    }
}
