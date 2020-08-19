using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using System.Runtime.CompilerServices;
using UnityEngine;

[UpdateAfter(typeof(EndFramePhysicsSystem))]
[UpdateAfter(typeof(ForwardMovementSystem))]
public class HandleCollisionsSystem : JobComponentSystem
{
    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;

    private EndSimulationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();

        entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {

        CatchTriggerEventsJob job = new CatchTriggerEventsJob()
        {
            projectiles = GetComponentDataFromEntity<Tag_Projectile>(true),
            damageComponents = GetComponentDataFromEntity<DamageOnCollisionComponent>(true),
            healthComponents = GetComponentDataFromEntity<HealthComponent>(false),
            parentEntityComponents = GetComponentDataFromEntity<ParentEntityComponent>(true),
            entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer()
        };

        JobHandle jobHandle = job.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, inputDeps);
        entityCommandBufferSystem.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }

    [BurstCompile]
    private struct CatchTriggerEventsJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentDataFromEntity<Tag_Projectile> projectiles;
        [ReadOnly] public ComponentDataFromEntity<DamageOnCollisionComponent> damageComponents;
        public ComponentDataFromEntity<HealthComponent> healthComponents;
        [ReadOnly] public ComponentDataFromEntity<ParentEntityComponent> parentEntityComponents;

        public EntityCommandBuffer entityCommandBuffer;

        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.EntityA;
            Entity entityB = triggerEvent.EntityB;
                        
            if (parentEntityComponents.HasComponent(entityA) && parentEntityComponents[entityA].value == entityB)
                return;
            if (parentEntityComponents.HasComponent(entityB) && parentEntityComponents[entityB].value == entityA)
                return;

            if (damageComponents.HasComponent(entityA) && healthComponents.HasComponent(entityB))
            {
                float damage = damageComponents[entityA].value;
                float health = healthComponents[entityB].value;
                healthComponents[entityB] = new HealthComponent { value = health - damage };
            }
            if (damageComponents.HasComponent(entityB) && healthComponents.HasComponent(entityA))
            {
                float damage = damageComponents[entityB].value;
                float health = healthComponents[entityA].value;
                healthComponents[entityA] = new HealthComponent { value = health - damage };
            }
            if (projectiles.HasComponent(entityA))
                entityCommandBuffer.DestroyEntity(entityA);
            if (projectiles.HasComponent(entityB))
                entityCommandBuffer.DestroyEntity(entityB);
        }
    }
}
