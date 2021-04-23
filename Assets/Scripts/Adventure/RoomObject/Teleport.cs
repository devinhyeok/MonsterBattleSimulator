using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Teleport : MonoBehaviour, IClickObject
{
    public void Click()
    {
        SceneManager.LoadScene("Ending");
    }
}
