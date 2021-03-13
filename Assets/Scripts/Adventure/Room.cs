using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Room : MonoBehaviour
{
    Transform[] SpawnPoints = new Transform[4];
    LevelGenerator levelGenerator;
    public string key;
    public bool[] door = new bool[4]; // 상하좌우 입구 여부
    public GameObject[] way =  new GameObject[4];
    public GameObject roomEvent;

    private void Awake()
    {
        if (transform.Find("Grid").gameObject.transform.Find("UpWay"))
        {
            way[0] = transform.Find("Grid").gameObject.transform.Find("UpWay").gameObject;
            way[0].SetActive(false);
        }
            
        if (transform.Find("Grid").gameObject.transform.Find("DownWay"))
        {
            way[1] = transform.Find("Grid").gameObject.transform.Find("DownWay").gameObject;
            way[1].SetActive(false);
        }
            
        if (transform.Find("Grid").gameObject.transform.Find("LeftWay"))
        {
            way[2] = transform.Find("Grid").gameObject.transform.Find("LeftWay").gameObject;
            way[2].SetActive(false);
        }
            
        if (transform.Find("Grid").gameObject.transform.Find("RightWay"))
        {
            way[3] = transform.Find("Grid").gameObject.transform.Find("RightWay").gameObject;
            way[3].SetActive(false);
        }                                            
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (transform.Find("SpawnPoint_Top") != null)
            Debug.DrawRay(transform.position, Vector2.up * 10f, Color.green);
        if (transform.Find("SpawnPoint_Bottom") != null)
            Debug.DrawRay(transform.position, Vector2.down * 10f, Color.green);
        if (transform.Find("SpawnPoint_Left") != null)
            Debug.DrawRay(transform.position, Vector2.left * 20f, Color.green);
        if (transform.Find("SpawnPoint_Right") != null)
            Debug.DrawRay(transform.position, Vector2.right * 20f, Color.green);
    }

    public void PlayCreateRoom()
    {
        StartCoroutine(CreateRoom());
    }

    private IEnumerator CreateRoom()
    {
        yield return new WaitForEndOfFrame();
        levelGenerator = GameObject.Find("LevelGenerator").GetComponent<LevelGenerator>();
        levelGenerator.RetriggerTimer();
        int layerMask = 1 << LayerMask.NameToLayer("Room");
        bool[] isCollsion = new bool[] { false, false , false , false }; // 상하좌우 입구 여부
        GameObject[] collsion = new GameObject[4];
        RaycastHit2D[] hit;

        // 상
        hit = Physics2D.RaycastAll(transform.position, Vector2.up, 10f, layerMask);
        for (int i = 0; i < hit.Length; ++i)
        {
            if (hit[i].collider.gameObject == gameObject)
                continue;
            isCollsion[0] = true;
            collsion[0] = hit[i].collider.gameObject;
        }

        // 하
        hit = Physics2D.RaycastAll(transform.position, Vector2.down, 10f, layerMask);
        for (int i = 0; i < hit.Length; ++i)
        {
            if (hit[i].collider.gameObject == gameObject)
                continue;
            isCollsion[1] = true;
            collsion[1] = hit[i].collider.gameObject;
        }
        
        // 좌
        hit = Physics2D.RaycastAll(transform.position, Vector2.left, 20f, layerMask);
        for (int i = 0; i < hit.Length; ++i)
        {
            if (hit[i].collider.gameObject == gameObject)
                continue;
            isCollsion[2] = true;
            collsion[2] = hit[i].collider.gameObject;
        }
        
        // 우
        hit = Physics2D.RaycastAll(transform.position, Vector2.right, 20f, layerMask);
        for (int i = 0; i < hit.Length; ++i)
        {
            if (hit[i].collider.gameObject == gameObject)
                continue;
            isCollsion[3] = true;
            collsion[3] = hit[i].collider.gameObject;
        }

        // 상단에 방을 추가해야하는가?
        if (transform.Find("SpawnPoint_Top") != null)
        {
            // 상단에 생성 가능한가?
            if (!isCollsion[0])
            {
                SpawnPoints[0] = transform.Find("SpawnPoint_Top").gameObject.transform;
                int random = Random.Range(0, levelGenerator.levelData.downRandomPool.Count - 1);
                GameObject tempObject = Instantiate(levelGenerator.levelData.downRandomPool[random], SpawnPoints[0].position, SpawnPoints[0].rotation);
                tempObject.gameObject.GetComponent<Room>().PlayCreateRoom();
            }
            // 방이 있는데 반대편 입구가 막혀있는가?
            else if(!collsion[0].GetComponent<Room>().door[1])
            {
                door[0] = false;
                ModifyRoom();
            }
        }

        // 하단에 방을 추가해야하는가?
        if (transform.Find("SpawnPoint_Bottom") != null)
        {
            // 하단에 생성 가능한가?
            if (!isCollsion[1])
            {
                SpawnPoints[1] = transform.Find("SpawnPoint_Bottom").gameObject.transform;
                int random = Random.Range(0, levelGenerator.levelData.upRandomPool.Count - 1);
                GameObject tempObject = Instantiate(levelGenerator.levelData.upRandomPool[random], SpawnPoints[1].position, SpawnPoints[1].rotation);
                tempObject.gameObject.GetComponent<Room>().PlayCreateRoom();
            }
            else if (!collsion[1].GetComponent<Room>().door[0])
            {
                door[1] = false;
                ModifyRoom();
            }
        }

        // 좌단에 방을 추가해야하는가?
        if (transform.Find("SpawnPoint_Left") != null)
        {
            // 좌단에 생성 가능한가?
            if (!isCollsion[2])
            {
                SpawnPoints[2] = transform.Find("SpawnPoint_Left").gameObject.transform;
                int random = Random.Range(0, levelGenerator.levelData.rightRandomPool.Count - 1);
                GameObject tempObject = Instantiate(levelGenerator.levelData.rightRandomPool[random], SpawnPoints[2].position, SpawnPoints[2].rotation);
                tempObject.gameObject.GetComponent<Room>().PlayCreateRoom();
            }
            else if (!collsion[2].GetComponent<Room>().door[3])
            {
                door[2] = false;
                ModifyRoom();
            }
        }

        // 상단에 방을 추가해야하는가?
        if (transform.Find("SpawnPoint_Right") != null)
        {
            // 우단에 생성 가능한가?
            if (!isCollsion[3])
            {
                SpawnPoints[3] = transform.Find("SpawnPoint_Right").gameObject.transform;
                int random = Random.Range(0, levelGenerator.levelData.leftRandomPool.Count - 1);
                GameObject tempObject = Instantiate(levelGenerator.levelData.leftRandomPool[random], SpawnPoints[3].position, SpawnPoints[3].rotation);
                tempObject.gameObject.GetComponent<Room>().PlayCreateRoom();
            }
            else if (!collsion[3].GetComponent<Room>().door[2])
            {
                door[3] = false;
                ModifyRoom();
            }
        }        
    }

    // 방 모양 수정
    void ModifyRoom()
    {
        GameObject room = null;
        // 통로 1개 짜리
        if (door[0] && !door[1] && !door[2] && !door[3])
        {
            room = levelGenerator.levelData.up;
        }
        else if (!door[0] && door[1] && !door[2] && !door[3])
        {
            room = levelGenerator.levelData.down;
        }
        else if (!door[0] && !door[1] && door[2] && !door[3])
        {
            room = levelGenerator.levelData.left;
        }
        else if (!door[0] && !door[1] && !door[2] && door[3])
        {
            room = levelGenerator.levelData.right;
        }

        // 통로 2개 짜리
        else if (door[0] && door[1] && !door[2] && !door[3])
        {
            room = levelGenerator.levelData.upDown;
        }
        else if (!door[0] && door[1] && door[2] && !door[3])
        {
            room = levelGenerator.levelData.downLeft;
        }
        else if (!door[0] && !door[1] && door[2] && door[3])
        {
            room = levelGenerator.levelData.leftRight;
        }
        else if (!door[0] && door[1] && !door[2] && door[3])
        {
            room = levelGenerator.levelData.downRight;
        }
        else if (door[0] && !door[1] && door[2] && !door[3])
        {
            room = levelGenerator.levelData.upLeft;
        }
        else if (door[0] && !door[1] && !door[2] && door[3])
        {
            room = levelGenerator.levelData.upRight;
        }

        // 통로 3개 짜리
        else if (!door[0] && door[1] && door[2] && door[3])
        {
            room = levelGenerator.levelData.downLeftRight;
        }
        else if (door[0] && !door[1] && door[2] && door[3])
        {
            room = levelGenerator.levelData.upLeftRight;
        }
        else if (door[0] && door[1] && !door[2] && door[3])
        {
            room = levelGenerator.levelData.upDownRight;
        }
        else if (door[0] && door[1] && door[2] && !door[3])
        {
            room = levelGenerator.levelData.upDownLeft;
        }

        // 통로 4개 짜리
        else if (door[0] && door[1] && door[2] && door[3])
        {
            room = levelGenerator.levelData.upDownLeftRight;
        }

        if (room != null)
        {
            Instantiate(room, transform.position, transform.rotation);
            Destroy(this.gameObject);
        }
    }
}