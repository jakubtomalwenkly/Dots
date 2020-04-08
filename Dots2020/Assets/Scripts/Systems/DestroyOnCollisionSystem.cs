using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Collections;

[UpdateBefore(typeof(DestroySystem))]
public class DestroyOnCollisionSystem : JobComponentSystem
{
    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBuffer;

    protected override void OnCreate()
    {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        endSimulationEntityCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }



    private struct DestroyOnTriggerJob : ITriggerEventsJob
    {
        public EntityCommandBuffer entityCommandBuffer;
        [ReadOnly]public ComponentDataFromEntity<DestroyOnContactTag> destroyOnContactGroup;

        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.Entities.EntityA;
            Entity entityB = triggerEvent.Entities.EntityB;
            if(destroyOnContactGroup.HasComponent(entityA))
            {
                entityCommandBuffer.AddComponent<ToDestroyTag>(entityA);
            }
            if (destroyOnContactGroup.HasComponent(entityB))
            {
                entityCommandBuffer.AddComponent<ToDestroyTag>(entityB);
            }
        }
    }

    private struct DestroyCollisionJob : ICollisionEventsJob
    {
        public EntityCommandBuffer entityCommandBuffer;
        [ReadOnly]public ComponentDataFromEntity<DestroyOnContactTag> destroyOnContactGroup;
        public void Execute(CollisionEvent collisionEvent)
        {
            {
                Entity entityA = collisionEvent.Entities.EntityA;
                Entity entityB = collisionEvent.Entities.EntityB;
                if (destroyOnContactGroup.HasComponent(entityA))
                {
                    entityCommandBuffer.AddComponent<ToDestroyTag>(entityA);
                }
                if (destroyOnContactGroup.HasComponent(entityB))
                {
                    entityCommandBuffer.AddComponent<ToDestroyTag>(entityB);
                }
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        DestroyOnTriggerJob destroyOnTriggerJob = new DestroyOnTriggerJob
        {
            entityCommandBuffer = endSimulationEntityCommandBuffer.CreateCommandBuffer(),
            destroyOnContactGroup = GetComponentDataFromEntity<DestroyOnContactTag>(true)
        };
        JobHandle jobHandleTrigger = destroyOnTriggerJob.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, inputDeps);
        jobHandleTrigger.Complete();

        DestroyCollisionJob destroyCollisionJob = new DestroyCollisionJob
        {
            entityCommandBuffer = endSimulationEntityCommandBuffer.CreateCommandBuffer(),
            destroyOnContactGroup = GetComponentDataFromEntity<DestroyOnContactTag>(true)
        };
        JobHandle jobHandleCollision = destroyOnTriggerJob.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, inputDeps);
        jobHandleCollision.Complete();
        return inputDeps;
    }

}
