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

public  class ControlHandler: MonoBehaviour
{
    public CharacterController Controller;
    
    public CinemachineOrbitalFollow OrbitalFollow;
    
    [SerializeField]
    private float m_cameraSensitivity = 0.15f;

    public Ray aimRay;
    
    private RectTransform Reticle = null;
    
    //Local offset from player that indicates the direction of shot, range projectiles
    [SerializeField]
    private Vector3 m_reticleoffset = Vector3.zero;
    
    //I don't like how much I'm fetching the main camera in general
    public Vector3 ReticlePos
    {
        get
        {
            if (Reticle == null)
            {
                return Vector3.zero;
            }
            
            //Projects reticle based on a cached (or provided muzzle)
            return ProjectedReticle(Muzzle);
        }
    }

    public Transform Muzzle
    {
        get
        {
            return Camera.main.transform;
        }
    }


    [SerializeField]
    [Range(0,1)]
    private float m_turnThreshold = 0.5f;
    
    private float m_cameraCoefficient = 0;

    [SerializeField]
    private float m_nimble = 1;
    
    public float GlobalWeight = 1f;
    public float HeadWeight = 1f;
    public float BodyWeight = 0.3f;

    public float m_lookDistance = 1f;
    
    //Initialise animation context - the modifications currently in place from animations
    public AnimationContext m_animationContext = new AnimationContext(1f,1f);

    public bool CameraBasedRotation = false;
    
    [SerializeField]
    private bool m_primaryActionSwitch = false;
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(ReticlePos,0.2f);
    }
    
    private void OnEnable()
    {
        //Fetch the reticle from the alread instance UI singleton
        Reticle = UIManager.Ins.AimReticle.rectTransform;
    }

    public Vector3 ProjectedReticle(Transform muzzle)
    {
        //Flattened camera forward
        Vector3 camForward = Camera.main.transform.forward;

        camForward = FlattenCameraForward();
        
        //Build a ray from the camera, to its flattened forward
        aimRay = Camera.main.ScreenPointToRay(Reticle.position);

        //Get point at end of ray (20)
        //This isn't the range BUT the distance that the reticle lives at
        //Its somewhat arbitrary when scanning from camera
        Vector3 targetPoint = aimRay.origin + aimRay.direction * 20;
        
        return targetPoint;
    }
    
    public Vector3 FlattenCameraForward()
    {
        Vector3 forward = Camera.main.transform.forward;
        Vector3 flatForward = forward;
        flatForward.y = 0;
        
        return flatForward.sqrMagnitude < Mathf.Epsilon? forward.normalized: flatForward.normalized;
    }
    
    public Vector3 GetMoveDirection(Vector2 input)
    {
        //Debug.LogFormat($"Getting move direction from input {input}");
        
        Vector3 forward =  Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;

        // Remove vertical component so we stay grounded
        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        return  
        ( 
            forward * input.y
            + right * input.x 
        );
    }

    public void ModifySpeed(float modifier)
    {
        m_animationContext.MoveSpeed = modifier;
    }

    public void ModifyRotate(float modifier)
    {
        m_animationContext.RotateSpeed = modifier;
    }

    //Weapon based hitscan - requires additional class structures
    // public void DoHitScan()
    // {
    //     Weapon weapon = Wrapper.EquippedItems.weapon;
    //     
    //     //Do fake hit scan
    //     //Getting this every time is tedious
    //     //Maybe I can design reticles/hit directions per weapon bheaviour
    //         
    //     //This is the execution request in code
    //     //It uses the ctx control handler to get required info to establish a hit scan
    //     var rayOrigin = aimRay.origin;
    //     var projectedReticle = ProjectedReticle(Muzzle);
    //     var directionToReticle = (projectedReticle - rayOrigin).normalized;
    //
    //     var range = weapon.Range;
    //         
    //     //Draw a line to represent the hit scan
    //     Debug.DrawLine(rayOrigin, rayOrigin+ directionToReticle*range, Color.red,0.025f);
    //         
    //     //The actual hitscan
    //     var hits = Physics.SphereCastAll(rayOrigin, 0.1f, directionToReticle, range, ~0, QueryTriggerInteraction.Ignore);
    //
    //     foreach (RaycastHit hit in hits)
    //     {
    //         Mortal mortal;
    //                 
    //         if (hit.transform.TryGetComponent(out mortal))
    //         {
    //             if (mortal == Wrapper.Mortal)
    //             {
    //                 continue;    
    //             }
    //             else
    //             {
    //                 SkulDebug.DrawSphere(hit.point, 0.2f, Color.green,0.1f);
    //                 mortal.DoDamage(new DamageContext(mortal.Attributes.CurrentHealth, weapon,Wrapper.Mortal));
    //                 break;
    //             }
    //         }
    //         else
    //         {
    //             SkulDebug.DrawSphere(hit.point, 0.2f, Color.red,0.1f);
    //         }
    //     }
    // }

    public void SetAnimatorBool(string referenceSemantic)
    {
        string[] parts = referenceSemantic.Split(':');
        string reference = parts[0];
        bool booleanValue = int.Parse(parts[1]) ==1;
     
        Debug.LogFormat($"Setting animator bool reference {reference} to {booleanValue}");
        
        // Wrapper.Animator.SetBool(reference, booleanValue);
    }
    
    
    public void ProcessIntent(PlayerInputContext ctx)
    {
        // var weapon = Wrapper.EquippedItems.weapon;
        // var mortal = Wrapper.Mortal;
        
        // //Process main action
        // if (ctx.PrimaryAction)
        // {
        //     var angleCameraPlayer = Vector3.SignedAngle(FlattenCameraForward(), transform.forward, Vector3.up);
        //     float offset = 90f - angleCameraPlayer;
        //     
        //     //Debug.LogFormat($"ANGLE Camera/Player {angleCameraPlayer}");
        //     
        //     //If the repeat switch is OFF we just pressed it
        //     if (!m_primaryActionSwitch)
        //     { 
        //         //Debug.LogFormat($"Primary action switch on");
        //         m_primaryActionSwitch = true;
        //         
        //         m_chestForwardConstraint.transform.GetChild(0).localRotation = Quaternion.AngleAxis(offset, Vector3.up);
        //         weapon.ProcessWeaponAction(WeaponPhaseFlag.Begin, Wrapper);
        //     }
        //     //This can skip over and go straight to end
        //     //This leaves us stuck in pre repeat state (the repeat sequence does not fire
        //     else //If its on, we are now repeating
        //     {
        //         m_chestForwardConstraint.transform.GetChild(0).localRotation = Quaternion.AngleAxis(offset, Vector3.up);
        //         m_chestForwardConstraint.weight = 1;
        //         weapon.ProcessWeaponAction(WeaponPhaseFlag.Repeat, Wrapper);
        //     }
        // }
        // else //CTX is no longer asking for the primary action
        // {
        //     if (m_primaryActionSwitch) //If the action switch is on, turn it off
        //     {
        //         //Debug.LogFormat($"Primary action switch off");
        //         m_primaryActionSwitch = false;
        //         weapon.ProcessWeaponAction(WeaponPhaseFlag.End, Wrapper);
        //     }
        // }
        
        //Manually controlling speed
        // float speed = ctx.Crouch ? mortal.StatSheet.Stats.MovementSpeed / 8 : mortal.StatSheet.Stats.MovementSpeed;
        // speed = ctx.Sprint ? mortal.StatSheet.Stats.MovementSpeed * 2 : speed;

        //Debug.LogFormat($"SPEED: Speed is {speed}");
        
        Vector3 moveDirection = GetMoveDirection(ctx.MoveDirection);
        
        float cameraMatch = Vector3.Dot(transform.forward, moveDirection);
        m_cameraCoefficient = Mathf.Lerp(m_cameraCoefficient, Mathf.InverseLerp(m_turnThreshold, 1, cameraMatch), Time.deltaTime * m_nimble);
        
        
        Vector3 scaledMoveDirection = moveDirection /*speed*/ * m_cameraCoefficient * Time.deltaTime * m_animationContext.MoveSpeed;
        Vector3 resolvedMove = new Vector3(scaledMoveDirection.x, -10, scaledMoveDirection.z);
        
        Controller.Move(resolvedMove);


        //Animation setters
        float walkIndex = 0f;
        
        if (ctx.Sprint)
        {
            walkIndex = 0.75f;
        }
        if (ctx.Crouch)
        {
            walkIndex = 1.5f;
        }
        
        //Set the roll
        //Animator.SetBool("_rolling", ctx.Dash);
        
        //Trigger movement
        //Animator.SetBool("_moving", moveDirection.magnitude>0);
        
        //Movement type
        //Animator.SetFloat("_moveIndex", walkIndex);
        
        if (CameraBasedRotation)
        {
            //Flat camera direction
            var fc = Camera.main.transform.forward;
            fc.y = 0;
                
            transform.rotation = Quaternion.Slerp(transform.rotation, quaternion.LookRotation(fc, Vector3.up), Time.deltaTime * 10f);
        }

        else
        {
            //Always rotate as long as there is intent
            if (ctx.MoveDirection.magnitude > 0.001)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, quaternion.LookRotation(moveDirection, Vector3.up), Time.deltaTime * 10f * ctx.MoveDirection.magnitude * m_animationContext.RotateSpeed);
            }
        }

        //Normal camera movement
        OrbitalFollow.HorizontalAxis.Value = (OrbitalFollow.HorizontalAxis.Value + (ctx.CameraDelta.x * m_cameraSensitivity));
        OrbitalFollow.VerticalAxis.Value = Mathf.Clamp(OrbitalFollow.VerticalAxis.Value - (ctx.CameraDelta.y*m_cameraSensitivity), 10,60);

        if (ctx.Interact)
        {
            //TryInteract();
        }
    }

    // public void TryInteract()
    // {
    //     Debug.LogFormat($"{Wrapper.Mortal.Soul.Profile.FullName} tried to interact");
    //     SkulDebug.DrawSphere(transform.position,2,Color.magenta,0.2f);
    //     
    //     //Check for interactions
    //     var detections = Physics.OverlapSphere(transform.position, 2);
    //
    //     foreach (var detected in detections)
    //     {
    //         Interactible interactible = null;
    //
    //         //If the detected object is an interactible object
    //         if (detected.TryGetComponent(out interactible))
    //         {
    //             interactible.OnInteract(Wrapper);
    //         }
    //     }
    //     
    // }
}