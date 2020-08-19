using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct ForwardMovementComponent : IComponentData
{
    public float speed;
    public bool isMoving;
}
