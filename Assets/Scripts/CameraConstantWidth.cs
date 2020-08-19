using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraConstantWidth : MonoBehaviour
{
    public Vector2 defaultResolution = new Vector2(1920f, 1080f);

    private Camera cameraComponent;

    private float initialSize;
    private float targetAspect;

    void Start()
    {
        cameraComponent = GetComponent<Camera>();
        initialSize = cameraComponent.orthographicSize;

        targetAspect = defaultResolution.x / defaultResolution.y;

#if !UNITY_EDITOR

        cameraComponent.orthographicSize = initialSize * (targetAspect / cameraComponent.aspect);

#endif    
    }


    void Update()
    {

#if UNITY_EDITOR

        cameraComponent.orthographicSize = initialSize * (targetAspect / cameraComponent.aspect);

#endif    

    }
}
