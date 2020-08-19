using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[GenerateAuthoringComponent]
public struct DetectorComponent : IComponentData
{
    [NonSerialized]
    public Entity closestUfoEntity;    
    public float ufoDetectionDistance;

    [NonSerialized]
    public Entity closestAsteroidEntity;    
    public float asteroidDetectionDistance;
}
