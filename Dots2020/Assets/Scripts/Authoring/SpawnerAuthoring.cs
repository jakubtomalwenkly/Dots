using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class SpawnerAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private GameObject firstNode;
    [SerializeField] private float timeBetweeneSpawns;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new SpawnerData
        {
            prefab = conversionSystem.GetPrimaryEntity(prefab),
            secondsBetweenSpawns = timeBetweeneSpawns,
            secondsToNextSpawn = timeBetweeneSpawns,
            spawnPosition = transform.position
        });
        dstManager.AddComponentData(entity, new DestinationData
        {
            destination = conversionSystem.GetPrimaryEntity(firstNode)
        }) ;
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(prefab);
    }
}
