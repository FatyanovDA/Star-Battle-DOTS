using System;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct WeaponComponent : IComponentData
{
    public Entity projectilePrefab;
    public float fireRate;
    [NonSerialized]
    public float cooldown;
}
