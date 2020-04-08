using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[AlwaysSynchronizeSystem]
public class InputSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        inputDeps.Complete();
        Entities.ForEach((ref MovementData movement, ref InputData inputData, in LocalToWorld localToWorld) =>
        {
            float3 newDirection;
            float3 forward = localToWorld.Forward * (Input.GetKey(inputData.upKey) ? 1 : 0);
            float3 backward = -localToWorld.Forward * (Input.GetKey(inputData.downKey) ? 1 : 0);
            float3 right = localToWorld.Right * (Input.GetKey(inputData.rightKey) ? 1 : 0);
            float3 left = -localToWorld.Right * (Input.GetKey(inputData.lefTKey) ? 1 : 0);
            newDirection = forward + backward + right + left;
            movement.direction = newDirection;
            inputData.IsFiring = (Input.GetKey(inputData.fireKey) ? true : false);

        }).Run();
        return default;
    }
}
