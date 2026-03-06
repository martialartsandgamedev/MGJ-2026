using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public CinemachineCamera ControllableCamera = null;


    private void OnEnable()
    {
        InputManager.ins.ControllableChangeEvent.AddListener(OnControllableChange);
    }

    private void OnDisable()
    {
        InputManager.ins.ControllableChangeEvent.RemoveListener(OnControllableChange);
    }

    private void OnControllableChange(IControllable controllable)
    {
        Debug.LogFormat($"Switching camera target to {controllable.ControlHandler.name}");
        
        var newTarget = new CameraTarget();
        newTarget.TrackingTarget = controllable.ControlHandler.transform;

        ControllableCamera.Target = newTarget;
    }
}
