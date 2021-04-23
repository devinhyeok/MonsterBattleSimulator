using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public LevelData levelData;

    [Header("읽기용")]
    public List<GameObject> roomList;
    public List<GameObject> eventList;

    [HideInInspector]
    public float endSecond;

    private void Start()
    {
        if (levelData.firstRoom != null)
        {
            Generation();
        }
    }

    // 방 생성 시작
    public void Generation()
    {
        // 방 초기화
        roomList.Clear();
        roomList.AddRange(GameObject.FindGameObjectsWithTag("Room"));
        for(int i=0; i < roomList.Count; i++)        
            Destroy(roomList[i]);
        
        // 이벤트 초기화
        eventList.Clear();
        eventList.AddRange(GameObject.FindGameObjectsWithTag("Event"));
        for (int i = 0; i < eventList.Count; i++)
            Destroy(eventList[i]);
        

        // 생성 시작
        GameObject tempObject = Instantiate(levelData.firstRoom, transform.position, transform.rotation);
        tempObject.GetComponent<Room>().PlayCreateRoom();
        tempObject.GetComponent<Room>().key = "First";
        AdventureModeManager.Instance.playerController.currentRoom = tempObject;
        AdventureModeManager.Instance.playerController.currentRoom = tempObject;
        StartCoroutine(PlayEndTimer());
    }

    // 생성이 멈췄는지 검사
    IEnumerator PlayEndTimer()
    {
        RetriggerTimer();
        while (endSecond > 0)
        {
            yield return new WaitForEndOfFrame();
            endSecond -= Time.deltaTime;
        }
        CheckFitLevel();
    }
    public void RetriggerTimer()
    {
        endSecond = 0.2f;
    }    

    // 레벨이 조건에 맞게 생성됬는지 검사
    void CheckFitLevel ()
    {
        // 방 갯수 업데이트
        roomList.Clear();
        roomList.AddRange(GameObject.FindGameObjectsWithTag("Room"));

        // 조건에 만족하는지 검사
        if (roomList.Count < levelData.minRoomCount || levelData.maxRoomCount < roomList.Count)
        {
            //Debug.Log("재생성: " + roomList.Count + "개");
            Generation();
        }
        else
        {
            //Debug.Log("만족: " + roomList.Count + "개");
            EndGeneration();
            AdventureModeManager.Instance.Stat = AdventureGameModeStat.adventure;
            AdventureModeManager.Instance.roomList = roomList;
        }
    }

    // 생성 후처리
    void EndGeneration()
    {
        // 이벤트 생성
        roomList[roomList.Count - 1].GetComponent<Room>().key = "Last";
        foreach(GameObject room in roomList)
        {
            // 시작방 이벤트, 마지막방 이벤트 생성
            string key = room.GetComponent<Room>().key;
            if (key == "First")
            {
                room.GetComponent<Room>().roomEvent = Instantiate(levelData.firstEvent, room.transform.position, room.transform.rotation);
                continue;
            }            
            if (key == "Last")
            {
                room.GetComponent<Room>().roomEvent = Instantiate(levelData.lastEvent, room.transform.position, room.transform.rotation);
                continue;
            }

            // 전투, 보상, 상점 이벤트 생성
            float _battlePercent = levelData.battlePercent;
            float _rewardPercent = _battlePercent + levelData.rewardPercent;
            float _shopPercent = _rewardPercent + levelData.shopPercent;
            float random = Random.value;
            if (random <= _battlePercent)
            {
                int index = Random.Range(0, levelData.battleEventPool.Count);
                if (levelData.battleEventPool[index])
                {
                    GameObject _gameObject = Instantiate(levelData.battleEventPool[index], room.transform.position, room.transform.rotation);
                    room.GetComponent<Room>().key = _gameObject.name;
                    room.GetComponent<Room>().roomEvent = _gameObject;
                }
                else
                {
                    Debug.LogError("battleEventPool이 비어 있습니다");
                }                               
            }
            else if (random <= _rewardPercent)
            {
                int index = Random.Range(0, levelData.rewardEventPool.Count);
                if (levelData.rewardEventPool[index])
                {
                    GameObject _gameObject = Instantiate(levelData.rewardEventPool[index], room.transform.position, room.transform.rotation);
                    room.GetComponent<Room>().key = _gameObject.name;
                    room.GetComponent<Room>().roomEvent = _gameObject;
                }
                else
                {
                    Debug.LogError("rewardEventPool이 비어 있습니다");
                }
            }
            else if (random <= _shopPercent)
            {
                int index = Random.Range(0, levelData.shopEventPool.Count);
                if (levelData.shopEventPool[index])
                {
                    GameObject _gameObject = Instantiate(levelData.shopEventPool[index], room.transform.position, room.transform.rotation);
                    room.GetComponent<Room>().key = _gameObject.name;
                    room.GetComponent<Room>().roomEvent = _gameObject;
                }
                else
                {
                    Debug.LogError("shopEventPool이 비어 있습니다");
                }
            }

            // 특수 이벤트 덮어쓰기
        }

        // 이벤트 정보 업데이트
        eventList.Clear();
        eventList.AddRange(GameObject.FindGameObjectsWithTag("Event"));
    }
}