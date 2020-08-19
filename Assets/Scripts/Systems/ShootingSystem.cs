using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

[UpdateAfter(typeof(DetectionSystem))]
public class ShootingSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem _entityCommandBufferSystem;
    protected override void OnCreate()
    {
        base.OnCreate();
        _entityCommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();

    }
    protected override void OnUpdate()
    {
        float time = Time.DeltaTime;
        var ecb = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        
        Entities.ForEach((Entity entity, int entityInQueryIndex, ref WeaponComponent weaponComponent, in DetectorComponent detectorComponent, in Translation translation, in Rotation rotation) =>
        {
            if(weaponComponent.projectilePrefab != Entity.Null)
                if (weaponComponent.cooldown <= 0)
                {
                    float3 ufoPosition;
                    float3 asteroidPosition;

                    bool ufoIsNull = detectorComponent.closestUfoEntity == Entity.Null;
                    if (!ufoIsNull)
                        ufoPosition = GetComponent<Translation>(detectorComponent.closestUfoEntity).Value;
                    else
                        ufoPosition = float3.zero;

                    bool asteroidIsNull = detectorComponent.closestAsteroidEntity == Entity.Null;
                    if (!asteroidIsNull)
                        asteroidPosition = GetComponent<Translation>(detectorComponent.closestAsteroidEntity).Value;
                    else
                        asteroidPosition = float3.zero;

                    if (!ufoIsNull || !asteroidIsNull) 
                    {
                        float ufoDistancesq = math.select(float.PositiveInfinity, math.distancesq(translation.Value, ufoPosition), ufoIsNull);
                        float asteroidDistancesq = math.select(float.PositiveInfinity, math.distancesq(translation.Value, asteroidPosition), asteroidIsNull);

                        float3 targetPosition = math.select(ufoPosition, asteroidPosition, ufoDistancesq < asteroidDistancesq);
                        quaternion projectileRotation = quaternion.LookRotationSafe(new float3(0, 0, 1), math.normalizesafe(targetPosition - translation.Value));

                        var instance = ecb.Instantiate(entityInQueryIndex, weaponComponent.projectilePrefab);
                        ecb.SetComponent(entityInQueryIndex, instance, translation);
                        ecb.SetComponent(entityInQueryIndex, instance, new Rotation { Value = projectileRotation });
                        ecb.SetComponent(entityInQueryIndex, instance, new ParentEntityComponent { value = entity });

                        weaponComponent.cooldown += 1f / weaponComponent.fireRate - time;
                    }
                }
                else
                {
                    weaponComponent.cooldown -= time;
                }
        }).ScheduleParallel();

        _entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }            
}
