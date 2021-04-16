using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTester : MonoBehaviour
{
    private void Start()
    {
        Unit[] units = Object.FindObjectsOfType<Unit>();

        foreach (Unit unit in units)
        {
            Debug.Log(unit.gameObject.name);
        }
    }
}
