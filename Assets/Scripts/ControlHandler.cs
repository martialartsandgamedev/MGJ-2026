using System;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;

[Flags]
public enum WeaponPhaseFlag
{
    None = 0,
    Begin = 1 << 1,
    Repeat = 1 << 2,
    End = 1 << 3
}

[Serializable]
public struct AnimationContext
{
    public float MoveSpeed;
    public float RotateSpeed;
    
    public AnimationContext(float moveSpeed, float rotateSpeed)
    {
        MoveSpeed = moveSpeed;
        RotateSpeed = rotateSpeed;
    }
}

public class ControlHandler: MonoBehaviour
{
    public UnityEngine.CharacterController Controller;
    

    private void OnEnable()
    {
    }


    public void SetAnimatorBool(string referenceSemantic)
    {
        string[] parts = referenceSemantic.Split(':');
        string reference = parts[0];
        bool booleanValue = int.Parse(parts[1]) ==1;
     
        Debug.LogFormat($"Setting animator bool reference {reference} to {booleanValue}");
        
    }
    
    
    public void ProcessIntent(PlayerInputContext ctx)
    {
        if(ctx.Dash)
        {
            Debug.LogWarning($"I'm dashing");
        }
    }

}