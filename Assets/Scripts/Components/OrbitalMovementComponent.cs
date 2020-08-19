using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct OrbitalMovementComponent : IComponentData
{
    public float radius;
    public float frequency;

    public float position;    
}
