using System;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
[Serializable]
public struct MovementData : IComponentData
{
    public float speed;
    public float3 direction;
}
