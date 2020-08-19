using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor;
using System;

[GenerateAuthoringComponent]
public struct AsteroidSpawnerComponent : IComponentData
{
    public Entity asteroidPrefab;
    public float spawnRate;
    [SerializeField, FloatRangeSlider(0f, 30f)]
    public FloatRange spawnRadius;
    
    [NonSerialized]
    public float cooldown;
}

