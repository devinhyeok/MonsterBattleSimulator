using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEventListener : MonoBehaviour
{
    [SerializeField] UnityEvent[] events;

    public void EventCall(int index)
    {
        if (index >= 0 && index < events.Length)
        {
            events[index].Invoke();
        }
        else
        {
            Debug.LogError("찾을 수 없는 애니메이션 이벤트입니다.");
        }
    }
}
