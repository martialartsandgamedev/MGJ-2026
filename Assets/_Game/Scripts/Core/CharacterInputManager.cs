using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class CharacterInputManager : MonoBehaviour
{
    public event Action<int> OnInteractPressed;
    public event Action<int> OnLeavePressed;

    public int Slot { get; private set; } = -1;

    private PlayerInput _input;
    private PlayerCharacterController _character;

    private void Awake()
    {
        _input = GetComponent<PlayerInput>();
    }

    public void Initialise(int slot)
    {
        Slot = slot;
    }

    public void AssignCharacter(PlayerCharacterController playerCharacter)
    {
        _character = playerCharacter;
    }

    public void ClearCharacter()
    {
        _character = null;
    }

    public void OnMove(InputValue value)
    {
        if (_character == null)
        {
            return;
        }

        _character.SetMoveVector(value.Get<Vector2>());
    }

    private void OnBoost(InputValue value)
    {
        if (!value.isPressed) return;

        if (_character != null)
        {
            _character.PerformBoost();
        }
    }

    private void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;

        if (_character != null)
        {
            _character.PerformInteract();
        }

        OnInteractPressed?.Invoke(Slot);
    }

    private void OnLeave(InputValue value)
    {
        if (!value.isPressed) return;
        OnLeavePressed?.Invoke(Slot);
    }

}
