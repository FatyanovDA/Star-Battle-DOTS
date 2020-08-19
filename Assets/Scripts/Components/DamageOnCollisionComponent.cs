using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct DamageOnCollisionComponent : IComponentData
{
    public float value;
}
