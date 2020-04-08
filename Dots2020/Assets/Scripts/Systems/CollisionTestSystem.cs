using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Collections;

using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class CollisionTestSystem : JobComponentSystem
{
    private EntityQuery enemyGroup;
    private EntityQuery barrierGroup;
    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;
    private CollisionWorld world;
    private NativeArray<int> triggerEntitiesIndex;
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBuffer;

    protected override void OnCreate()
    {
        enemyGroup = GetEntityQuery(ComponentType.ReadOnly<MovementData>(), ComponentType.ReadOnly<HealthData>());
        barrierGroup = GetEntityQuery(ComponentType.ReadOnly<BarrierTag>());
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        endSimulationEntityCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        triggerEntitiesIndex = new NativeArray<int>(1, Allocator.Persistent);
        triggerEntitiesIndex[0] = 0;
    }

    private struct GetTriggerEventCount : ITriggerEventsJob
    {
        [NativeFixedLength(1)] public NativeArray<int> pCounter;
        [DeallocateOnJobCompletion]public NativeArray<Entity> enemyEntitiesGroup;
        [DeallocateOnJobCompletion]public NativeArray<Entity> barrierEntitiesGroup;
        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.Entities.EntityA;
            Entity entityB = triggerEvent.Entities.EntityB;

            bool isAEnemy = enemyEntitiesGroup.Contains(entityA);
            bool isBEnemy = enemyEntitiesGroup.Contains(entityB);
            bool isABarrier = barrierEntitiesGroup.Contains(entityA);
            bool isBBarrier = barrierEntitiesGroup.Contains(entityB);

            if ( (isAEnemy && isBBarrier) || (isBEnemy && isABarrier) )
            {
                pCounter[0]++;
            }
        }
    }

    private struct ListTriggerEventEntitiesJob : ITriggerEventsJob
    {
        [DeallocateOnJobCompletion]public NativeArray<Entity> enemyEntitiesGroup;
        [DeallocateOnJobCompletion]public NativeArray<Entity> barrierEntitiesGroup;
        public NativeArray<Entity> toDestroy;
        [NativeFixedLength(1)] public NativeArray<int> pCounter;

        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.Entities.EntityA;
            Entity entityB = triggerEvent.Entities.EntityB;

            bool isAEnemy = enemyEntitiesGroup.Contains(entityA);
            bool isBEnemy = enemyEntitiesGroup.Contains(entityB);
            bool isABarrier = barrierEntitiesGroup.Contains(entityA);
            bool isBBarrier = barrierEntitiesGroup.Contains(entityB);

            if ( (isAEnemy && isBBarrier) || (isBEnemy && isABarrier) )
            {
                // Increment the output counter in a thread safe way.
                var count = ++pCounter[0] - 1;
                toDestroy[count] = entityA;
            }
        }
    }

    private struct DestroyEntitiesJob : IJob
    {
        [DeallocateOnJobCompletion]public NativeArray<Entity> toDestroy;
        public EntityCommandBuffer commandBuffer;
        public void Execute()
        {
            for(int entityIndex = 0; entityIndex < toDestroy.Length; entityIndex++)
            {
                commandBuffer.DestroyEntity(toDestroy[entityIndex]);
                GameManager.points += 1;
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        triggerEntitiesIndex[0] = 0;
        GetTriggerEventCount triggerEventCount = new GetTriggerEventCount()
        {
            pCounter = triggerEntitiesIndex,
            enemyEntitiesGroup = enemyGroup.ToEntityArray(Allocator.TempJob),
            barrierEntitiesGroup = barrierGroup.ToEntityArray(Allocator.TempJob)
        };
        JobHandle jobHandleCount = triggerEventCount.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, inputDeps);
        jobHandleCount.Complete();

        var triggerEntities = new NativeArray<Entity>(triggerEntitiesIndex[0], Allocator.TempJob);
        triggerEntitiesIndex[0] = 0;
        ListTriggerEventEntitiesJob listTriggerEventEntities = new ListTriggerEventEntitiesJob()
        {
            enemyEntitiesGroup = enemyGroup.ToEntityArray(Allocator.TempJob),
            barrierEntitiesGroup = barrierGroup.ToEntityArray(Allocator.TempJob),
            toDestroy = triggerEntities,
            pCounter = triggerEntitiesIndex
        };
        JobHandle listOfEntitiesHandle = listTriggerEventEntities.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, inputDeps);
        listOfEntitiesHandle.Complete();
        DestroyEntitiesJob destroyEntities = new DestroyEntitiesJob()
        {
            toDestroy = triggerEntities,
            commandBuffer = endSimulationEntityCommandBuffer.CreateCommandBuffer()
        };
        JobHandle destroyEntitiesHandle = destroyEntities.Schedule(inputDeps);
        endSimulationEntityCommandBuffer.AddJobHandleForProducer(destroyEntitiesHandle);

        return destroyEntitiesHandle;
    }
}
