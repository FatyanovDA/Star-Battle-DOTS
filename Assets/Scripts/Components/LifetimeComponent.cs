using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct LifetimeComponent : IComponentData
{
    public float time; 
}
