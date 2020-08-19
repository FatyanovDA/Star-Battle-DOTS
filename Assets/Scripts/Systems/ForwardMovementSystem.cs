using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

public class ForwardMovementSystem : JobComponentSystem
{    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float time = Time.DeltaTime;

        return Entities.ForEach((ref Translation translation, in ForwardMovementComponent forwardMovement, in Rotation rotation) =>
        {
            if (forwardMovement.isMoving)
                translation.Value += forwardMovement.speed * math.mul(rotation.Value, math.up()) * time;
        }).Schedule(inputDeps);
    }
}
