using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(ForwardMovementSystem))]
public class AsteroidCyclingSystem : JobComponentSystem
{
    
    protected override void OnCreate()
    {
        base.OnCreate();
        
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {

        float2 gameFieldSize = float2.zero;

        Entities
            .WithoutBurst()
            .WithAll<Camera>().ForEach((Entity entity, in Camera camera) => {
                if (camera.enabled)
                    gameFieldSize = new float2(camera.orthographicSize * camera.aspect + 2f, camera.orthographicSize + 2f);                
            }).Run();

        return Entities.WithAll<Tag_Asteroid>().ForEach((ref Translation tranlation, in ForwardMovementComponent forwardMovement, in Rotation rotation) => {
            float3 position = tranlation.Value;
            float3 direction = math.mul(rotation.Value, math.up());
            if (math.abs(position.x) > gameFieldSize.x && direction.x * position.x > 0)
                position.x *= -1;
            if (math.abs(position.y) > gameFieldSize.y && direction.y * position.y > 0)
                position.y *= -1;
            tranlation.Value = position;
        }).Schedule(inputDeps);
    }    
}
