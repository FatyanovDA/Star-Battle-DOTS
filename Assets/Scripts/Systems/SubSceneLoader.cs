using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Scenes;
using UnityEngine.InputSystem;

public class SubSceneLoader : ComponentSystem
{
    private SceneSystem sceneSystem;
    private Keyboard keyboard;

    protected override void OnCreate()
    {
        sceneSystem = World.GetOrCreateSystem<SceneSystem>();
        keyboard = Keyboard.current;
    }

    protected override void OnUpdate()
    {
        if (keyboard.spaceKey.isPressed)
        {
            Debug.Log("Space");
            LoadSubscene(SubSceneReferences.Instance.scene1);
        }
        if (keyboard.altKey.isPressed)
        {
            Debug.Log("Alt");
            UnloadSubscene(SubSceneReferences.Instance.scene1);
        }
    }

    private void LoadSubscene(SubScene subScene)
    {
        sceneSystem.LoadSceneAsync(subScene.SceneGUID);
    }
    private void UnloadSubscene(SubScene subScene)
    {
        sceneSystem.UnloadScene(subScene.SceneGUID);
    }
}
