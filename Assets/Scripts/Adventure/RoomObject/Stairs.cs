using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stairs : MonoBehaviour, IClickObject
{
    public LevelData levelData;

    public void Click()
    {
        // 다음맵 재생성
        LevelGenerator levelGenerators = FindObjectsOfType<LevelGenerator>()[0];
        levelGenerators.levelData = levelData;
        levelGenerators.Generation();

        // 카메라 위치 초기화
        AdventurePlayerController playerController = AdventureModeManager.Instance.playerController;
        playerController.playerCamera.transform.position = new Vector3(0, 0, -10);
    }
}
