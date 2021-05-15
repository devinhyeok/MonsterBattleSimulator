using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff : MonoBehaviour
{
    public Sprite thumbnail;
    public BuffType buffType;
    public GameObject parentGameObject;
    Coroutine timer;
    public float maxSecond = 0;
    public float currentSecond = 0;

    // 버프 설정
    public void SetSecond(float second)
    {
        // 들어온 시간이 더긴가?
        if (second > currentSecond)
        {
            // 첫 실행인가?
            if (currentSecond <= 0)
            {
                PlayAnim();
            }

            // 시간 갱신하기
            maxSecond = second;
            currentSecond = maxSecond;

            if (timer == null)
            {
                timer = StartCoroutine(StartCountTime());
            }
            else
            {
                StopCoroutine(timer);
                timer = StartCoroutine(StartCountTime());
            }
        }
        // 버프 해제인가?
        else
        {
            if (timer != null)
            {
                StopCoroutine(timer);
            }
            currentSecond = 0f;
            StopAnim();
        }
    }

    IEnumerator StartCountTime()
    {
        while (currentSecond > 0)
        {
            yield return new WaitForFixedUpdate();
            currentSecond -= Time.fixedDeltaTime;
        }
        currentSecond = 0f;
        StopAnim();
    }

    void PlayAnim()
    {
        // 애니메이션이 있으면 애니메이션 설정
        gameObject.SetActive(true);
        if (gameObject.GetComponent<Animator>().ContainsParam("Active"))
        {
            gameObject.GetComponent<Animator>().SetBool("Active", true);
        }
    }
    void StopAnim()
    {
        // 애니메이션이 있으면 애니메이션 설정
        if (gameObject.GetComponent<Animator>().ContainsParam("Active"))
        {
            gameObject.GetComponent<Animator>().SetBool("Active", false);
        }
        else
        {
            gameObject.SetActive(false);
        }

        // 버프 시간이 종료되었다 알림
        parentGameObject.GetComponent<Unit>().ReleaseBuff(buffType);
    }
}

