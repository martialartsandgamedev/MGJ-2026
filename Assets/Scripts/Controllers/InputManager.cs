using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Utilities;

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
        CameraDelta   = cameraDelta;
        Crouch        = crouch;
        Sprint        = sprint;
        Dash          = dash;
        Interact      = interact;
        PrimaryAction = primaryAction;
    }
}


public class ControllableChangeEvent : UnityEvent<IControllable> { };

public class InputManager : MonoBehaviour
{
    public static InputManager ins;

    private readonly List<PlayerSlot> m_slots = new();

    public Inputs m_uiInputs;

    public ControllableChangeEvent ControllableChangeEvent { get;set; }

    public UnityEvent<InputDevice[]> PlayerJoinRequestEvent = new();
    public int SlotCount => m_slots.Count;

    private void Awake()
    {
        if (ins == null)
        {
            ins = this;
        }
    }

    private void OnEnable()
    {
        ControllableChangeEvent ??= new ControllableChangeEvent();
        m_uiInputs = new Inputs();
        m_uiInputs.UI.Enable();
        BeginListeningForJoins();
    }

    private void OnDisable()
    {
        foreach (var slot in m_slots)
            slot.Dispose();
        m_slots.Clear();
        m_uiInputs?.Dispose();
        m_uiInputs = null;
    }

    public void Register(IControllable controllable, InputDevice[] devices = null)
    {
        devices ??= GetNextAvailableDevice();
        var slot = new PlayerSlot(controllable, devices);
        m_slots.Add(slot);
        ControllableChangeEvent?.Invoke(controllable);
    }

    public void Unregister(IControllable controllable)
    {
        var slot = m_slots.FirstOrDefault(s => s.Controllable == controllable);
        if (slot == null) return;
        slot.Dispose();
        m_slots.Remove(slot);
    }

private bool m_listeningForJoins = false;

public void BeginListeningForJoins() => m_listeningForJoins = true;
public void StopListeningForJoins()  => m_listeningForJoins = false;

    private void Update()
    {
        if (m_listeningForJoins)
            PollForJoins();

        foreach (var slot in m_slots)
            slot.Tick();

    }

    private void PollForJoins()
    {
        var usedDevices = new HashSet<InputDevice>(m_slots.SelectMany(s => s.Devices));

        foreach (var device in InputSystem.devices)
        {
            if (!AnyButtonPressedThisFrame(device)) continue;

            InputDevice[] candidates;
            if      (device is Keyboard && Mouse.current != null)    candidates = new InputDevice[] { device, Mouse.current };
            else if (device is Mouse    && Keyboard.current != null) candidates = new InputDevice[] { device, Keyboard.current };
            else                                                     candidates = new InputDevice[] { device };

            bool alreadyUsed = false;
            foreach (var d in candidates)
                if (usedDevices.Contains(d)) { alreadyUsed = true; break; }

            if (!alreadyUsed)
            {
                PlayerJoinRequestEvent?.Invoke(candidates);
                return; // One join per frame
            }
        }
    }

    private static bool AnyButtonPressedThisFrame(InputDevice device)
    {
        foreach (var control in device.allControls)
            if (control is ButtonControl button && !control.synthetic && button.wasPressedThisFrame)
                return true;
        return false;
    }

    public void ToggleInputs(bool active)
    {
        foreach (var slot in m_slots)
            slot.SetPlayerInputActive(active);
    }

    private InputDevice[] GetNextAvailableDevice()
    {
        // First player always gets Keyboard + Mouse.
        if (m_slots.Count == 0)
        {
            var kb    = Keyboard.current;
            var mouse = Mouse.current;
            if (kb != null && mouse != null)
                return new InputDevice[] { kb, mouse };
            return Array.Empty<InputDevice>();
        }

        // Additional players get the next unpaired Gamepad.
        var usedDevices = new HashSet<InputDevice>(m_slots.SelectMany(s => s.Devices));
        foreach (var gamepad in Gamepad.all)
        {
            if (!usedDevices.Contains(gamepad))
                return new InputDevice[] { gamepad };
        }

        Debug.LogWarning("InputManager: No available device for new player slot.");
        return Array.Empty<InputDevice>();
    }

}