using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleModeManager : MonoBehaviour
{
    // 인스턴스화
    private static TitleModeManager instance;
    public static TitleModeManager Instance
    {
        get
        {
            if (null == instance)
            {
                instance = FindObjectOfType(typeof(TitleModeManager)) as TitleModeManager;
                if (instance == null)
                {
                    //Debug.Log("모험 모드 매니저가 없습니다.");                    
                }
            }
            return instance;
        }
    }

    public void NewGame()
    {
        Debug.Log("새 게임");
        SceneManager.LoadScene("Adventure");
    }

    public void Continue()
    {
        Debug.Log("이어하기");
    }

    public void LoadSave()
    {
        Debug.Log("불러오기");
    }

    public void Settings()
    {
        Debug.Log("옵션");
    }

    public void End()
    {
        Debug.Log("종료하기");
        Application.Quit();
    }
}
