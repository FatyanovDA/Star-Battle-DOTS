using System.Numerics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(DetectionSystem))]
public class HeadingSystem : JobComponentSystem
{
    private const float m_minDistance = 1.5f;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float time = Time.DeltaTime;
        ComponentDataFromEntity<Translation> translations = GetComponentDataFromEntity<Translation>(true);

        return Entities.WithAll<Tag_Ufo>().ForEach((Entity entity, ref Rotation rotation, ref ForwardMovementComponent forwardMovementComponent, in DetectorComponent detectorComponent) =>
        {
            if (detectorComponent.closestUfoEntity == Entity.Null)
            {
                forwardMovementComponent.isMoving = false;
                return;
            }

            float3 thisUfoPosition = translations[entity].Value;
            float3 targetUfoPosition = translations[detectorComponent.closestUfoEntity].Value;
            float3 direction = math.normalizesafe(targetUfoPosition - thisUfoPosition);

            rotation.Value = quaternion.LookRotationSafe(new float3(0, 0, 1), direction);

            forwardMovementComponent.isMoving = true;

            float speed = forwardMovementComponent.speed;
            bool isTooClose = math.distancesq(thisUfoPosition, targetUfoPosition) < m_minDistance * m_minDistance;

            forwardMovementComponent.speed = math.select(speed, -speed, isTooClose & speed > 0 || !isTooClose && speed < 0);

        }).WithReadOnly(translations).Schedule(inputDeps);
    }
}