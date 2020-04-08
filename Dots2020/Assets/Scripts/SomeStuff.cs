using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public static class SomeStuff
{
    public static float CalculateDistance(float3 from, float3 to)
    {
        float lengh;
        float3 direction = float3.zero;
        direction.x = to.x - from.x;
        direction.y = to.y - from.y;
        direction.z = to.z - from.z;
        lengh = Mathf.Sqrt(Mathf.Pow(direction.x, 2) + Mathf.Pow(direction.y, 2) + Mathf.Pow(direction.z, 2));
        return lengh;
    }

    public static float3 CalculateDirection(float3 from, float3 to)
    {
        float lengh;
        float3 direction = float3.zero;
        direction.x = to.x - from.x;
        direction.y = to.y - from.y;
        direction.z = to.z - from.z;
        lengh = Mathf.Sqrt(Mathf.Pow(direction.x, 2) + Mathf.Pow(direction.y, 2) + Mathf.Pow(direction.z, 2));
        return direction / lengh;
    }

    public static bool bool3Tobool(bool3 mybool3)
    {
        return mybool3.x && mybool3.y && mybool3.z;
    }


}