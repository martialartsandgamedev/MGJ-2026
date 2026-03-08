using System;
using UnityEngine;
using UnityEngine.InputSystem;

/**
 * Player Character is in charge of driving the movement of the player based on incoming inputs from IControllable
 */
public class PlayerCharacter : MonoBehaviour, IControllable
{
    [SerializeField] private ControlHandler m_controlHandler;
    [SerializeField] private float currentSpeed;
    [SerializeField] private MovementSettings movementSettings;

    private string m_id;
    public int PlayerIndex { get; private set; }
    private Vector2 _aimVector;
    private Vector3 _velocity;
    private Vector2 _moveVector;
    public FloatingUI floatingUI;
    public string ControllableID => m_id;
    public PlayerInputContext Inputs { get; private set; }
    public event Action InteractPressed;

    private Rigidbody _rb;

    private float _boostProgress;
    private float _timeUntilBoost;
    private bool _isBoosting;
    public bool IsBoosting => _isBoosting;
    public float CurrentSpeed => currentSpeed;
    [SerializeField] private BoostSettings boostSettings;

    private PlayerInventory _inventory;
    public Gamepad Gamepad { get; private set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _inventory = GetComponent<PlayerInventory>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.TryGetComponent(out PlayerCharacter other)) return;

        bool theyAreBoosting = other.IsBoosting;

        if (!theyAreBoosting) return;

        // They are boosting — drop unless I'm also boosting and I'm faster
        if (_isBoosting && currentSpeed > other.CurrentSpeed) return;

        _inventory?.DropRandom();
        Rumble.Play(this, Gamepad, 0.4f, 0.2f, 0.3f);
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
    }

    public ControlHandler ControlHandler => m_controlHandler;

    private void OnDisable() => InputManager.ins.Unregister(this);

    private void FixedUpdate()
    {
        // Convert from units/sec to units/frame to preserve existing tuning
        _velocity = _rb.linearVelocity * Time.fixedDeltaTime;

        var dragReduction = _aimVector.magnitude <= 0.1f ? movementSettings.DragStrength * Time.fixedDeltaTime : 0;
        var draggedVelocity = Vector3.MoveTowards(_velocity, Vector3.zero, dragReduction);
        var updatedVelocity = Vector3.MoveTowards(draggedVelocity,
            new Vector3(_aimVector.x, 0, _aimVector.y),
            movementSettings.Acceleration * Time.fixedDeltaTime);
        _velocity = Vector3.ClampMagnitude(updatedVelocity, movementSettings.MaxSpeed);

        if (_isBoosting)
        {
            _boostProgress += Time.fixedDeltaTime;
            if (_boostProgress > boostSettings.Duration)
            {
                _isBoosting = false;
            }

            var boostSpeed = boostSettings.SpeedCurve.Evaluate(Mathf.Clamp01(_boostProgress / boostSettings.Duration));
            _velocity += new Vector3(_aimVector.x, 0, _aimVector.y) * (boostSpeed * Time.fixedDeltaTime);
        }

        if (_timeUntilBoost > 0f)
        {
            _timeUntilBoost = Mathf.MoveTowards(_timeUntilBoost, 0f, Time.fixedDeltaTime);
        }

        _rb.linearVelocity = _velocity / Time.fixedDeltaTime;
        currentSpeed = _velocity.magnitude;
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

    public void AssertControlIntent(PlayerInputContext ctx)
    {
        m_controlHandler.ProcessIntent(ctx);
        _aimVector = ctx.MoveDirection;
        Inputs = ctx;
        if (!_isBoosting && _timeUntilBoost == 0f &&ctx.Boost)
        {
            _isBoosting = true;
            _boostProgress = 0;
            _timeUntilBoost = boostSettings.Cooldown;
            Debug.LogFormat("{0} is starting a boost", name);
        }
    }

    public void OnAction(PlayerAction action)
    {
        switch (action)
        {
            case PlayerAction.Interact: InteractPressed?.Invoke(); break;
        }
    }
}
