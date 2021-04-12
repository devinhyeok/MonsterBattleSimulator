using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardBox : MonoBehaviour, IClickObject, IMessage
{
    [Header("참조값")]
    public GameObject fieldItem;

    [Header("편집값")]
    public List<ItemData> itemDataRandomPool = new List<ItemData>();
    public int numberOfItems;

    List<ItemData> itemDatas = new List<ItemData>();
    List<GameObject> spawnObjects = new List<GameObject>();

    private void Awake()
    {
        if (numberOfItems <= 0)
            return;
        for (int i = 0; i < numberOfItems; i++)
        {
            int index = Random.Range(0, itemDataRandomPool.Count);
            itemDatas.Add(itemDataRandomPool[index]);
            itemDataRandomPool.RemoveAt(index);
        }
    }

    public void Click()
    {
        Vector3 spawnPostion = transform.position;
        spawnPostion -= new Vector3((itemDatas.Count - 1) / 2, 0, 0);
        foreach (ItemData itemData in itemDatas)
        {
            FieldItem _fieldItem = Instantiate(fieldItem, spawnPostion, Quaternion.identity).GetComponent<FieldItem>();
            _fieldItem.ItemData = itemData;
            _fieldItem.messageObject = gameObject;
            spawnObjects.Add(_fieldItem.gameObject);
            spawnPostion += new Vector3(1, 0, 0);
        }
        gameObject.SetActive(false);
    }

    public void Message()
    {
        foreach(GameObject spawnObject in spawnObjects)
        {
            spawnObject.SetActive(false);
        }
    }
}