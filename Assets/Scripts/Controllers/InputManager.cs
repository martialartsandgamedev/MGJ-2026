using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[Serializable]
public struct PlayerInputContext
{
    public Vector2 MoveDirection;
    public Vector2 CameraDelta;
    public bool Crouch;
    public bool Sprint;
    public bool Dash;
    public bool Interact;
    public bool PrimaryAction;

    public PlayerInputContext(Vector2 moveDirection, Vector2 cameraDelta, bool crouch = false, bool sprint = false, bool dash = false, bool interact = false, bool primaryAction = false)
    {
        MoveDirection = moveDirection;
        CameraDelta = cameraDelta;
        Crouch = crouch;
        Sprint = sprint;
        Dash = dash;
        Interact = interact;
        PrimaryAction = primaryAction;
    }
}

public struct UIInputContext
{
    public bool RequestMenu;

    public UIInputContext(bool requestMenu)
    {
        RequestMenu = requestMenu;
    }
}



public class ControllableChangeEvent : UnityEvent<IControllable> { };


public class InputManager : LevelEventMonoBehaviour
{
    public static InputManager ins;
    
    public Inputs Inputs;
    
    private Dictionary<string, IControllable> ControllableIndex = new Dictionary<string, IControllable>();
    
    //This may have to be set via ID or something
    [SerializeField]
    private string m_activeControllableID;

    [SerializeField]
    [HideInInspector]
    private IControllable m_activeControllable = null;
    
    private PlayerInputContext m_playerInputIntent = new PlayerInputContext()
        ;
    public IControllable ActiveControllable
    {
        get
        {
            if (m_activeControllable == null)
            {
                if (SetControllable(m_activeControllableID))
                {
                    return m_activeControllable;
                }
                else
                {
                    //If the ID doesn't resolve, we cant set it and must return null.
                    return null;   
                }
            }
            else
            {
                //If the active controllable isnt null, just return it.
                return m_activeControllable;
            }
        }
        set
        {
            m_activeControllable = value;
        }
    }
    
    public ControllableChangeEvent ControllableChangeEvent { get; private set; }
    
    private void  Awake()
    {
        if (ins == null)
        {
            ins = this;
        }
    }

    private void OnEnable()
    {
        base.OnEnable();
        
        if (ControllableChangeEvent == null)
        {
            ControllableChangeEvent = new ControllableChangeEvent();
        }
        
        if (Inputs == null)
        {
            Inputs = new Inputs();
            Inputs.Enable();
        }
        
        Inputs.Player.Crouch.performed += OnCrouch;
        
        Inputs.Player.Sprint.started  += OnSprint;
        Inputs.Player.Sprint.canceled  += OnSprint;

        Inputs.Player.Jump.started  += OnDash;
        Inputs.Player.Jump.canceled  += OnDash;

        Inputs.Player.Attack.started += OnPrimaryAction;
        Inputs.Player.Attack.canceled += OnPrimaryAction;

        Inputs.Player.Interact.started += OnInteract;
        Inputs.Player.Interact.canceled += OnInteract;
    }

    private void OnDisable()
    {
        base.OnDisable();
        
        Inputs.Player.Crouch.performed -= OnCrouch;
        
        Inputs.Player.Sprint.started  -= OnSprint;
        Inputs.Player.Sprint.canceled  -= OnSprint;
        
        Inputs.Player.Jump.started  -= OnDash;
        Inputs.Player.Jump.canceled  -= OnDash;
        
        Inputs.Player.Attack.started -= OnPrimaryAction;
        Inputs.Player.Attack.canceled -= OnPrimaryAction;
        
        Inputs.Player.Interact.started -= OnInteract;
        Inputs.Player.Interact.canceled -= OnInteract;
    }

    
    public void ToggleInputs(bool active)
    {
        if (active)
        {
            Inputs.Player.Enable();
        }
        else
        {
            Inputs.Player.Disable();   
        }
    }
    
    private bool SetControllable(string newControllableID)
    {
        IControllable newControllable = null;

        //The ID resolves to a new controllable
        if (ControllableIndex.TryGetValue(newControllableID, out newControllable))
        {
            m_activeControllableID = newControllableID;
            ActiveControllable = newControllable;

            //Broadcast the change if the application is playing (has reference to control instances
            if (Application.isPlaying)
            {
                ControllableChangeEvent.Invoke(newControllable);
            }

            return true;
        }

        return false;
    }
    
    public void CycleControllable()
    {
        if (ControllableIndex.Count < 1)
        {
            Debug.LogFormat($"No controllables to cycle through.");
            return;
        }
        
        int elementIndex = ControllableIndex.Values.ToList().IndexOf(ActiveControllable);
        int newIndex = (int)Mathf.Repeat(elementIndex + 1, ControllableIndex.Values.Count);
        SetControllable(ControllableIndex.ElementAt(newIndex).Key);
    }
    
    private void OnInteract(InputAction.CallbackContext ctx)
    {
        m_playerInputIntent.Interact = ctx.ReadValueAsButton();
    }
    
    private void OnCrouch(InputAction.CallbackContext ctx)
    {
        m_playerInputIntent.Crouch = !m_playerInputIntent.Crouch;
    }
    
    //Turns ON
    private void OnDash(InputAction.CallbackContext ctx)
    {
        m_playerInputIntent.Dash = ctx.ReadValueAsButton();
        
        //Scrub primary action intent from player intent
        m_playerInputIntent.PrimaryAction = false;
    }
    
    private void OnSprint(InputAction.CallbackContext ctx)
    {
        //Any sprint action toggles off crouch
        m_playerInputIntent.Crouch = false;
        
        m_playerInputIntent.Sprint = ctx.ReadValueAsButton();
    }

    //Left mouse button was pressed
    private void OnPrimaryAction(InputAction.CallbackContext ctx)
    {
        m_playerInputIntent.PrimaryAction = ctx.ReadValueAsButton();
    }

    public void Register(IControllable controllable)
    {
        ControllableIndex.Add(controllable.ControllableID, controllable);
        CycleControllable();
    }

    private void Update()
    {
        if (m_activeControllable != null)
        {
            //PLAYER
            m_playerInputIntent.MoveDirection = Inputs.Player.Move.ReadValue<Vector2>();

            //Camera movement
            m_playerInputIntent.CameraDelta = Inputs.Player.Look.ReadValue<Vector2>();

            ActiveControllable.AssertControlIntent(m_playerInputIntent);
        }

        else
        {
            Debug.LogWarningFormat($"There is no controllable set");
        }
        
        //UI
            var requestMenu = Inputs.UI.MiddleClick.WasPressedThisFrame();

            UIManager.Ins.ProcessUIRequest(new UIInputContext(requestMenu));
    }
    
    //This is mega clean
    protected override void OnLevelPrepared(Level level)
    {
        Debug.LogFormat($"Input Manager received day prepare event");
        Cursor.lockState = CursorLockMode.None;
        Inputs.Player.Disable();
    }
   
    protected override void OnlevelStarted(Level level)
    {
        Debug.LogFormat($"Input Manager received day start event");
        Cursor.lockState = CursorLockMode.Locked;
        Inputs.Player.Enable();
    }
    
    protected override void OnLevelTicked(Level level, float deltaTime)
    {
       
    }
    
    protected override void OnLevelEnded(Level level)
    {
        Cursor.lockState = CursorLockMode.None;
        Inputs.Player.Disable();
    }
}
