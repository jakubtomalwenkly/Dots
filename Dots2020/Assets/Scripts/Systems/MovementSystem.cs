using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Collections;
using Unity.Physics;
using Unity.Mathematics;

public class MovementSystem : JobComponentSystem
{
    [RequireComponentTag(typeof(DestinationData))]
    private struct SetDestinationJob : IJobForEachWithEntity<MovementData, Translation>
    {
        [ReadOnly] public ComponentDataFromEntity<Translation> nodeTranslations;
        [NativeDisableParallelForRestriction] public ComponentDataFromEntity<DestinationData> Destinations;

        public void Execute(Entity entity, int index, ref MovementData movemont, [ReadOnly]ref Translation translation)
        {

            var NodePosition2D = nodeTranslations[Destinations[entity].destination].Value;
            var EnemyPosition2D = translation.Value;
            NodePosition2D.y = 0f;
            EnemyPosition2D.y = 0f;
            if (SomeStuff.CalculateDistance(nodeTranslations[Destinations[entity].destination].Value, translation.Value) <= 1f)
            {
                var newDestination = Destinations[Destinations[entity].destination];
                Destinations[entity] = newDestination;
                movemont.direction = SomeStuff.CalculateDirection(translation.Value, nodeTranslations[Destinations[entity].destination].Value);
            }
            else if (SomeStuff.bool3Tobool(movemont.direction == float3.zero))
            {
                //enemy won't calculate direction again whe something else change enemy's position  
                movemont.direction = SomeStuff.CalculateDirection(translation.Value, nodeTranslations[Destinations[entity].destination].Value);
            }
            movemont.direction = SomeStuff.CalculateDirection(translation.Value, nodeTranslations[Destinations[entity].destination].Value);

        }
    }

    private struct SetNewDirectionJob : IJobForEachWithEntity<MovementData, ChangeDirectionData>
    {
        public void Execute(Entity entity, int index, ref MovementData movement, [ReadOnly]ref ChangeDirectionData newDiraction)
        {
            var newMovement = movement;
            newMovement.direction = newDiraction.newDirection;
            movement = newMovement;
        }
    }

    private struct MovementJob : IJobForEach<Translation, MovementData>
    {
        public float delta;
        public void Execute(ref Translation translation, [ReadOnly]ref MovementData movemontData)
        {
            translation.Value += movemontData.direction * movemontData.speed * delta;
        }
    }



    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        SetDestinationJob setDestinationJob = new SetDestinationJob()
        {
            nodeTranslations = GetComponentDataFromEntity<Translation>(true),
            Destinations = GetComponentDataFromEntity<DestinationData>(false),
        };
        JobHandle destinationJobHandle = setDestinationJob.Schedule(this, inputDeps);

        SetNewDirectionJob setNewDirectionJob = new SetNewDirectionJob();
        JobHandle newDirectionJobHandle = setNewDirectionJob.Schedule(this, destinationJobHandle);

        MovementJob movementJob = new MovementJob()
        {
            delta = Time.DeltaTime
        };
        JobHandle jobHandle = movementJob.Schedule(this, newDirectionJobHandle);
        return jobHandle;
    }
}
