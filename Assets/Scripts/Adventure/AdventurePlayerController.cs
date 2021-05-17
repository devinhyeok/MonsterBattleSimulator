using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using TMPro;

public class AdventurePlayerController : MonoBehaviour
{
    [Header("편집용")]
    public int startHealth;
    public int startCost;
    public int startGold;
    public int team;
    public int maxBattleInventorySlotCount;
    public float cameraMoveSpeed;
    public Color colorSelected;
    public Color colorUnselected;
    

    [Header("참조용")]
    public Canvas canvas;
    public Camera playerCamera;
    public TMP_Text healthText;
    public TMP_Text costText;
    public TMP_Text goldText;       
    public Image unitFilter;
    public Image equipFilter;
    public Image selectButton;
    public Image moveButton;
    public GameObject content;
    public GameObject itemSlotUI;
    public UnitInfoPanel unitInfoPanel;
    public DragSlotUI dragSlotUI;
    public ScrollRect scrollRect;
    public UpgradeItemPanel upgradeItemPanel;
    public Button selectItemButton;
    public Button moveItemButton;
    public Button openUpgradePanelButton;
    public Button upgradeItemButton;

    [Header("읽기용")]
    public InventoryCategory invetoryCategory;
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
    public int maxCost;
    private int currentCost;
    public int CurrentCost
    {
        get { return currentCost; }
        set
        {
            currentCost = Mathf.Clamp(value, 0, maxCost);
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

    private bool selectMode;
    public bool SelectMode 
    {
        get { return selectMode; }
        set
        {            
            selectMode = value;
            if (selectMode)
                selectButton.color = colorSelected;
            else
                selectButton.color = colorUnselected;
        }
    }

    private bool upgradeMode;
    public bool UpgradeMode
    {
        get { return upgradeMode; }
        set
        {
            upgradeMode = value;
            if (upgradeMode)
                openUpgradePanelButton.GetComponent<Image>().color = colorSelected;
            else
                openUpgradePanelButton.GetComponent<Image>().color = colorUnselected;
        }
    }

    public GameObject currentRoom; // 현재 방향
    public GameObject draggingUnit; // 드래깅중인 유닛
    Vector3 dragStartPosition; 

    // 인벤토리
    public List<ItemSlotData> battleInventory = new List<ItemSlotData>();
    public List<ItemSlotData> collectInventory = new List<ItemSlotData>();

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
        maxCost = startCost;
        gold = startGold;
        CurrentHealth = maxHealth; // 시작시 피 전부 회복
        CurrentCost = maxCost;
    }

    private void Start()
    {

        // 시작 아이템 인벤토리 설정 (소환형)
        AddBattleInventory(new ItemSlotData(ItemData.Get("VacanoTurtle")));
        AddBattleInventory(new ItemSlotData(ItemData.Get("WildBoar")));
        AddBattleInventory(new ItemSlotData(ItemData.Get("Bear")));
        AddBattleInventory(new ItemSlotData(ItemData.Get("Slime")));

        AddBattleInventory(new ItemSlotData(ItemData.Get("FireWyvern")));
        AddBattleInventory(new ItemSlotData(ItemData.Get("WaterDragon")));
        AddBattleInventory(new ItemSlotData(ItemData.Get("Cobra")));
        AddBattleInventory(new ItemSlotData(ItemData.Get("IronEagle")));

        AddBattleInventory(new ItemSlotData(ItemData.Get("Spider")));
        AddBattleInventory(new ItemSlotData(ItemData.Get("Phoenix")));
        AddBattleInventory(new ItemSlotData(ItemData.Get("Larva")));
        AddBattleInventory(new ItemSlotData(ItemData.Get("NightWolf")));      

        // 시작 아이템 인벤토리 설정 (스킬형)
        collectInventory.Add(new ItemSlotData(ItemData.Get("VacanoTurtle")));
        collectInventory.Add(new ItemSlotData(ItemData.Get("WildBoar")));
        collectInventory.Add(new ItemSlotData(ItemData.Get("Bear")));
        collectInventory.Add(new ItemSlotData(ItemData.Get("Slime")));
        collectInventory.Add(new ItemSlotData(ItemData.Get("DividedSlime")));
        collectInventory.Add(new ItemSlotData(ItemData.Get("FireWyvern")));
        collectInventory.Add(new ItemSlotData(ItemData.Get("WaterDragon")));
        collectInventory.Add(new ItemSlotData(ItemData.Get("Cobra")));
        collectInventory.Add(new ItemSlotData(ItemData.Get("IronEagle")));
        collectInventory.Add(new ItemSlotData(ItemData.Get("Spider")));
        collectInventory.Add(new ItemSlotData(ItemData.Get("Phoenix")));
        collectInventory.Add(new ItemSlotData(ItemData.Get("Larva")));
        collectInventory.Add(new ItemSlotData(ItemData.Get("BabyLarva")));
        collectInventory.Add(new ItemSlotData(ItemData.Get("NightWolf")));
        collectInventory.Add(new ItemSlotData(ItemData.Get("Reaper")));
        collectInventory.Add(new ItemSlotData(ItemData.Get("DeathWorm")));
        collectInventory.Add(new ItemSlotData(ItemData.Get("IcePenguin")));
        RefreshInventory();

        // 유닛 필터 선택한 채로 시작
        unitFilter.color = colorSelected;
        equipFilter.color = colorUnselected;        
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
        if (AdventureModeManager.Instance.Stat == AdventureGameModeStat.adventure)
            CheckMoveCamera();
    }

    /// ------------------------------------------------------------- 마우스 조작 관련 ------------------------------------------------------------- ///
    // 슬롯 드래깅
    void CheckDraggingSlotStart()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 가져올 데이터 정보
            ItemSlotUI itemSlotUI = null;

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
                itemSlotUI = raycastResult.gameObject.GetComponent<ItemSlotUI>();
                //Debug.Log(string.Format("{0} {1}번 슬롯 클릭 (Key: {2})", filter, itemSlotData.index, itemSlotData.itemData.key));                
            }

            // 데이터 없으면 스킵
            if (itemSlotUI == null)
                return; 
            if (itemSlotUI.ItemSlotData == null)
                return;

            // 유닛 활성화 비활성화
            if (itemSlotUI.slotType == SlotType.battleSlot)
            {
                if (!SelectMode)
                {
                    // 드래깅 시작
                    dragSlotUI.ItemSlotData = itemSlotUI.ItemSlotData;

                    // 유닛 드래깅 시작시 배치 가능 지역 미리보기 보이기                
                    if (dragSlotUI.ItemSlotData.itemData.filter == Filter.unit && AdventureModeManager.Instance.Stat == AdventureGameModeStat.battlePlanPhase)
                        currentRoom.GetComponent<Room>().spawnArea.SetActive(true);
                }
                else
                {
                    // 슬롯 선택
                    itemSlotUI.Select = !itemSlotUI.Select;
                }
            }
            else if (itemSlotUI.slotType == SlotType.collectSlot)
            {
                if (SelectMode)
                {
                    itemSlotUI.Select = !itemSlotUI.Select;
                }                
            }
            else if (itemSlotUI.slotType == SlotType.materialSlot)
            {
                // 슬롯 선택
                itemSlotUI.Select = !itemSlotUI.Select;
            }
            else if (itemSlotUI.slotType == SlotType.upgradeSlot)
            {
                
            }            
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

            // 마우스 포인터 밑에 있는 객체 가져오기 (UI)
            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(pointer, results);
            foreach (RaycastResult raycastResult in results)
            {
                // 아이템 슬롯에 드래그
                if (raycastResult.gameObject.tag == "ItemSlot")
                {
                    if (dragSlotUI.ItemSlotData == null)
                        continue;
                    ItemSlotUI itemSlot = raycastResult.gameObject.GetComponent<ItemSlotUI>();                    
                    if (itemSlot.slotType == SlotType.upgradeSlot)
                    {
                        Debug.Log("아이템 등록");
                        upgradeItemPanel.UpgradeItemSlotData = dragSlotUI.ItemSlotData;                        
                    }
                }
                // 유닛 인벤토리에 넣기
                else if (raycastResult.gameObject.tag == "Inventory")
                {
                    if (!draggingUnit)
                        continue;
                    int index = FindSlot(draggingUnit.GetComponent<Unit>()).index;

                    // 비용 회수
                    CurrentCost += battleInventory[index].deltaCost + battleInventory[index].itemData.cost;

                    // 유닛 인벤토리에 넣기                 
                    battleInventory[index].SpawnUnit = null;
                    battleInventory[index].IsActive = true;
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
        if (AdventureModeManager.Instance.Stat != AdventureGameModeStat.battlePlanPhase)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            // 마우스 밑에 레이저 검사
            int layerMask = 1 << LayerMask.NameToLayer("BattleUnitPlanPhase");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, layerMask);
            if (hit.collider == null)
                return;
            if (team != hit.collider.gameObject.GetComponent<Unit>().team)
                return;

            // 드레깅 가능한 장소 보여주기            
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
                layerMask = 1 << LayerMask.NameToLayer("BattleUnitPlanPhase");
                RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity, layerMask);
                GameObject underUnit = null;
                foreach (RaycastHit2D _hit in hits)
                {
                    if (_hit.collider.gameObject == draggingUnit.gameObject)
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
                unitInfoPanel.unit = null;
                unitInfoPanel.gameObject.SetActive(false);
            }
            else if(hit.collider.gameObject.GetComponent<Unit>() == null)
            {
                unitInfoPanel.unit = null;
                unitInfoPanel.gameObject.SetActive(false);
            }
            else if (hit.collider.gameObject.GetComponent<Unit>() != null)
            {
                unitInfoPanel.unit = hit.collider.gameObject.GetComponent<Unit>();
                unitInfoPanel.gameObject.SetActive(true);
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

            // 클릭 오브젝트 클릭했다고 알림
            IClickObject clickObject = hit.collider.gameObject.GetComponent<IClickObject>();
            if (clickObject == null)
                return;
            clickObject.Click();

            // 보상 상자를 클릭했는가?

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
                if (AdventureModeManager.Instance.Stat != AdventureGameModeStat.battlePlanPhase)
                    return;

                // 활성화된 슬롯인지 검사
                if (!dragSlotUI.ItemSlotData.IsActive)
                {
                    Debug.Log(string.Format("{0}번 슬롯은 이미 비활성화된 아이템입니다.", dragSlotUI.ItemSlotData.index));
                    return;
                }

                // 해당 슬롯의 유닛이 이미 소환된 상태거나 죽었는지 검사
                if (battleInventory[dragSlotUI.ItemSlotData.index].SpawnUnit != null)
                {
                    Debug.Log(string.Format("{0}번 슬롯 유닛 소환, 게임오브젝트: {1}", dragSlotUI.ItemSlotData.index, battleInventory[dragSlotUI.ItemSlotData.index].SpawnUnit));
                    return;
                }

                // 소환 지역에 소환 할려고 했는지 검사
                layerMask = 1 << LayerMask.NameToLayer("SpawnArea");
                hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, layerMask);
                if (hit.collider == null)
                    return;

                // 소환할 위치에 다른 유닛이 존재하는지 검사
                ray.origin = new Vector3(Mathf.Floor(hit.point.x) + 0.5f, Mathf.Floor(hit.point.y) + 0.5f, ray.origin.z);
                layerMask = 1 << LayerMask.NameToLayer("BattleUnitPlanPhase");
                RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity, layerMask);
                GameObject underUnit = null;
                foreach (RaycastHit2D _hit in hits)
                {
                    if (_hit.collider.gameObject == draggingUnit.gameObject)
                        continue;
                    underUnit = _hit.collider.gameObject;
                }
                if (underUnit != null)
                {
                    Debug.Log("밑에 유닛이 있어 소환할 수 없습니다.");
                    return;
                }
                    
                // 최대 마리수가 초과했는지 검사
                //if (AdventureModeManager.Instance.IsMaxUnitCount())
                //{
                //    Debug.LogWarning("최대 인원 수가 초과되어 더 이상 필드에 유닛을 소환할 수 없습니다.");
                //    return;
                //}

                // 코스트가 충분한지 검사
                int tempCost = battleInventory[dragSlotUI.ItemSlotData.index].deltaCost + battleInventory[dragSlotUI.ItemSlotData.index].itemData.cost;
                if (CurrentCost - tempCost < 0)
                {
                    Debug.LogWarning("코스트가 부족합니다");
                    return;
                }
                CurrentCost -= tempCost;

                // 밑에 유닛이 있는지 검사

                // 소환
                Debug.Log(string.Format("{0} 유닛 소환, 좌표: {1}", dragSlotUI.ItemSlotData.itemData.key, point));
                dragSlotUI.ItemSlotData.IsActive = false;
                Unit unit = SpawnUnit(dragSlotUI.ItemSlotData.itemData.spawnObject, point);                
                unit.Level = dragSlotUI.ItemSlotData.Level;
                RefreshInventory();
            }
            else if (_filter == Filter.battle)
            {
                if (AdventureModeManager.Instance.Stat != AdventureGameModeStat.battleRunPhase)
                    return;

                // 코스트가 충분한지 검사
                int tempCost = battleInventory[dragSlotUI.ItemSlotData.index].deltaCost + battleInventory[dragSlotUI.ItemSlotData.index].itemData.cost;
                if (CurrentCost - tempCost < 0)
                {
                    Debug.LogWarning("코스트가 부족합니다");
                    return;
                }
                CurrentCost -= tempCost;

                Debug.Log(string.Format("{0} 스킬 소환, 좌표: {1}", dragSlotUI.ItemSlotData.itemData.key, point));
                dragSlotUI.ItemSlotData.IsActive = false;
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

        // 해당 방의 이벤트로 왔다고 알림
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
        costText.text = CurrentCost.ToString() + "/" + maxCost.ToString();
        goldText.text = gold.ToString();
    }

    // 인벤토리 UI 새로고침
    void RefreshInventory()
    {
        // 인벤토리 키순으로 정렬
        battleInventory = battleInventory.OrderBy(_itemSlotData => _itemSlotData.itemData.key).ToList(); // 키순으로 정렬
        collectInventory = collectInventory.OrderBy(_itemSlotData => _itemSlotData.itemData.key).ToList(); // 키순으로 정렬

        // 인벤토리 데이터 설정
        int i = 0;
        foreach (ItemSlotData itemSlotData in battleInventory)
        {
            itemSlotData.fromSlotType = SlotType.battleSlot;
            itemSlotData.index = i;
            i++;
        }
        i = 0;
        foreach (ItemSlotData itemSlotData in collectInventory)
        {
            itemSlotData.fromSlotType = SlotType.collectSlot;
            itemSlotData.index = i;
            i++;
        }

        // 인벤토리 정보 가져오기
        List<ItemSlotData> inventory = new List<ItemSlotData>();
        SlotType slotType;
        if (invetoryCategory == InventoryCategory.battle)
        {            
            inventory = battleInventory;
            slotType = SlotType.battleSlot;
        }
        else
        {            
            inventory = collectInventory;
            slotType = SlotType.collectSlot;
        }

        // itemSlotUI 전부 없애기
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }

        // itemSlotUI 재생성        
        foreach (ItemSlotData itemSlotData in inventory)
        {
            ItemSlotUI _itemSlotUI = Instantiate(itemSlotUI, content.transform).GetComponent<ItemSlotUI>();
            _itemSlotUI.ItemSlotData = itemSlotData;
            _itemSlotUI.ItemSlotData.itemSlotUI = _itemSlotUI;            
            _itemSlotUI.ItemSlotData.fromSlotType = slotType;
            _itemSlotUI.slotType = slotType;            
            _itemSlotUI.RefreshSlot();
            i++;
        }
    }

    // 유닛 필터
    public void ClickBattleInventory()
    {
        if (invetoryCategory == InventoryCategory.battle)
            return;
        invetoryCategory = InventoryCategory.battle;
        unitFilter.color = colorSelected;
        equipFilter.color = colorUnselected;

        RefreshInventory();
        ResetSelect();
    }

    // 장비 필터
    public void ClickCollectionInventory()
    {
        if (invetoryCategory == InventoryCategory.collection)
            return;
        invetoryCategory = InventoryCategory.collection;
        unitFilter.color = colorUnselected;
        equipFilter.color = colorSelected;
        
        RefreshInventory();
        ResetSelect();        
    }

    public void ResetSelect()
    {
        // 전부 선택 해제
        foreach (ItemSlotData itemSlotData in battleInventory)
        {
            if (!itemSlotData.itemSlotUI)
                continue;
            itemSlotData.itemSlotUI.Select = false;
        }
        foreach (ItemSlotData itemSlotData in collectInventory)
        {
            if (!itemSlotData.itemSlotUI)
                continue;
            itemSlotData.itemSlotUI.Select = false;
        }
    }

    public void ClickFunctionButton0()
    {
        if (AdventureModeManager.Instance.Stat != AdventureGameModeStat.adventure)
        {
            Debug.LogWarning("전투중에는 아이템을 선택 할 수 없습니다.");
            return;
        }
        UpgradeMode = false;
        CloseUpgradeItemPanel();
        SelectMode = !SelectMode;
        if (SelectMode)
            moveItemButton.interactable = true;
        else
            moveItemButton.interactable = false;
    }

    public void ClickFunctionButton1()
    {
        if (AdventureModeManager.Instance.Stat != AdventureGameModeStat.adventure)
        {
            Debug.LogWarning("전투중에는 아이템을 등록/미등록 할 수 없습니다.");
            return;
        }        
            
        if (invetoryCategory == InventoryCategory.battle)
        {
            
            // 추가 삭제할 아이템 선택
            List<ItemSlotData> tempItemSlotData = new List<ItemSlotData>();
            foreach (ItemSlotData itemSlotData in battleInventory)
            {
                if (itemSlotData.itemSlotUI.Select)
                {
                    tempItemSlotData.Add(itemSlotData);
                }
            }
         
            // 아이템 옮기기
            foreach (ItemSlotData itemSlotData in tempItemSlotData)
            {
                battleInventory.Remove(itemSlotData);
                collectInventory.Add(itemSlotData);
            }
        }
        else if (invetoryCategory == InventoryCategory.collection)
        {
            // 추가 삭제할 아이템 선택
            List<ItemSlotData> tempItemSlotData = new List<ItemSlotData>();
            foreach (ItemSlotData itemSlotData in collectInventory)
            {
                if (itemSlotData.itemSlotUI.Select)
                {
                    tempItemSlotData.Add(itemSlotData);
                }
            }

            // 옮긴후 아이템 갯수 세어보기
            int count = battleInventory.Count + tempItemSlotData.Count;
            if (count > maxBattleInventorySlotCount)
            {
                Debug.LogWarning("빈 슬롯이 없어 이동할 수 없습니다.");
                return;
            }

            // 아이템 옮기기
            foreach (ItemSlotData itemSlotData in tempItemSlotData)
            {
                collectInventory.Remove(itemSlotData);
                battleInventory.Add(itemSlotData);                
            }
        }
        RefreshInventory();
    }

    public void ClickFunctionButton2()
    {
        
        if (AdventureModeManager.Instance.Stat != AdventureGameModeStat.adventure)
            return;
        SelectMode = false;
        if (!UpgradeMode)
        {
            Debug.Log("아이템 강화창 열기");
            OpenUpgradeItemPanel();            
            moveItemButton.interactable = false;
        }
        else
        {
            Debug.Log("아이템 강화창 닫기");
            CloseUpgradeItemPanel();            
            moveItemButton.interactable = true;
        }        
    }

    public void ClickFunctionButton3()
    {
        // 깊은 복사
        List<ItemSlotData> inventory = new List<ItemSlotData>();       

        // 업그레이드에 있는 재료 아이템 배열 가져오기
        foreach(ItemSlotData _itemSlotData in upgradeItemPanel.materialInventory)
        {
            inventory.Add(_itemSlotData);
        }
        inventory = inventory.OrderByDescending(_itemSlotData => _itemSlotData.index).ToList(); // 인덱스 역순으로 정렬


        // 강화 재료 아이템 모두 없애기
        int deltaLevel = 0;
        foreach (ItemSlotData _itemSlotData in inventory)
        {
            if (!_itemSlotData.itemSlotUI.Select)
                continue;
            if (_itemSlotData.fromSlotType == SlotType.battleSlot)
            {                                
                battleInventory.Remove(_itemSlotData);
                upgradeItemPanel.materialInventory.Remove(_itemSlotData);
                deltaLevel += 1;
            }
            else if (_itemSlotData.fromSlotType == SlotType.collectSlot)
            {
                collectInventory.Remove(_itemSlotData);
                upgradeItemPanel.materialInventory.Remove(_itemSlotData);
                deltaLevel += 1;
            }            
        }

        // 업글레이드 아이템 업글레이드
        ItemSlotData itemSlotData = upgradeItemPanel.upgradeItemSlotUI.ItemSlotData;
        if (itemSlotData.fromSlotType == SlotType.battleSlot)
        {
            int index = battleInventory.FindIndex(_itemSlotData => _itemSlotData == itemSlotData);
            battleInventory[index].Level += deltaLevel;
            upgradeItemPanel.UpgradeItemSlotData = battleInventory[index];
        }
        else if(itemSlotData.fromSlotType == SlotType.collectSlot)
        {
            int index = collectInventory.FindIndex(_itemSlotData => _itemSlotData == itemSlotData);
            collectInventory[index].Level += deltaLevel;
            upgradeItemPanel.UpgradeItemSlotData = collectInventory[index];
        }        

        // 정렬
        RefreshInventory();
        upgradeItemPanel.FindMaterialItemSlot();        
    }

    public void OpenUpgradeItemPanel()
    {
        upgradeItemPanel.gameObject.SetActive(true);        
        UpgradeMode = true;
    }

    public void CloseUpgradeItemPanel()
    {        
        upgradeItemPanel.Close();        
        UpgradeMode = false;
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
    public void AddBattleInventory(ItemSlotData itemSlotData)
    {
        if(battleInventory.Count < 12)
        {
            battleInventory.Add(itemSlotData);
        }
        else
        {
            Debug.LogWarning("아이템 초과해서 추가할수없습니다.");
        }
    }

    /// ------------------------------------------------------------- 기타 함수 ------------------------------------------------------------- ///
    Unit SpawnUnit(GameObject _unitObject, Vector2 point)
    {
        // 유닛 데이터 가져오기
        GameObject unitObject = _unitObject;
        if (unitObject == null)
        {
            Debug.LogWarning("소환할 유닛 오브젝트가 없습니다");
            return null;
        }
        
        // 유닛 소환할 좌표 가져오기
        point = new Vector2(Mathf.Floor(point.x) + 0.5f, Mathf.Floor(point.y) + 0.5f);

        // 유닛 소환하기
        Unit unit = Instantiate(unitObject, point, Quaternion.identity).GetComponent<Unit>();
        unit.team = team;        
        unit.gameObject.layer = LayerMask.NameToLayer("BattleUnitPlanPhase");

        // 유닛 소환 후 처리        
        battleInventory[dragSlotUI.ItemSlotData.index].SpawnUnit = unit;
        AdventureModeManager.Instance.SaveSpawnData();

        return unit;
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
