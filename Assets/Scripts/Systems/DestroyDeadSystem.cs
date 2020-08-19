using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(HandleCollisionsSystem))]
public class DestroyDeadSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
    protected override void OnUpdate()
    {
        var ecb = entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        Entities.ForEach((Entity entity, int entityInQueryIndex, in HealthComponent healthComponent) => {
            if (healthComponent.value <= 0f)
                ecb.DestroyEntity(entityInQueryIndex, entity);
        }).ScheduleParallel();

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
