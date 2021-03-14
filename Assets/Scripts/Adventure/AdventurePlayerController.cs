using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class AdventurePlayerController : MonoBehaviour
{
    [Header("편집용")]
    public int team;
    public float cameraMoveSpeed;
    public Color colorSelected;
    public Color colorUnselected;

    [Header("참조용")]
    public Canvas canvas;
    public Camera playerCamera;    
    public Text hpText;
    public Text goldText;
    public Image unitFilter;
    public Image equipFilter;
    public Image battleFilter;
    public GameObject content;
    public GameObject itemSlotUI;
    public UnitInfoUI unitInfoUI;
    public DragSlotUI dragSlotUI;


    [Header("읽기용")]    
    public Filter filter;
    public int maxHp;
    private int currentHp;
    public int CurrentHp
    {
        get { return currentHp; } 
        set 
        { 
            currentHp = Mathf.Clamp(value, 0, maxHp);
            RefreshPlayerUI();
        }
    }
    private int gold = 20;
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
    public Unit draggingUnit; // 드래깅중인 유닛

    // 인벤토리
    public List<ItemSlotData> unitInventory = new List<ItemSlotData>();
    public List<ItemSlotData> equipInventory = new List<ItemSlotData>();
    public List<ItemSlotData> battleInventory = new List<ItemSlotData>();
    public List<ItemSlotData> Inventory
    {
        get
        {
            if (filter == Filter.unit)
            {
                return unitInventory;
            }
            if (filter == Filter.equip)
            {
                return equipInventory;
            }
            if (filter == Filter.battle)
            {
                return battleInventory;
            }
            else 
                return null;
        }
        set
        {
            if (filter == Filter.unit)
            {
                unitInventory = value;
            }
            if (filter == Filter.equip)
            {
                equipInventory = value;
            }
            if (filter == Filter.battle)
            {
                battleInventory = value;
            }
        }
    }

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

        CurrentHp = maxHp; // 시작시 피 전부 회복
    }

    private void Start()
    {
        // 시작 아이템 인벤토리 설정
        List<ItemData> startInventory = new List<ItemData>();        
        for (int i = 0; i < 6; i++)
        {
            startInventory.Add(ItemData.GetData("Antonus"));
            startInventory.Add(ItemData.GetData("EquipItem1"));
            startInventory.Add(ItemData.GetData("EquipItem2"));
            startInventory.Add(ItemData.GetData("BattleItem1"));
            startInventory.Add(ItemData.GetData("BattleItem2"));
        }        
        AddItems(startInventory);

        // 유닛 필터 선택한 채로 시작
        ClickUnitFilter();
    }

    private void Update()
    {
        // 마우스 입력 검사
        CheckClickUI();
        CheckClickGameObject();

        // 드랍 검사
        CheckDropItemToField();

        // 드래깅 슬롯 마우스 따라다니게 하기
        dragSlotUI.gameObject.transform.position = Input.mousePosition;

        // 드래깅 종료시 드래깅 슬롯 비우기
        if (Input.GetMouseButtonUp(0))
            dragSlotUI.ItemSlotData = null;

        // 카메라 입력 검사
        if (AdventureModeManager.Instance.stat == AdventureGameModeStat.adventure)
            CheckMoveCamera();
    }

    /// ------------------------------------------------------------- 마우스 조작 관련 ------------------------------------------------------------- ///
    // UI 클릭 여부 확인
    void CheckClickUI()
    {
        // 슬롯 드래깅 시작 여부
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
        }

        // 슬롯 드랍 여부
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
                if (raycastResult.gameObject.tag != "ItemSlot")
                    continue;
                ItemSlotData itemSlotData = raycastResult.gameObject.GetComponent<ItemSlotUI>().ItemSlotData;
                if (itemSlotData != null)
                    Debug.Log(string.Format("{0} {1}번 슬롯에 {2} 아이템 드랍", filter, itemSlotData.index, dragSlotUI.ItemSlotData.itemData.key));
            }
        }
    }

    // 게임 오브젝트를 클릭했는지 검사
    void CheckClickGameObject()
    {
        // 클릭시 UI에 유닛 정보 가져오기
        if (Input.GetMouseButtonDown(0))
        {
            // 마우스 밑에 유닛 검사
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

            if (!hit.collider)
                return;

            // 유닛을 클릭했는가?
            if (hit.collider.gameObject.GetComponent<Unit>() != null)
            {
                unitInfoUI.unit = hit.collider.gameObject.GetComponent<Unit>();
                unitInfoUI.gameObject.SetActive(true);
            }
            else
            {
                unitInfoUI.unit = null;
                unitInfoUI.gameObject.SetActive(false);
            }

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
                    Debug.Log(string.Format("아이템 구매{0}", displayGoods.ItemData));
                    Gold -= displayGoods.ItemData.buyGold;
                    AddItems(displayGoods.ItemData);
                    displayGoods.gameObject.SetActive(false);
                }
                else if (displayGoods.ItemData.buyGold > Gold)
                {
                    Debug.Log("아이템을 구매할 돈이 부족합니다.");
                }                    
            }

            // 보상 상자를 클릭했는가?
            if (hit.collider.gameObject.GetComponent<RewardEvent>())
            {
                RewardEvent rewardEvent = hit.collider.gameObject.GetComponent<RewardEvent>();
                AddItems(rewardEvent.itemDatas);
                rewardEvent.gameObject.SetActive(false);
            }
        }
    }

    // 필드에 아이템을 드랍했는지 체크
    void CheckDropItemToField()
    {
        if (Input.GetMouseButtonUp(0))
        {
            // 마우스 밑에 유닛 검사
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);
            if (hit.collider == null)
                return;
            if (hit.collider.tag != "Room")
                return;
            if (dragSlotUI.ItemSlotData == null)
                return;
            Room room = hit.collider.gameObject.GetComponent<Room>();
            Vector2 point = hit.point;
            Filter _filter = dragSlotUI.ItemSlotData.itemData.filter;
            AdventureGameModeStat modeStat = AdventureModeManager.Instance.stat;
            Debug.Log(string.Format("{0} 방의 {1} 좌표에 {2} 아이템 드랍", room, point, dragSlotUI.ItemSlotData.itemData.key));

            // 드랍된 아이템이 유닛 타입이고 배치 페이즈인가?
            if (dragSlotUI.ItemSlotData.itemData.filter == Filter.unit && modeStat == AdventureGameModeStat.battlePlanPhase)
            {
                // 해당 유닛이 소환된적이 없거나 HP가 0 초과인가?
                if (Inventory[dragSlotUI.ItemSlotData.index].SpawnUnit == null && Inventory[dragSlotUI.ItemSlotData.index].Health > 0)
                {
                    SpawnUnit(dragSlotUI.ItemSlotData, point);
                }
                else
                {
                    Debug.Log(string.Format("{0}번째 슬롯에서 소환된 유닛: {1}", dragSlotUI.ItemSlotData.index, Inventory[dragSlotUI.ItemSlotData.index].SpawnUnit));
                }
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
        hpText.text = CurrentHp.ToString() + "/" + maxHp.ToString();
        goldText.text = gold.ToString();
    }

    // 인벤토리 UI 새로고침
    void RefreshInventory()
    {
        // 인벤토리 정보 가져오기
        List<ItemSlotData> inventory = new List<ItemSlotData>();
        Inventory = Inventory.OrderBy(_itemSlotData => _itemSlotData.itemData.key).ToList(); // 키순으로 정렬
        inventory = Inventory;

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
    public void ClickUnitFilter()
    {
        filter = Filter.unit;
        unitFilter.color = colorSelected;
        equipFilter.color = colorUnselected;
        battleFilter.color = colorUnselected;
        RefreshInventory();
    }

    // 장비 필터
    public void ClickEquipFilter()
    {
        filter = Filter.equip;
        unitFilter.color = colorUnselected;
        equipFilter.color = colorSelected;
        battleFilter.color = colorUnselected;
        RefreshInventory();
    }

    // 전투 필터
    public void ClickBattleFilter()
    {
        filter = Filter.battle;
        unitFilter.color = colorUnselected;
        equipFilter.color = colorUnselected;
        battleFilter.color = colorSelected;
        RefreshInventory();
    }

    /// ------------------------------------------------------------- 인벤토리 관련 ------------------------------------------------------------- ///
    public void AddItems(ItemData itemData)
    {
        if (itemData.filter == Filter.unit)
        {
            unitInventory.Add(new ItemSlotData(itemData));
        }
        else if (itemData.filter == Filter.equip)
        {
            equipInventory.Add(new ItemSlotData(itemData));
        }
        else if (itemData.filter == Filter.battle)
        {
            battleInventory.Add(new ItemSlotData(itemData));
        }
        RefreshInventory();
    }

    public void AddItems(List<ItemData> itemDatas)
    {
        foreach(ItemData itemData in itemDatas)
        {
            if (itemData.filter == Filter.unit)
            {
                unitInventory.Add(new ItemSlotData(itemData));
            }
            else if (itemData.filter == Filter.equip)
            {
                equipInventory.Add(new ItemSlotData(itemData));
            }
            else if(itemData.filter == Filter.battle)
            {
                battleInventory.Add(new ItemSlotData(itemData));
            }
        }
        RefreshInventory();
    }

    /// ------------------------------------------------------------- 기타 함수 ------------------------------------------------------------- ///
    void SpawnUnit(ItemSlotData itemSlotData, Vector2 point)
    {
        // 유닛 데이터 가져오기
        GameObject _gameObject = itemSlotData.itemData.key.GetUnitPrefab();
        if (_gameObject == null)
            return;

        // 유닛 소환할 좌표 가져오기
        point = new Vector2(Mathf.Floor(point.x) + 0.5f, Mathf.Floor(point.y) + 0.5f);

        // 유닛 소환하기
        Unit unit = Instantiate(_gameObject, point, Quaternion.identity).GetComponent<Unit>();
        unit.team = 0;

        // 유닛 소환 후 처리
        AdventureModeManager.Instance.unitsInBattle.Add(unit.gameObject);
        Inventory[dragSlotUI.ItemSlotData.index].SpawnUnit = unit;
        Debug.Log(string.Format("{0} 유닛 {1} 좌표에 소환", itemSlotData.itemData.key, point));
    }
}
