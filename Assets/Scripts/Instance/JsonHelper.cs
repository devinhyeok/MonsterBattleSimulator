using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

// Json을 Wrapper 클라스 통해 변환하여 배열 사용
public static class JsonHelper
{
    // Wrapper 생성자 선언
    [System.Serializable]
    private class Wrapper<T>
    {
        public List<T> list; // 들어온 클래스 배열 생성
    }

    // 변환후 저장하기: Wrapper<T> => T[]
    public static List<T> FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.list;
    }

    //​ 변환후 로드하기: T[] => Wrapper<T>
    public static string ToJson<T>(List<T> array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.list = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(List<T> array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.list = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }
}
