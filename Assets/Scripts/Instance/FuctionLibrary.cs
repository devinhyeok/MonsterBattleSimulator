using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FuctionLibrary
{
    // 애니메이션에 해당 속성의 이름과 일치하는 것이 있는지 검사
    public static bool ContainsParam(this Animator anim, string paramName)
    {
        if (anim == null)
            return false;
        
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }

    // 해당 이름의 유닛 프리팹 가져오기
    public static GameObject GetUnitPrefab(this string filename)
    {
        // 첫 문장 대문자로 변경
        char[] tempString = filename.ToCharArray();
        tempString[0] = char.ToUpper(tempString[0]);
        filename = new string(tempString);

        // 오브젝트 반환
        return Resources.Load<GameObject>(string.Format("Prefabs/Unit/{0}", filename));
    }

    // 헤당 이름의 버프프리팹 가져오기 
    public static GameObject GetBuffPrefab(this string filename)
    {
        // 첫 문장 대문자로 변경
        char[] tempString = filename.ToCharArray();
        tempString[0] = char.ToUpper(tempString[0]);
        filename = new string(tempString);

        // 오브젝트 반환
        return Resources.Load<GameObject>(string.Format("Prefabs/Buff/{0}", filename));
    }

    // 복사한 파일 주소를 Resource.Load에 쓸 수 있도록 하기
    public static string FullPathToResouresPath(this string path)
    {
        path = path.Replace("Assets/Resources/", "");
        path = path.Split('.')[0];
        return path;
    }

    // 벡터 돌리기
    public static Vector2 Rotate(this Vector2 vector, float delta)
    {
        delta *= Mathf.Deg2Rad;
        return new Vector2(vector.x * Mathf.Cos(delta) - vector.y * Mathf.Sin(delta), vector.x * Mathf.Sin(delta) + vector.y * Mathf.Cos(delta));
    }
}
