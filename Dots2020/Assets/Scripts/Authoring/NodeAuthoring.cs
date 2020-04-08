using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class NodeAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    [SerializeField] private GameObject nextNode;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new DestinationData
        {
            destination = conversionSystem.GetPrimaryEntity(nextNode)
        });
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(nextNode);
    }
}
