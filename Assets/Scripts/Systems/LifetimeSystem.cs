using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class LifetimeSystem : SystemBase
{
    EndInitializationEntityCommandBufferSystem _entityCommandBufferSystem;
    protected override void OnCreate()
    {
        base.OnCreate();
        _entityCommandBufferSystem = World.GetExistingSystem<EndInitializationEntityCommandBufferSystem>();

    }
    protected override void OnUpdate()
    {
        float time = Time.DeltaTime;
        var ecb = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        
        Entities.ForEach((Entity entity, int entityInQueryIndex, ref LifetimeComponent lifetimeComponent) => {
            if (lifetimeComponent.time > 0)
                lifetimeComponent.time -= time;
            else
            {
                ecb.DestroyEntity(entityInQueryIndex, entity);
            }
        }).ScheduleParallel();

        _entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
