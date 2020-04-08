using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

public class DestroySystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBuffer;

    protected override void OnCreate()
    {
        endSimulationEntityCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    private struct DestroyJob : IJobForEachWithEntity<ToDestroyTag>
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        public void Execute(Entity entity, int index, [ReadOnly]ref ToDestroyTag destroyTag)
        {
            entityCommandBuffer.DestroyEntity(index, entity);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        DestroyJob destroyJob = new DestroyJob
        {
            entityCommandBuffer = endSimulationEntityCommandBuffer.CreateCommandBuffer().ToConcurrent()
        };

        JobHandle jobHandle = destroyJob.Schedule(this, inputDeps);
        endSimulationEntityCommandBuffer.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }
}
