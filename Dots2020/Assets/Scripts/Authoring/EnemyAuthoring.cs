using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;

public class EnemyAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] MovementData movementData;
    [SerializeField] HealthData healthData;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, movementData);
        dstManager.AddComponentData(entity, healthData);
        dstManager.AddBuffer<Damage>(entity);
        dstManager.AddComponent<DestinationData>(entity);
    }

}
