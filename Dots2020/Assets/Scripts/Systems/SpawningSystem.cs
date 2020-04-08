using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Physics;
using Unity.Mathematics;
using UnityEngine;

#if true 

[BurstCompile(CompileSynchronously = true)]
public class SpawningSystem : JobComponentSystem
{
    private BeginInitializationEntityCommandBufferSystem beginInitializationEntityCommandBuffer;

    protected override void OnCreate()
    {
        beginInitializationEntityCommandBuffer = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    private struct SpawnJob : IJobForEachWithEntity<SpawnerData, LocalToWorld>
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        [ReadOnly] public ComponentDataFromEntity<DestinationData> destinations;
        [ReadOnly] public ComponentDataFromEntity<InputData> inputs;

        public float deltaTime;


        public void Execute(Entity entity, int index, ref SpawnerData spawnerData, [ReadOnly]ref LocalToWorld localToWorld)
        {
            spawnerData.secondsToNextSpawn -= deltaTime;
            if (spawnerData.secondsToNextSpawn <= 0)
            {
                if (destinations.Exists(entity))
                {
                    Entity instance = InstansiateEntity(index,ref spawnerData, ref localToWorld);
                    entityCommandBuffer.SetComponent(index, instance, destinations[entity]);
                }

                if (inputs.Exists(entity) && inputs[entity].IsFiring)
                {
                    Entity instance = InstansiateEntity(index, ref spawnerData, ref localToWorld);
                    entityCommandBuffer.AddComponent(index, instance, new ChangeDirectionData
                    {

                        newDirection = localToWorld.Forward
                    }) ;
                }
            }
        }

        private Entity InstansiateEntity(int index, ref SpawnerData spawnerData, ref LocalToWorld localToWorld)
        {
            spawnerData.secondsToNextSpawn = spawnerData.secondsBetweenSpawns;
            Entity instance = entityCommandBuffer.Instantiate(index, spawnerData.prefab);
            entityCommandBuffer.SetComponent(index, instance, new Translation
            {
                Value = localToWorld.Position
            });
            return instance;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        SpawnJob spawningJob = new SpawnJob()
        {
            entityCommandBuffer = beginInitializationEntityCommandBuffer.CreateCommandBuffer().ToConcurrent(),
            destinations = GetComponentDataFromEntity<DestinationData>(true),
            inputs = GetComponentDataFromEntity<InputData>(true),
            deltaTime = Time.DeltaTime
        };
        JobHandle jobHandle = spawningJob.Schedule(this, inputDeps);
        beginInitializationEntityCommandBuffer.AddJobHandleForProducer(jobHandle);
        return jobHandle;

    }
}
#endif

#if false 
public class SpawningSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var delta = UnityEngine.Time.deltaTime;
        Entities.ForEach((ref SpawnerData spawnerData, ref LocalToWorld localToWorld) =>
        {
            spawnerData.secondsToNextSpawn -= delta;
            if (spawnerData.secondsToNextSpawn <= 0)
            {
                spawnerData.secondsToNextSpawn += spawnerData.secondsBetweenSpawns;
                var prefabs = EntityManager.GetBuffer<SpawnerListComponent>(spawnerData.prefab);
                var instance = EntityManager.Instantiate(prefabs[0].Prefab);
                EntityManager.SetComponentData(instance, new Translation
                {
                    Value = localToWorld.Position
                });
            }
        });
    }
}
#endif
