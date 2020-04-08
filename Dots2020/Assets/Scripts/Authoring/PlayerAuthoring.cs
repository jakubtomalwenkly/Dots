using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Physics.Authoring;
using Unity.Transforms;

public class PlayerAuthoring : MonoBehaviour , IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    [SerializeField]private InputData inputData;
    [SerializeField]private MovementData movement;
    [SerializeField]private SpawnerData gun;
    [SerializeField]private Transform gunPosition;
    [SerializeField] private GameObject bulletPrefab; 
    private Entity myEntity;
    private Translation entityTranslation;
    private EntityManager entityManager;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        gun.spawnPosition = gunPosition.position;
        gun.prefab = conversionSystem.GetPrimaryEntity(bulletPrefab);

        myEntity = entity;
        dstManager.AddComponentData(entity, inputData);
        dstManager.AddComponentData(entity, movement);
        dstManager.AddComponentData(entity, gun);
        Destroy(gameObject.GetComponent<MeshFilter>());
        Destroy(gameObject.GetComponent<MeshRenderer>());
        Destroy(gameObject.GetComponent<PhysicsShapeAuthoring>());
        Destroy(gameObject.GetComponent<PhysicsBodyAuthoring>());
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(bulletPrefab);
    }

    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

void Update()
    {
        entityTranslation = entityManager.GetComponentData<Translation>(myEntity);
        transform.position = entityTranslation.Value;
    }
}
