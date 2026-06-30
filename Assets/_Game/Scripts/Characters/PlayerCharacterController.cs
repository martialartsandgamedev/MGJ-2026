using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/**
 * Player Character is in charge of driving the movement of the player based on incoming inputs from IControllable
 */
public class PlayerCharacterController : MonoBehaviour
{
    [SerializeField] private ControlHandler m_controlHandler;
    [SerializeField] private float currentSpeed;
    [SerializeField] private MovementSettings movementSettings;

    private string m_id;
    public int PlayerIndex { get; private set; }
    private Vector2 _aimVector;
    private Vector3 _velocity;
    public FloatingUI floatingUI;
    public string ControllableID => m_id;
    public event Action InteractPressed;
    public UnityEvent<PlayerCharacterController, float> boostCooldownChanged;

    private Rigidbody _rb;

    private float _boostProgress;
    private float _timeUntilBoost;
    private bool _isBoosting;
    public bool IsBoosting => _isBoosting;
    public float CurrentSpeed => currentSpeed;
    [SerializeField] private BoostSettings boostSettings;

    private PlayerInventory _inventory;
    public Gamepad Gamepad { get; private set; }
    private Vector3 _boostDirection;
    private readonly Dictionary<string, Vector3> _externalVelocitySources = new();

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _inventory = GetComponent<PlayerInventory>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Stop boosting when we collide with _anything_
        Rumble.Play(this, Gamepad, 0.4f, 0.2f, 0.3f);

        if (!collision.gameObject.TryGetComponent(out PlayerCharacterController other))
        {
            _isBoosting = false;
            return;
        }

        bool theyAreBoosting = other.IsBoosting;
        if (!theyAreBoosting) return;

        // They are boosting — drop unless I'm also boosting and I'm faster
        if (_isBoosting && currentSpeed > other.CurrentSpeed)
        {
            return;
        }

        _isBoosting = false;
        other._isBoosting = false;
        _inventory?.DropRandom();
    }

    public void Init(int playerIndex, InputDevice[] devices)
    {
        PlayerIndex = playerIndex;
        m_id = $"Player {playerIndex + 1}";
        gameObject.name = m_id;

        foreach (var device in devices)
            if (device is Gamepad gp) { Gamepad = gp; break; }

        floatingUI = GetComponentInChildren<FloatingUI>();
        floatingUI.Init(devices);
        floatingUI.ShowPrompt("direction", 5f);

        GetComponent<PlayerInventory>()?.Init(playerIndex);
        GetComponent<FishingController>().Initialise(playerIndex);
    }

    public ControlHandler ControlHandler => m_controlHandler;

    private void FixedUpdate()
    {
        var dt = Time.fixedDeltaTime;

        ApplySteering(dt);
        ApplyBoost(dt);
        TickBoostCooldown(dt);

        _rb.linearVelocity = _velocity + GetTotalExternalVelocity();
        currentSpeed = _velocity.magnitude;
        transform.forward = Vector3.RotateTowards(transform.forward, _velocity.normalized, dt, dt);
    }

    private void ApplySteering(float dt)
    {
        var drag = _aimVector.magnitude <= 0.1f ? movementSettings.DragStrength * dt : 0f;
        var afterDrag = Vector3.MoveTowards(_velocity, Vector3.zero, drag);
        var aimTarget = new Vector3(_aimVector.x, 0f, _aimVector.y) * movementSettings.MaxSpeed;
        var afterSteer = Vector3.MoveTowards(afterDrag, aimTarget, movementSettings.Acceleration * dt);
        _velocity = Vector3.ClampMagnitude(afterSteer, movementSettings.MaxSpeed);
    }

    private void ApplyBoost(float dt)
    {
        if (!_isBoosting) return;

        _boostProgress += dt;
        var t = Mathf.Clamp01(_boostProgress / boostSettings.Duration);
        _velocity += _boostDirection * boostSettings.SpeedCurve.Evaluate(t);

        if (_boostProgress >= boostSettings.Duration)
            _isBoosting = false;
    }

    private void TickBoostCooldown(float dt)
    {
        if (_timeUntilBoost <= 0f) return;
        _timeUntilBoost = Mathf.MoveTowards(_timeUntilBoost, 0f, dt);
        boostCooldownChanged?.Invoke(this, _timeUntilBoost);
    }

    private void OnDrawGizmos()
    {
        var aimPosition = transform.position + new Vector3(_aimVector.x, 0, _aimVector.y) * movementSettings.MaxSpeed;
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(aimPosition, 1f);

        var moveVector = transform.position + _velocity;
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(moveVector, 1f);
    }

    public void SetMoveVector(Vector2 newAimVector)
    {
        _aimVector = newAimVector;
    }

    internal void PerformBoost()
    {
        if (_isBoosting || _timeUntilBoost > 0f) return;

        _isBoosting = true;
        _boostProgress = 0f;
        _timeUntilBoost = boostSettings.Cooldown;
        boostCooldownChanged?.Invoke(this, _timeUntilBoost);
        _boostDirection = Vector3.Normalize(new Vector3(_aimVector.x, 0, _aimVector.y));
    }

    internal void PerformInteract()
    {
        InteractPressed?.Invoke();
    }

    public void SetExternalVelocitySource(string sourceId, Vector3 velocity)
    {
        _externalVelocitySources[sourceId] = velocity;
    }

    public void ClearExternalVelocitySource(string sourceId)
    {
        _externalVelocitySources.Remove(sourceId);
    }

    private Vector3 GetTotalExternalVelocity()
    {
        var total = Vector3.zero;
        foreach (var velocity in _externalVelocitySources.Values)
        {
            total += velocity;
        }

        return total;
    }
}
