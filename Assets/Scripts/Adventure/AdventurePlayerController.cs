﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class AdventurePlayerController : MonoBehaviour
{
    [Header("편집용")]
    public int startHealth;
    public int startGold;
    public int team;
    public float cameraMoveSpeed;
    public Color colorSelected;
    public Color colorUnselected;

    [Header("참조용")]
    public Canvas canvas;
    public Camera playerCamera;    
    public Text healthText;
    public Text goldText;
    public Image unitFilter;
    public Image equipFilter;
    public GameObject content;
    public GameObject itemSlotUI;
    public UnitInfoUI unitInfoUI;
    public DragSlotUI dragSlotUI;
    public ScrollRect scrollRect;

    [Header("읽기용")]
    public InvetoryCategory invetoryCategory;
    private int maxHealth;    
    private int currentHealth;
    public int CurrentHealth
    {
        get { return currentHealth; } 
        set 
        { 
            currentHealth = Mathf.Clamp(value, 0, maxHealth);
            RefreshPlayerUI();
        }
    }
    private int gold;
    public int Gold
    {
        get { return gold; }
        set
        {
            gold = Mathf.Clamp(value, 0, int.MaxValue);
            RefreshPlayerUI();
        }
    }
    public GameObject currentRoom; // 현재 방향
    public GameObject draggingUnit; // 드래깅중인 유닛
    Vector3 dragStartPosition; 

    // 인벤토리
    public List<ItemSlotData> battleInventory = new List<ItemSlotData>();
    public List<ItemSlotData> collectionInventory = new List<ItemSlotData>();

    // 마우스 조작 관련
    GraphicRaycaster raycaster;
    EventSystem eventSystem;
    PointerEventData pointer;

    // 카메라 조작 관련
    bool isCameraMoving = false;
    RaycastHit2D[] hits;      

    private void Awake()
    {
        Input.imeCompositionMode = IMECompositionMode.Off; // 한영키 끄기

        // 컴포넌트 가져오기
        raycaster = GameObject.Find("Canvas").GetComponent<GraphicRaycaster>();
        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();

        maxHealth = startHealth;
        gold = startGold;
        CurrentHealth = maxHealth; // 시작시 피 전부 회복
    }

    private void Start()
    {
        // 시작 아이템 인벤토리 설정 (소환형)
        battleInventory.Add(new ItemSlotData("Agares"));
        battleInventory.Add(new ItemSlotData("Argos"));
        battleInventory.Add(new ItemSlotData("Eyeclops"));
        battleInventory.Add(new ItemSlotData("Fesdemic"));
        battleInventory.Add(new ItemSlotData("FogMushroom"));
        battleInventory.Add(new ItemSlotData("Fox"));
        battleInventory.Add(new ItemSlotData("GuardianGolem"));
        battleInventory.Add(new ItemSlotData("Harpy"));
        battleInventory.Add(new ItemSlotData("Hider"));
        battleInventory.Add(new ItemSlotData("Hydra"));
        battleInventory.Add(new ItemSlotData("Lizard"));
        battleInventory.Add(new ItemSlotData("MagicGolem"));
        battleInventory.Add(new ItemSlotData("MouseBoar"));
        battleInventory.Add(new ItemSlotData("Phoenix"));
        battleInventory.Add(new ItemSlotData("Ripper"));
        battleInventory.Add(new ItemSlotData("Siren"));
        battleInventory.Add(new ItemSlotData("StoneMonkey"));
        battleInventory.Add(new ItemSlotData("Tarantula"));
        battleInventory.Add(new ItemSlotData("TreeMushroom"));
        battleInventory.Add(new ItemSlotData("VolcanoTurtle"));
        battleInventory.Add(new ItemSlotData("Wolf"));
        battleInventory.Add(new ItemSlotData("Zephyr"));

        // 시작 아이템 인벤토리 설정 (스킬형)
        battleInventory.Add(new ItemSlotData("AttackPotion"));
        battleInventory.Add(new ItemSlotData("AttackSpeedPotion"));
        battleInventory.Add(new ItemSlotData("DefensePotion"));
        battleInventory.Add(new ItemSlotData("ExplosiveBomb"));
        battleInventory.Add(new ItemSlotData("FlameBomb"));
        battleInventory.Add(new ItemSlotData("Glue"));
        battleInventory.Add(new ItemSlotData("HealthPotion"));
        battleInventory.Add(new ItemSlotData("ManaBomb"));
        battleInventory.Add(new ItemSlotData("ManaPotion"));
        battleInventory.Add(new ItemSlotData("WalkSpeedPotion"));

        RefreshInventory();

        // 유닛 필터 선택한 채로 시작
        ClickBattleInventory();
    }

    private void Update()
    {                  
        // 마우스버튼 다운 검사
        CheckClickUnit();
        CheckClickAdventureObject();
        CheckDraggingSlotStart();
        CheckDraggingUnitStart();

        // 마우스버튼 눌림 검사
        CheckDraggingUnit();

        // 마우스버튼 업 검사
        CheckDropItemToField();
        CheckDraggingSlotEnd();
        CheckDraggingUnitEnd();
        CheckDropItemToShop();

        // 드래깅 중일때 설정
        if (dragSlotUI.ItemSlotData != null)
        {
            dragSlotUI.gameObject.transform.position = Input.mousePosition; // 슬롯 마우스 따라다니기
            scrollRect.StopMovement(); // 인벤토리 휠 이동 막기
            scrollRect.enabled = false;
        }
        else
        {
            scrollRect.enabled = true;
        }
        

        // 드래깅 종료 처리
        if (Input.GetMouseButtonUp(0))
        {
            dragSlotUI.ItemSlotData = null;
            currentRoom.GetComponent<Room>().spawnArea.SetActive(false);
        }            

        // 카메라 입력 검사
        if (AdventureModeManager.Instance.stat == AdventureGameModeStat.adventure)
            CheckMoveCamera();
    }

    /// ------------------------------------------------------------- 마우스 조작 관련 ------------------------------------------------------------- ///
    // 슬롯 드래깅
    void CheckDraggingSlotStart()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 마우스 포인터 정보 생성 및 저장
            pointer = new PointerEventData(eventSystem);
            pointer.position = Input.mousePosition;

            // 마우스 포인터 밑에 있는 객체 가져오기
            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(pointer, results);
            foreach(RaycastResult raycastResult in results)
            {
                if (raycastResult.gameObject.tag != "ItemSlot")
                    continue;
                ItemSlotData itemSlotData = raycastResult.gameObject.GetComponent<ItemSlotUI>().ItemSlotData;
                //Debug.Log(string.Format("{0} {1}번 슬롯 클릭 (Key: {2})", filter, itemSlotData.index, itemSlotData.itemData.key));
                dragSlotUI.ItemSlotData = itemSlotData;                
            }

            // 유닛 드래깅 시작시 배치 가능 지역 미리보기 보이기
            if (dragSlotUI.ItemSlotData == null)
                return;
            if (dragSlotUI.ItemSlotData.itemData.filter == Filter.unit &&
            AdventureModeManager.Instance.stat == AdventureGameModeStat.battlePlanPhase)
                currentRoom.GetComponent<Room>().spawnArea.SetActive(true);
        }
    }
    void CheckDraggingSlotEnd()
    {
        // 슬롯 드래깅 종료
        if (Input.GetMouseButtonUp(0))
        {
            // 마우스 포인터 정보 생성 및 저장
            pointer = new PointerEventData(eventSystem);
            pointer.position = Input.mousePosition;

            // 마우스 포인터 밑에 있는 객체 가져오기
            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(pointer, results);
            foreach (RaycastResult raycastResult in results)
            {
                if (raycastResult.gameObject.tag == "ItemSlot")
                {
                    if (dragSlotUI.ItemSlotData == null)
                        continue;                    
                    ItemSlotData itemSlotData = raycastResult.gameObject.GetComponent<ItemSlotUI>().ItemSlotData;
                    //Debug.Log(string.Format("{0} {1}번 슬롯에 {2} 아이템 드랍", filter, itemSlotData.index, dragSlotUI.ItemSlotData.itemData.key));
                }
                // 유닛 인벤토리에 넣기
                else if (raycastResult.gameObject.tag == "Inventory")
                {
                    if (!draggingUnit)
                        continue;
                    int index = FindSlot(draggingUnit.GetComponent<Unit>()).index;
                    battleInventory[index].SpawnUnit = null;
                    Destroy(draggingUnit.gameObject);
                    draggingUnit = null;
                    dragStartPosition = new Vector3();
                    AdventureModeManager.Instance.SaveSpawnData();
                }            
            }
        }
    }

    // 유닛 드래깅
    void CheckDraggingUnitStart()
    {
        // 배치 페이즈가 아니면 생략
        if (AdventureModeManager.Instance.stat != AdventureGameModeStat.battlePlanPhase)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            // 마우스 밑에 레이저 검사
            int layerMask = 1 << LayerMask.NameToLayer("BattleUnit");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, layerMask);
            if (hit.collider == null)
                return;
            if (team != hit.collider.gameObject.GetComponent<Unit>().team)
                return;

            // 드레깅 중인 유닛 넣어주기
            draggingUnit = hit.collider.gameObject;
            dragStartPosition = draggingUnit.transform.position;
            currentRoom.GetComponent<Room>().spawnArea.SetActive(true);
        }
    }
    void CheckDraggingUnit()
    {
        if (Input.GetMouseButton(0))
        {
            if (draggingUnit)
            {
                Vector3 tempPosition = Camera.main.ScreenPointToRay(Input.mousePosition).origin;
                tempPosition.z = 0;
                draggingUnit.gameObject.transform.position = tempPosition;
            }
        }
    }
    void CheckDraggingUnitEnd()
    {
        if (!draggingUnit)
            return;

        if (Input.GetMouseButtonUp(0))
        {
            // 마우스 밑에 레이저 검사
            int layerMask = 1 << LayerMask.NameToLayer("SpawnArea");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);   
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, layerMask);
            
            // 스폰 가능한 위치에 드랍했는가?
            if (hit.collider != null)
            {
                // 밑에 다른 유닛이 존재하는지 검사
                ray.origin = new Vector3(Mathf.Floor(hit.point.x) + 0.5f, Mathf.Floor(hit.point.y) + 0.5f, ray.origin.z);
                layerMask = 1 << LayerMask.NameToLayer("BattleUnit");
                RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity, layerMask);
                GameObject underUnit = null;
                foreach (RaycastHit2D _hit in hits)
                {
                    if (_hit.collider.gameObject == draggingUnit)
                        continue;
                    underUnit = _hit.collider.gameObject;
                }

                // 밑에 아무것도 없을 경우 그냥 옮기기
                if (underUnit == null)
                {
                    Vector2 point = new Vector2(Mathf.Floor(hit.point.x) + 0.5f, Mathf.Floor(hit.point.y) + 0.5f);
                    draggingUnit.transform.position = point;
                }
                // 밑에 유닛이 존재할 경우 위치 바꾸기
                else
                {
                    Vector3 tempPosition = underUnit.transform.position;
                    underUnit.transform.position = dragStartPosition;
                    draggingUnit.transform.position = tempPosition;                    
                }
            }
            // 스폰 불가능한 위치에 드랍했는가?
            else
            {
                draggingUnit.transform.position = dragStartPosition;
                Debug.LogWarning("해당 위치로 유닛을 옮길 수 없습니다.");
            }

            // 유닛 드래깅 정보 초기화
            draggingUnit = null;
            dragStartPosition = new Vector3();
        }
    }

    // 유닛 우클릭
    void CheckClickUnit()
    {
        // 클릭시 UI에 유닛 정보 가져오기
        if (Input.GetMouseButtonDown(1))
        {
            // 마우스 밑에 레이저 검사
            int layerMask = 1 << LayerMask.NameToLayer("BattleUnit");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, layerMask);

            // 유닛을 클릭했는가?
            if (!hit.collider)
            {
                unitInfoUI.unit = null;
                unitInfoUI.gameObject.SetActive(false);
            }
            else if(hit.collider.gameObject.GetComponent<Unit>() == null)
            {
                unitInfoUI.unit = null;
                unitInfoUI.gameObject.SetActive(false);
            }            
            else if (hit.collider.gameObject.GetComponent<Unit>() != null)
            {
                unitInfoUI.unit = hit.collider.gameObject.GetComponent<Unit>();
                unitInfoUI.gameObject.SetActive(true);
            }                     
        }
    }

    // 모험 오브젝트를 클릭했는지 검사
    void CheckClickAdventureObject()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 마우스 밑에 레이저 검사
            int layerMask = 1 << LayerMask.NameToLayer("AdventureObject");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, layerMask);

            if (!hit.collider)
                return;

            // 상품을 클릭했는가?
            if (hit.collider.gameObject.GetComponent<DisplayGoods>())
            {
                DisplayGoods displayGoods = hit.collider.gameObject.GetComponent<DisplayGoods>();
                if (!displayGoods.ItemData)
                {
                    Debug.LogWarning("상품 오브젝트에 아이템 정보가 없습니다");
                }
                else if (displayGoods.ItemData.buyGold <= Gold)
                {
                    Debug.Log(string.Format("아이템 구매 {0}", displayGoods.ItemData));
                    Gold -= displayGoods.ItemData.buyGold;
                    collectionInventory.Add(new ItemSlotData(displayGoods.ItemData));
                    displayGoods.gameObject.SetActive(false);
                }
                else if (displayGoods.ItemData.buyGold > Gold)
                {
                    Debug.LogWarning("아이템을 구매할 돈이 부족합니다.");
                }
            }

            // 보상 상자를 클릭했는가?
            if (hit.collider.gameObject.GetComponent<RewardEvent>())
            {
                RewardEvent rewardEvent = hit.collider.gameObject.GetComponent<RewardEvent>();
                foreach(ItemData itemData in rewardEvent.itemDatas)
                {
                    collectionInventory.Add(new ItemSlotData(itemData));
                    rewardEvent.gameObject.SetActive(false);
                }                
            }
        }
    }

    // 필드에 아이템을 드랍했는지 검사
    void CheckDropItemToField()
    {
        if (Input.GetMouseButtonUp(0))
        {
            // 필드에 아이템을 드랍했는지 검사            
            int layerMask = 1 << LayerMask.NameToLayer("Room");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);            
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, layerMask);
            if (hit.collider == null)
                return;
            if (dragSlotUI.ItemSlotData == null)
                return;            

            // 아이템 드랍한 방 정보, 좌표 가져오기
            Room room = hit.collider.gameObject.GetComponent<Room>();
            Vector2 point = hit.point;            

            // 드랍한 아이템 종류, 현재 모드 상태값 가져오기
            Filter _filter = dragSlotUI.ItemSlotData.itemData.filter;

            if (_filter == Filter.unit)
            {                 
                // 현재 배치 페이즈인지 검사
                if (AdventureModeManager.Instance.stat != AdventureGameModeStat.battlePlanPhase)
                    return;

                // 활성화된 슬롯인지 검사
                if (!dragSlotUI.ItemSlotData.isActive)
                {
                    Debug.Log(string.Format("{0}번 슬롯은 이미 비활성화된 아이템입니다.", dragSlotUI.ItemSlotData.index));
                    return;
                }

                // 해당 슬롯의 유닛이 이미 소환된 상태거나 죽었는지 검사
                if (battleInventory[dragSlotUI.ItemSlotData.index].SpawnUnit != null || battleInventory[dragSlotUI.ItemSlotData.index].Health <= 0)
                {
                    Debug.Log(string.Format("{0}번 슬롯 유닛 소환, 게임오브젝트: {1}", dragSlotUI.ItemSlotData.index, battleInventory[dragSlotUI.ItemSlotData.index].SpawnUnit));
                    return;
                }

                // 소환 지역에 소환 할려고 했는지 검사
                layerMask = 1 << LayerMask.NameToLayer("SpawnArea");
                hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, layerMask);
                if (hit.collider == null)
                    return;

                // 최대 마리수가 초과했는지 검사
                //if (AdventureModeManager.Instance.IsMaxUnitCount())
                //{
                //    Debug.LogWarning("최대 인원 수가 초과되어 더 이상 필드에 유닛을 소환할 수 없습니다.");
                //    return;
                //}

                // 소환
                Debug.Log(string.Format("{0} 유닛 소환, 좌표: {1}", dragSlotUI.ItemSlotData.itemData.key, point));
                dragSlotUI.ItemSlotData.isActive = false;
                SpawnUnit(dragSlotUI.ItemSlotData.itemData.spawnObject, point);
                RefreshInventory();
            }
            else if (_filter == Filter.battle)
            {
                if (AdventureModeManager.Instance.stat != AdventureGameModeStat.battleRunPhase)
                    return;
                Debug.Log(string.Format("{0} 스킬 소환, 좌표: {1}", dragSlotUI.ItemSlotData.itemData.key, point));
                dragSlotUI.ItemSlotData.isActive = false;
                SpawnSkill(dragSlotUI.ItemSlotData.itemData.spawnObject, point);
                RefreshInventory();
            }
        }
    }

    // 상점 주인에게 아이템을 드랍했는지 검사
    void CheckDropItemToShop()
    {
        if (Input.GetMouseButtonUp(0))
        {
            int layerMask = 1 << LayerMask.NameToLayer("AdventureObject");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, layerMask);

            if (hit.collider == null)
                return;

            if (hit.collider.gameObject.name == "Shop")
            {
                if (dragSlotUI.ItemSlotData == null)
                    return;                
                Gold += dragSlotUI.ItemSlotData.itemData.sellGold * dragSlotUI.ItemSlotData.Stack;
                Debug.Log(string.Format("{0}골드 {1}개 획득", dragSlotUI.ItemSlotData.itemData.sellGold, dragSlotUI.ItemSlotData.Stack));
                battleInventory.Remove(dragSlotUI.ItemSlotData);
            }
        }        
    }

    /// ------------------------------------------------------------- 카메라 관련 ------------------------------------------------------------- ///
    // 카메라 이동 여부 확인
    void CheckMoveCamera()
    {
        if (!isCameraMoving)
        {
            // 방이동 입력
            if (Input.GetKeyDown(KeyCode.W))
            {
                Debug.Log("이동");
                GameObject tempRoom = null;
                // 가려는 곳에 방이 있는지 검사
                int layerMask = 1 << LayerMask.NameToLayer("Room");
                hits = Physics2D.RaycastAll(currentRoom.transform.position, Vector2.up, 10f, layerMask);
                for (int i = 0; i < hits.Length; ++i)
                {
                    if (hits[i].collider.gameObject == currentRoom)
                        continue;
                    tempRoom = hits[i].collider.gameObject;                    
                    break;
                }
                // 가려는 곳에 방이 있고 문이 있는가?
                if (tempRoom && tempRoom.GetComponent<Room>().door[1])
                {
                    OpenDoor(currentRoom.GetComponent<Room>(), tempRoom.GetComponent<Room>(), Direction.up, Direction.down);
                    currentRoom = tempRoom;
                    MoveUpRoom();
                }
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                GameObject tempRoom = null;
                // 가려는 곳에 방이 있는지 검사
                int layerMask = 1 << LayerMask.NameToLayer("Room");
                hits = Physics2D.RaycastAll(currentRoom.transform.position, Vector2.down, 10f, layerMask);
                for (int i = 0; i < hits.Length; ++i)
                {
                    if (hits[i].collider.gameObject == currentRoom)
                        continue;
                    tempRoom = hits[i].collider.gameObject;
                    break;
                }
                // 가려는 곳에 방이 있고 문이 있는가?
                if (tempRoom && tempRoom.GetComponent<Room>().door[0])
                {
                    OpenDoor(currentRoom.GetComponent<Room>(), tempRoom.GetComponent<Room>(), Direction.down, Direction.up);
                    currentRoom = tempRoom;
                    MoveDownRoom();
                }
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                GameObject tempRoom = null;
                // 가려는 곳에 방이 있는지 검사
                int layerMask = 1 << LayerMask.NameToLayer("Room");
                hits = Physics2D.RaycastAll(currentRoom.transform.position, Vector2.left, 20f, layerMask);
                for (int i = 0; i < hits.Length; ++i)
                {
                    if (hits[i].collider.gameObject == currentRoom)
                        continue;
                    tempRoom = hits[i].collider.gameObject;
                    break;
                }
                // 가려는 곳에 방이 있고 문이 있는가?
                if (tempRoom && tempRoom.GetComponent<Room>().door[3])
                {
                    OpenDoor(currentRoom.GetComponent<Room>(), tempRoom.GetComponent<Room>(), Direction.left, Direction.right);
                    currentRoom = tempRoom;
                    MoveLeftRoom();
                }
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                GameObject tempRoom = null;
                // 가려는 곳에 방이 있는지 검사
                int layerMask = 1 << LayerMask.NameToLayer("Room");
                hits = Physics2D.RaycastAll(currentRoom.transform.position, Vector2.right, 20f, layerMask);
                for (int i = 0; i < hits.Length; ++i)
                {
                    if (hits[i].collider.gameObject == currentRoom)
                        continue;
                    tempRoom = hits[i].collider.gameObject;
                    break;
                }
                // 가려는 곳에 방이 있고 문이 있는가?
                if (tempRoom && tempRoom.GetComponent<Room>().door[2])
                {
                    OpenDoor(currentRoom.GetComponent<Room>(), tempRoom.GetComponent<Room>(), Direction.right, Direction.left);
                    currentRoom = tempRoom;
                    MoveRightRoom();
                }
            }
        }
    }

    // 방 이동
    void MoveUpRoom()
    {
        Vector3 tempVector = new Vector3(0f, 12f, 0f);
        StartCoroutine(MoveCamera(tempVector));
    }
    void MoveDownRoom()
    {
        Vector3 tempVector = new Vector3(0f, -12f, 0f);
        StartCoroutine(MoveCamera(tempVector));
    }
    void MoveLeftRoom()
    {
        Vector3 tempVector = new Vector3(-20f, 0f, 0f);
        StartCoroutine(MoveCamera(tempVector));
    }
    void MoveRightRoom()
    {
        Vector3 tempVector = new Vector3(20f, 0f, 0f);
        StartCoroutine(MoveCamera(tempVector));
    }

    // 문 열기
    void OpenDoor(Room room1, Room room2, Direction direction1, Direction direction2)
    {
        if (direction1 == Direction.up)
            room1.way[0].SetActive(true);
        else if (direction1 == Direction.down)
            room1.way[1].SetActive(true);
        else if (direction1 == Direction.left)
            room1.way[2].SetActive(true);
        else if (direction1 == Direction.right)
            room1.way[3].SetActive(true);

        if (direction2 == Direction.up)
            room2.way[0].SetActive(true);
        else if (direction2 == Direction.down)
            room2.way[1].SetActive(true);
        else if (direction2 == Direction.left)
            room2.way[2].SetActive(true);
        else if (direction2 == Direction.right)
            room2.way[3].SetActive(true);
    }

    // 문 닫기
    void CloseDoor()
    {
        // 길 모두 숨기기
        foreach(GameObject tempObject in AdventureModeManager.Instance.roomList)
        {
            if (tempObject.GetComponent<Room>().way[0])
                tempObject.GetComponent<Room>().way[0].SetActive(false);
            if (tempObject.GetComponent<Room>().way[1])
                tempObject.GetComponent<Room>().way[1].SetActive(false);
            if (tempObject.GetComponent<Room>().way[2])
                tempObject.GetComponent<Room>().way[2].SetActive(false);
            if (tempObject.GetComponent<Room>().way[3])
                tempObject.GetComponent<Room>().way[3].SetActive(false);
        }
        if (currentRoom.GetComponent<Room>().roomEvent == null)
            return;
        if (currentRoom.GetComponent<Room>().roomEvent.GetComponent<RoomEvent>() == null)
            return;
        currentRoom.GetComponent<Room>().roomEvent.GetComponent<RoomEvent>().EnterRoom();
        
    }

    // 카메라 이동 함수
    IEnumerator MoveCamera(Vector3 forcedVelocity)
    {
        isCameraMoving = true;
        Vector3 targetPosition = playerCamera.transform.position + forcedVelocity;

        while ((playerCamera.transform.position - targetPosition).magnitude > 0f)
        {
            playerCamera.transform.position = Vector3.MoveTowards(playerCamera.transform.position, targetPosition, Time.deltaTime * cameraMoveSpeed);
            yield return new WaitForEndOfFrame();
        }
        playerCamera.transform.position = targetPosition;
        isCameraMoving = false;
        CloseDoor();
    }

    /// ------------------------------------------------------------- HUD 관련 ------------------------------------------------------------- ///
    // 플레이어 정보에 따라 UI 업데이트
    void RefreshPlayerUI()
    {
        healthText.text = CurrentHealth.ToString() + "/" + maxHealth.ToString();
        goldText.text = gold.ToString();
    }

    // 인벤토리 UI 새로고침
    void RefreshInventory()
    {
        // 인벤토리 정보 가져오기
        List<ItemSlotData> inventory = new List<ItemSlotData>();
        if (invetoryCategory == InvetoryCategory.Battle)
        {
            battleInventory = battleInventory.OrderBy(_itemSlotData => _itemSlotData.itemData.key).ToList(); // 키순으로 정렬
            inventory = battleInventory;
        }
        else
        {
            collectionInventory = collectionInventory.OrderBy(_itemSlotData => _itemSlotData.itemData.key).ToList(); // 키순으로 정렬
            inventory = collectionInventory;
        }        

        // itemSlotUI 전부 없애기
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }

        // itemSlotUI 재생성
        int i = 0;
        foreach (ItemSlotData itemSlotData in inventory)
        {
            ItemSlotUI _itemSlotUI = Instantiate(itemSlotUI, content.transform).GetComponent<ItemSlotUI>();
            _itemSlotUI.ItemSlotData = itemSlotData;
            _itemSlotUI.ItemSlotData.itemSlotUI = _itemSlotUI;
            _itemSlotUI.ItemSlotData.index = i;            
            i++;
        }
    }

    // 유닛 필터
    public void ClickBattleInventory()
    {
        invetoryCategory = InvetoryCategory.Battle;
        unitFilter.color = colorSelected;
        equipFilter.color = colorUnselected;
        RefreshInventory();
    }

    // 장비 필터
    public void ClickCollectionInventory()
    {
        invetoryCategory = InvetoryCategory.Collection;
        unitFilter.color = colorUnselected;
        equipFilter.color = colorSelected;
        RefreshInventory();
    }

    /// ------------------------------------------------------------- 인벤토리 관련 ------------------------------------------------------------- ///

    public ItemSlotData FindSlot(Unit unit)
    {
        foreach(ItemSlotData itemSlotData in battleInventory)
        {
            if (unit == itemSlotData.SpawnUnit)
            {
                return itemSlotData;
            }                
        }
        return null;
    }

    /// ------------------------------------------------------------- 기타 함수 ------------------------------------------------------------- ///
    void SpawnUnit(GameObject _unitObject, Vector2 point)
    {
        // 유닛 데이터 가져오기
        GameObject unitObject = _unitObject;
        if (unitObject == null)
        {
            Debug.LogWarning("소환할 유닛 오브젝트가 없습니다");
            return;
        }
        
        // 유닛 소환할 좌표 가져오기
        point = new Vector2(Mathf.Floor(point.x) + 0.5f, Mathf.Floor(point.y) + 0.5f);

        // 유닛 소환하기
        Unit unit = Instantiate(unitObject, point, Quaternion.identity).GetComponent<Unit>();
        unit.team = team;

        // 유닛 소환 후 처리        
        battleInventory[dragSlotUI.ItemSlotData.index].SpawnUnit = unit;
        AdventureModeManager.Instance.SaveSpawnData();
    }

    void SpawnSkill(GameObject _skillObject, Vector2 point)
    {
        // 스킬 데이터 가져오기
        GameObject skillObject = _skillObject;
        if (skillObject == null)
        {
            Debug.LogWarning("소환할 스킬 오브젝트가 없습니다.");
            return;
        }

        // 스킬 소환하기
        if (skillObject.GetComponent<Skill>())
        {
            Skill skill = Instantiate(skillObject, point, Quaternion.identity).GetComponent<Skill>();
            skill.team = team;
        }
        else
        {
            Debug.LogWarning("소환한 오브젝트가 스킬이 아닙니다.");            
        }        
    }
}
