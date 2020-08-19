using System.Collections;
using System.Collections.Generic;
using Unity.Scenes;
using UnityEngine;

public class SubSceneReferences : MonoBehaviour
{
    public SubScene scene1;
    public static SubSceneReferences Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
}
