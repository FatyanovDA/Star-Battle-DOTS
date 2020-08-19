using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

[UpdateBefore(typeof(ForwardMovementSystem))]
public class OrbitalMovementSystem : JobComponentSystem
{
    
    private const float _2PI = 2 * math.PI;
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float time = Time.DeltaTime;

        return Entities.ForEach((ref Translation translation, ref OrbitalMovementComponent orbitalMovement) =>
        {
            Calculate(time, ref translation, ref orbitalMovement);
        }).Schedule(inputDeps);
    }    
    
    /*
    protected override void OnUpdate()
    {
        float time = Time.DeltaTime;

        Entities.ForEach((int entityInQueryIndex, ref Translation translation, ref OrbitalMovementComponent orbitalMovement) =>
        {
            Calculate(time, ref translation, ref orbitalMovement);
        }).ScheduleParallel();
    }
    */
    [BurstCompile]
    private static float clampPosition(float position)
    {
        if (position < 0)
            return position += _2PI;
        if (position > _2PI)
            return position -= _2PI;

        return position;
    }

    [BurstCompile]
    private static void Calculate(float time, ref Translation translation, ref OrbitalMovementComponent orbitalMovement)
    {
        float oldPosition = orbitalMovement.position;
        float newPosition = clampPosition(oldPosition - _2PI * orbitalMovement.frequency * time);

        translation.Value.x += orbitalMovement.radius * (math.cos(newPosition) - math.cos(oldPosition));
        translation.Value.y += orbitalMovement.radius * (math.sin(newPosition) - math.sin(oldPosition));

        orbitalMovement.position = newPosition;
    }
}    