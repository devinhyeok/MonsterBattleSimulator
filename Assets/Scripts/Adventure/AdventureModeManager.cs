using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class AdventureModeManager : MonoBehaviour
{
    // 인스턴스화
    private static AdventureModeManager instance;
    public static AdventureModeManager Instance
    {
        get
        {
            if (null == instance)
            {
                instance = FindObjectOfType(typeof(AdventureModeManager)) as AdventureModeManager;
                if (instance == null)
                {
                    //Debug.Log("모험 모드 매니저가 없습니다.");                    
                }
            }
            return instance;
        }
    }

    // 변수 선언
    [Header("참조용")]
    public AdventurePlayerController playerController;
    public GameObject menuPanel;
    public GameObject battlePlayer;
    public Image playImage;
    public Image fastImage;

    [Header("읽기용")]
    public List<GameObject> roomList;
    public RoomEvent roomEvent;
    public List<SpawnData> spawnDataList;
    private bool pause = true;
    public bool Pause
    {
        get { return pause; }
        set
        {
            pause = value;
            if (pause)
                playImage.color = new Color32(255, 255, 255, 255);
            else
                playImage.color = new Color32(0, 255, 0, 255);
        }
    }
    private int battleSpeed = 1;
    public int BattleSpeed
    {
        get { return battleSpeed; }
        set
        {
            battleSpeed = value;
            if (battleSpeed == 1)
                fastImage.color = new Color32(255, 255, 255, 255);
            else if (battleSpeed == 2)
                fastImage.color = new Color32(0, 255, 0, 255);
        }
    }

    private AdventureGameModeStat stat;
    public AdventureGameModeStat Stat
    {
        get { return stat; }
        set
        {
            stat = value;
            if (stat == AdventureGameModeStat.adventure)
            {
                playerController.selectItemButton.interactable = true;
                playerController.openUpgradePanelButton.interactable = true;
                playerController.moveItemButton.interactable = false;                
            }
            else
            {
                playerController.selectItemButton.interactable = false;
                playerController.openUpgradePanelButton.interactable = false;
                playerController.moveItemButton.interactable = false;
                playerController.upgradeItemPanel.gameObject.SetActive(false);
            }
        }
    }

    private int maxUnitCount = 9;
    
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

        // 시작시 로딩 상태로 시작
        Stat = AdventureGameModeStat.loading;
    }

    private void Update()
    {        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Stat == AdventureGameModeStat.battlePlanPhase || Stat == AdventureGameModeStat.battleRunPhase)
            {
                ClickPlayButton();
                return;                                
            }            
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Stat == AdventureGameModeStat.lose)
                return;
            if (!menuPanel.activeSelf)
            {                       
                PauseGame();
                menuPanel.transform.FindChild("Message").GetComponent<TextMeshProUGUI>().text = "Pause Game";
                menuPanel.SetActive(true);
            }                
            else
            {
                PlayGame();
                menuPanel.SetActive(false);
            }                
        }
    }

    private void FixedUpdate()
    {
        if (Stat == AdventureGameModeStat.battleRunPhase)
        {
            // 아군이나 적군 둘중 하나 전멸하면 배치 페이즈 시작
            if (!IsFriendVaild() || !IsEnemyVaild())
            {
                StartCoroutine(StartBattlePlanPhase());
            }
        }

        List<SpawnData> crashSpawnDataList = new List<SpawnData>();
        foreach(SpawnData spawnData in spawnDataList)
        {
            if (spawnData.spawnObject == null)
                crashSpawnDataList.Add(spawnData);
        }
        foreach(SpawnData spawnData in crashSpawnDataList)
        {
            spawnDataList.Remove(spawnData);
        }
    }

    // 전투 지역에 적 유닛이 존재하는지 검사
    private bool IsEnemyVaild()
    {
        bool isEnemyVaild = false;
        Unit[] units = FindObjectsOfType<Unit>();
        foreach (Unit unit in units)
        {
            if (unit.GetComponent<Unit>().isDead)
                continue;
            if (!unit.gameObject.activeSelf)
                continue;
            if (playerController.team != unit.GetComponent<Unit>().team)            
                isEnemyVaild = true;            
        }
        return isEnemyVaild;
    }

    // 전투 지역에 적이 존재하는지 검사
    private bool IsFriendVaild()
    {
        bool isFriendVaild = false;
        Unit[] units = FindObjectsOfType<Unit>();
        foreach (Unit unit in units)
        {            
            if (unit.GetComponent<Unit>().isDead)
                continue;
            if (!unit.gameObject.activeSelf)
                continue;
            if (playerController.team == unit.team)
                isFriendVaild = true;            
        }
        return isFriendVaild;
    }

    // 전체 아군 유닛 수 구하기
    public bool IsMaxUnitCount()
    {
        int friendCount = 0;
        foreach (SpawnData spawnData in spawnDataList)
        {
            GameObject unit = spawnData.spawnObject;
            if (playerController.team != unit.GetComponent<Unit>().team)
                continue;
            friendCount += 1;
        }
        return friendCount >= maxUnitCount;
    }


    // 죽은 아군 유닛 수 구하기
    private int GetDeadFriendCount()
    {
        int friendCount = 0;
        foreach (SpawnData spawnData in spawnDataList)
        {
            GameObject unit = spawnData.spawnObject;
            if (!unit.GetComponent<Unit>().isDead)
                continue;
            if (playerController.team != unit.GetComponent<Unit>().team)
                continue;                         
            friendCount += 1;
        }
        return friendCount;
    }

    // 전투 세팅
    public void InitBattle(BattleEvent battleEvent)
    {        
        // 현재 이벤트 설정
        roomEvent = battleEvent;
        playerController.CurrentCost = playerController.maxCost;
        Debug.Log(string.Format("{0} 이벤트 발생", roomEvent));

        // 전투 유닛 모두 활성화        
        foreach (GameObject unit in battleEvent.units)
        {
            unit.SetActive(true);
            unit.layer = LayerMask.NameToLayer("BattleUnitPlanPhase");
        }

        // 스폰 데이터 저장
        SaveSpawnData();

        // 배틀 페이즈 설정
        Stat = AdventureGameModeStat.battlePlanPhase;

        // 아이템 선택 해제
        playerController.SelectMode = false;
        playerController.ResetSelect();
    }

    // 전투 해제
    public void ReleaseBattle()
    {
        Stat = AdventureGameModeStat.adventure;
        playerController.CurrentCost = playerController.maxCost;

        // 모든 유닛 제거하기
        Unit[] units = Object.FindObjectsOfType<Unit>();
        foreach (Unit unit in units)
        {
            Destroy(unit.gameObject);            
        }

        // 모든 스킬 오브젝트 제거하기
        Skill[] skills = Object.FindObjectsOfType<Skill>();
        foreach (Skill skill in skills)
        {
            Destroy(skill.gameObject);
        }

        spawnDataList.Clear();

        // 유닛 인벤토리 초기화
        for (int i = 0; i < playerController.battleInventory.Count; i++)
        {
            if (playerController.battleInventory[i].itemData.filter == Filter.unit)
            {
                playerController.battleInventory[i].IsActive = true;
            }            
        }

        RoomEvent tempRoomEvent = roomEvent;

        // 유닛 상점 생성
        if ((roomEvent as BattleEvent).unitShop)
        {
            roomEvent = Instantiate((roomEvent as BattleEvent).unitShop, playerController.currentRoom.transform.position, Quaternion.identity).GetComponent<RoomEvent>();
        }
        Destroy(tempRoomEvent.gameObject);

        // 타임 스케일 초기화
        ResetBattlePlayer();
    }

    // 전투 배치 페이즈 시작
    public IEnumerator StartBattlePlanPhase()
    {
        Debug.Log("전투 대기 페이즈 시작 !!");
        Stat = AdventureGameModeStat.battleWaitPhase;
        playerController.CurrentCost = playerController.maxCost;

        // 모든 유닛 행동 중지
        Unit[] units = FindObjectsOfType<Unit>();
        foreach (Unit unit in units)
        {            
            unit.gameObject.layer = LayerMask.NameToLayer("BattleUnitPlanPhase");
            unit.GetComponent<Unit>().aiState = AIState.none;
            unit.rigidbody.velocity = Vector2.zero;
        }

        // 전투 종료 대기 시간
        yield return new WaitForSeconds(1f);

        // 죽은 아군 수 만큼 플레이어 피해 입힘
        playerController.CurrentHealth -= GetDeadFriendCount();
        
        // 모든 스킬 오브젝트 제거하기
        Skill[] skills = Object.FindObjectsOfType<Skill>();
        foreach (Skill skill in skills)
        {
            Destroy(skill.gameObject);
        }

        // 내 인벤토리와 필드에 유닛이 없거나 플레이어 HP가 0 이면 패배 처리
        if ((playerController.CurrentHealth <= 0))
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
            Stat = AdventureGameModeStat.battlePlanPhase;

            // 유닛 모두 원위치 시키기
            int i = 0;
            foreach (SpawnData spawnData in spawnDataList)
            {
                GameObject unit = spawnData.spawnObject;
                if (unit.GetComponent<Unit>().isDead)
                    continue;
                spawnData.spawnObject.gameObject.GetComponent<Rigidbody2D>().velocity = Vector3.zero; // 속력 없애기
                spawnData.spawnObject.gameObject.transform.position = spawnData.spawnPosition;                
                i++;
            }

            // 사망 유닛 목록에서 제거하기
            foreach (SpawnData spawnData in spawnDataList)
            {
                GameObject unit = spawnData.spawnObject;
                if (!unit.GetComponent<Unit>().isDead)
                    continue;                
                Destroy(unit);
            }
            SaveSpawnData();
        }
        ResetBattlePlayer();
    }

    // 전투 실행 페이즈 시작
    public void StartBattleRunPhase()
    {
        // 전투 실행 할 조건이 되는가 검사
        if (!IsFriendVaild())
        {
            Debug.LogWarning("아군이 없어 실행할 수 없습니다");            
            return;
        }            
        if (!IsEnemyVaild())
        {
            Debug.LogWarning("적이 없어 실행할 수 없습니다");
            return;
        }            
        if (Stat != AdventureGameModeStat.battlePlanPhase)
        {
            Debug.LogWarning("배치 페이즈때 실행해 주세요");
            return;
        }
        if (playerController.dragSlotUI.ItemSlotData != null)
        {            
            Debug.LogWarning("아이템 드래깅 중에는 실행할 수 없습니다");
            return;
        }            
        if (playerController.draggingUnit)
        {
            Debug.LogWarning("유닛 드래깅 중에는 실행할 수 없습니다");
            return;
        }            

        Debug.Log("전투 실행 페이즈 시작 !!");
        Stat = AdventureGameModeStat.battleRunPhase;

        // 유닛 AI 전부 켜고 위치 저장
        foreach (SpawnData spawnData in spawnDataList)
        {
            Unit unit = spawnData.spawnObject.GetComponent<Unit>();
            unit.gameObject.layer = LayerMask.NameToLayer("BattleUnit");
            spawnData.spawnPosition = unit.gameObject.transform.position;
            if (unit.GetComponent<Unit>().isDead)
                continue;
            unit.GetComponent<Unit>().aiState = AIState.idle;
        }        
    }

    // 전투 승리 처리
    public void WinBattle()
    {        
        playerController.Gold += ((BattleEvent)roomEvent).rewardGold; // 보상치 만큼 골드 증가        
        ReleaseBattle();
    }

    // 전투 패배 처리
    public void LoseBattle()
    {
        Debug.Log("전투 패배 !!");
        menuPanel.transform.FindChild("Message").GetComponent<TextMeshProUGUI>().text = "Lose Game";
        Stat = AdventureGameModeStat.lose;
        PauseGame();
        menuPanel.SetActive(true);
    }

    public void SaveSpawnData()
    {
        spawnDataList.Clear();
        Unit[] units = FindObjectsOfType<Unit>();
        foreach (Unit unit in units)
        {
            if (unit.isDead)
                continue;
            spawnDataList.Add(new SpawnData(unit.gameObject, unit.gameObject.transform.position));
        }
    }

    public void ClickPlayButton()
    {
        if (Stat == AdventureGameModeStat.battlePlanPhase)
        {
            if (IsEnemyVaild() && IsFriendVaild())
            {
                StartBattleRunPhase();
                PlayGame();
            }
        }
        else if (Stat == AdventureGameModeStat.battleRunPhase)
        {
            if (Pause)
                PlayGame();
            else
                PauseGame();
        }        
    }

    public void ClickBattleSpeed()
    {
        if (BattleSpeed == 1)
            BattleSpeed = 2;
        else if (BattleSpeed == 2)
            BattleSpeed = 1;
        Time.timeScale = BattleSpeed;
        Debug.Log(BattleSpeed);
    }

    public void ResetBattlePlayer()
    {
        Pause = true;
        BattleSpeed = 1;
        Time.timeScale = 1;
    }

    public void PlayGame()
    {
        Pause = false;
        Time.timeScale = BattleSpeed;
    }

    public void PauseGame()
    {
        Pause = true;
        Time.timeScale = 0;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("Adventure");
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Title");
    }
}
