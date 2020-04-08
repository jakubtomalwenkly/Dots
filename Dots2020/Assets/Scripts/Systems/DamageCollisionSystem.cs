using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Collections;

[UpdateBefore(typeof(ResolveDamageSystem))]
public class DamageCollisionSystem : JobComponentSystem
{
    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;

    protected override void OnCreate()
    {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
    }

    private struct DamageCollisionJob : ITriggerEventsJob
    {
        public BufferFromEntity<Damage> damageGroup;
        [ReadOnly]public ComponentDataFromEntity<DealDamage> dealDamageGroup;
        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.Entities.EntityA;
            Entity entityB = triggerEvent.Entities.EntityB;
            if (CanEntityADealDamageToEntityB(entityA, entityB))
            {
                damageGroup[entityB].Add(new Damage
                {
                    Value = dealDamageGroup[entityA].Value
                }) ;
            }

            if (CanEntityADealDamageToEntityB(entityB, entityA))
            {
                damageGroup[entityA].Add(new Damage
                {
                    Value = dealDamageGroup[entityB].Value
                });
            }
        }

        private bool CanEntityADealDamageToEntityB(Entity entityA, Entity entityB)
        {
            return dealDamageGroup.HasComponent(entityA) && damageGroup.Exists(entityB);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        DamageCollisionJob damageCollisionJob = new DamageCollisionJob
        {
            damageGroup = GetBufferFromEntity<Damage>(false),
            dealDamageGroup = GetComponentDataFromEntity<DealDamage>(true)
        };
        JobHandle jobHandle = damageCollisionJob.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, inputDeps);
        jobHandle.Complete();
        return inputDeps;
    }
}
