using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct EntityWithPosition
{
    public Entity entity;
    public float3 position;
}

[UpdateAfter(typeof(ForwardMovementSystem))]
public class DetectionSystem : JobComponentSystem
{
    private EntityQuery _asteroidsQuery;
    private EntityQuery _ufoQuery;

    protected override void OnCreate()
    {
        base.OnCreate();
        _asteroidsQuery = GetEntityQuery(ComponentType.ReadOnly<Tag_Asteroid>(), ComponentType.ReadOnly<Translation>());
        _ufoQuery = GetEntityQuery(ComponentType.ReadOnly<Tag_Ufo>(), ComponentType.ReadOnly<Translation>());
    }

    protected override JobHandle OnUpdate(JobHandle inputDebs)
    {
        NativeArray<EntityWithPosition> asteroidPositions = new NativeArray<EntityWithPosition>(_asteroidsQuery.CalculateEntityCount(), Allocator.TempJob);

        JobHandle jobHandle = Entities.WithAll<Tag_Asteroid>().ForEach((Entity entity, int entityInQueryIndex, in Translation translation) => 
        {            
            asteroidPositions[entityInQueryIndex] = new EntityWithPosition
            {
                entity = entity,
                position = translation.Value
            };
        }).Schedule(inputDebs);

        NativeArray<EntityWithPosition> ufoPositions = new NativeArray<EntityWithPosition>(_ufoQuery.CalculateEntityCount(), Allocator.TempJob);

        jobHandle = Entities.WithAll<Tag_Ufo>().ForEach((Entity entity, int entityInQueryIndex, in Translation translation) =>
        {
            ufoPositions[entityInQueryIndex] = new EntityWithPosition
            {
                entity = entity,
                position = translation.Value
            };
        }).Schedule(jobHandle);

        jobHandle = Entities.ForEach((Entity entity, ref DetectorComponent detector, in Translation translation) =>
        {
            EntityWithPosition closestAsteroid = new EntityWithPosition { entity = Entity.Null, position = float3.zero };
            EntityWithPosition closestUfo = new EntityWithPosition { entity = Entity.Null, position = float3.zero };

            EntityWithPosition thisEntityWithPosition = new EntityWithPosition 
            { 
                entity = entity,
                position = translation.Value 
            };
            float asteroidDetectionDistanceSquare = detector.asteroidDetectionDistance * detector.asteroidDetectionDistance;

            for (int i = 0; i < asteroidPositions.Length; i++)
                closestAsteroid = FindClosestPosition(thisEntityWithPosition, asteroidDetectionDistanceSquare, closestAsteroid, asteroidPositions[i]);

            float ufoDetectionDistanceSquare = detector.ufoDetectionDistance * detector.asteroidDetectionDistance;

            for (int i = 0; i < ufoPositions.Length; i++)
                closestUfo = FindClosestPosition(thisEntityWithPosition, ufoDetectionDistanceSquare, closestUfo, ufoPositions[i]);

            detector.closestAsteroidEntity = closestAsteroid.entity;
            detector.closestUfoEntity = closestUfo.entity;
        }).Schedule(jobHandle);

        jobHandle = asteroidPositions.Dispose(jobHandle);
        jobHandle = ufoPositions.Dispose(jobHandle);

        return jobHandle;
    }

    [BurstCompile]
    private static EntityWithPosition FindClosestPosition(EntityWithPosition thisEntityWithPosition, float detectionDistanceSquare, EntityWithPosition currentClosestEntity, EntityWithPosition tryNewEntity)
    {
        if (thisEntityWithPosition.entity == tryNewEntity.entity)
            return currentClosestEntity;

        float distancesq = math.distancesq(thisEntityWithPosition.position, tryNewEntity.position);

        var inDetectionDistance = distancesq < detectionDistanceSquare;

        if (currentClosestEntity.entity == Entity.Null & inDetectionDistance)
            return tryNewEntity;
        var nearest = distancesq < math.distancesq(thisEntityWithPosition.position, currentClosestEntity.position);
                       
        return inDetectionDistance && nearest ? tryNewEntity : currentClosestEntity;
    }    
}

[DisableAutoCreation]
public class DebugGridSystem : SystemBase
{
    protected override void OnUpdate()
    {
        for (int x = -100; x <= 100; x++)
        {
            UnityEngine.Debug.DrawLine(new Vector3((float)x, -100f, 0f), new Vector3((float)x, 100f, 0f));
            UnityEngine.Debug.DrawLine(new Vector3(-100f, (float)x, 0f), new Vector3(100f, (float)x, 0f));
        }
    }
}

#if UNITY_EDITOR
[AlwaysSynchronizeSystem]
public class DebugDetectionSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var translations = GetComponentDataFromEntity<Translation>();

        Entities.WithoutBurst().ForEach((in Translation tranlation, in DetectorComponent detectorComponent) =>
        {
            if (detectorComponent.closestUfoEntity != Entity.Null)
                if(EntityManager.Exists(detectorComponent.closestUfoEntity))
                    UnityEngine.Debug.DrawLine(tranlation.Value, translations[detectorComponent.closestUfoEntity].Value, Color.red);
            if (detectorComponent.closestAsteroidEntity != Entity.Null)
                if (EntityManager.Exists(detectorComponent.closestAsteroidEntity))
                    UnityEngine.Debug.DrawLine(tranlation.Value, translations[detectorComponent.closestAsteroidEntity].Value, Color.green);
        }).Run();

        return default;
    }
}
#endif