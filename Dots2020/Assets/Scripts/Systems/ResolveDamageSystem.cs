using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;

[UpdateBefore(typeof(DestroySystem))]
public class ResolveDamageSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBuffer;

    protected override void OnCreate()
    {
        endSimulationEntityCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityCommandBuffer entityCommandBuffer = endSimulationEntityCommandBuffer.CreateCommandBuffer();

        Entities.WithNone<ToDestroyTag>().ForEach((Entity entity, ref DynamicBuffer<Damage> damageBuffer,ref HealthData health) =>
        {
            for (int i = 0; i < damageBuffer.Length; i++)
            {
                health.currentHealth -= damageBuffer[i].Value;
                if (health.currentHealth <= 0)
                {
                    entityCommandBuffer.AddComponent<ToDestroyTag>(entity);
                    break;
                }
            }
            damageBuffer.Clear();
        }).Run();

        return default;
    }
}
