using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New LevelData", menuName = "Data/New LevelData")]
public class LevelData : ScriptableObject
{
    public string key;

    [Header("참조값")]
    public GameObject down;
    public GameObject downLeft;
    public GameObject downLeftRight;
    public GameObject downRight;
    public GameObject left;
    public GameObject leftRight;
    public GameObject right;
    public GameObject up;
    public GameObject upDown;
    public GameObject upDownLeft;
    public GameObject upDownLeftRight;
    public GameObject upDownRight;
    public GameObject upLeft;
    public GameObject upLeftRight;
    public GameObject upRight;

    [Header("방생성 편집")]
    public int minRoomCount;
    public int maxRoomCount;
    public GameObject firstRoom;
    public List<GameObject> upRandomPool;
    public List<GameObject> downRandomPool;
    public List<GameObject> rightRandomPool;
    public List<GameObject> leftRandomPool;

    [Header("이벤트생성 편집")]
    public float battlePercent;
    public float rewardPercent;
    public float shopPercent;
    public GameObject firstEvent;
    public GameObject lastEvent;
    public List<GameObject> battleEventPool;
    public List<GameObject> rewardEventPool;
    public List<GameObject> shopEventPool;
}
