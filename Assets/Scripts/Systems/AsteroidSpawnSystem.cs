using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public class AsteroidSpawnSystem : SystemBase
{
    private EndInitializationEntityCommandBufferSystem _entityCommandBufferSystem;
    private Random _random;

    protected override void OnCreate()
    {
        base.OnCreate();

        _random = new Random(81736);
        _entityCommandBufferSystem = World.GetExistingSystem<EndInitializationEntityCommandBufferSystem>(); 
    }

    protected override void OnUpdate()
    {
        var ecb = _entityCommandBufferSystem.CreateCommandBuffer();
        var time = Time.DeltaTime;        
        
        Entities.WithoutBurst().ForEach((ref AsteroidSpawnerComponent asteroidSpawner) => {       
            if (asteroidSpawner.asteroidPrefab != Entity.Null)
                if (asteroidSpawner.cooldown <= 0)
                {
                    var spawnRadius = _random.NextFloat(asteroidSpawner.spawnRadius.min, asteroidSpawner.spawnRadius.max);
                    var spawnDirection = _random.NextFloat2Direction();
                    var position = new float3(spawnRadius * spawnDirection.x, spawnRadius * spawnDirection.y, 0f);

                    var instance = ecb.Instantiate(asteroidSpawner.asteroidPrefab);
                    ecb.SetComponent(instance, new Translation() { Value = position });
                    ecb.SetComponent(instance, new ForwardMovementComponent() { speed = _random.NextFloat(2f, 4f), isMoving = true });
                    ecb.SetComponent(instance, new Rotation { Value = quaternion.Euler(0f, 0f, _random.NextFloat(0f, 360f)) });

                    asteroidSpawner.cooldown += 1 / asteroidSpawner.spawnRate - time;
                }
                else
                {
                    asteroidSpawner.cooldown -= time;
                }
        }).Run();

        _entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
