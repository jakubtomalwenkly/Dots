using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct SpawnerData : IComponentData
{
    public Entity prefab;
    public float secondsBetweenSpawns;
    public float secondsToNextSpawn;
    public float3 spawnPosition;
}
