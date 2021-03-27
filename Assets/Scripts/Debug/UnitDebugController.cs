using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDebugController : MonoBehaviour
{
    public UnitInfoUI unitInfoUI;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckClickUnit();
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
                unitInfoUI.unit = null;
                unitInfoUI.gameObject.SetActive(false);
            }
            else if (hit.collider.gameObject.GetComponent<Unit>() == null)
            {
                unitInfoUI.unit = null;
                unitInfoUI.gameObject.SetActive(false);
            }
            else if (hit.collider.gameObject.GetComponent<Unit>() != null)
            {
                Debug.Log(hit.collider.gameObject);
                unitInfoUI.unit = hit.collider.gameObject.GetComponent<Unit>();
                unitInfoUI.gameObject.SetActive(true);
            }
        }
    }
}
