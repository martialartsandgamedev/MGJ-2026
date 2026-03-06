using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Utilities;

public class PlayerSlot
{
    public readonly IControllable Controllable;

    public ReadOnlyArray<InputDevice> Devices => m_user.pairedDevices;

    private readonly Inputs m_inputs;
    private InputUser m_user;
    private PlayerInputContext m_intent;

    public PlayerSlot(IControllable controllable, InputDevice[] devices)
    {
        Controllable = controllable;

        m_user = InputUser.CreateUserWithoutPairedDevices();

        foreach (var device in devices)
            m_user = InputUser.PerformPairingWithDevice(device, user: m_user);

        m_inputs = new Inputs();
        m_user.AssociateActionsWithUser(m_inputs);
        m_inputs.Player.Enable();

        m_inputs.Player.Sprint.started     += OnSprint;
        m_inputs.Player.Sprint.canceled    += OnSprint;
        m_inputs.Player.Attack.started     += OnPrimaryAction;
        m_inputs.Player.Attack.canceled    += OnPrimaryAction;
        m_inputs.Player.Interact.started   += OnInteract;
        m_inputs.Player.Interact.canceled  += OnInteract;
    }

    public void Dispose()
    {
        m_inputs.Player.Sprint.started     -= OnSprint;
        m_inputs.Player.Sprint.canceled    -= OnSprint;
        m_inputs.Player.Attack.started     -= OnPrimaryAction;
        m_inputs.Player.Attack.canceled    -= OnPrimaryAction;
        m_inputs.Player.Interact.started   -= OnInteract;
        m_inputs.Player.Interact.canceled  -= OnInteract;

        m_inputs.Dispose();
        m_user.UnpairDevicesAndRemoveUser();
    }

    public void SetPlayerInputActive(bool active)
    {
        if (active)
        {
            m_inputs.Player.Enable();
        } 
        else
            m_inputs.Player.Disable();
    }

    public void Tick()
    {
        m_intent.MoveDirection = m_inputs.Player.Move.ReadValue<Vector2>();
        m_intent.CameraDelta   = m_inputs.Player.Look.ReadValue<Vector2>();
        Controllable.AssertControlIntent(m_intent);
    }

    private void OnSprint(InputAction.CallbackContext ctx)
    {
        m_intent.Crouch = false;
        m_intent.Sprint = ctx.ReadValueAsButton();
    }

    private void OnPrimaryAction(InputAction.CallbackContext ctx) => m_intent.PrimaryAction = ctx.ReadValueAsButton();
    private void OnInteract(InputAction.CallbackContext ctx)      => m_intent.Interact       = ctx.ReadValueAsButton();
}