using UnityEngine;

public class PlayerCharacter : MonoBehaviour, IControllable
{
    [SerializeField] private ControlHandler m_controlHandler;
    [SerializeField] private float currentSpeed;
    [SerializeField] private MovementSettings movementSettings;

    private string m_id;
    private Vector2 _aimVector;
    private Vector3 _velocity;
    private Vector2 _moveVector;
    public string ControllableID => m_id;

    public void Init(int playerIndex)
    {
        m_id = $"Player {playerIndex + 1}";
        gameObject.name = m_id;
    }

    public ControlHandler ControlHandler => m_controlHandler;

    private void OnDisable() => InputManager.ins.Unregister(this);

    private void FixedUpdate()
    {
        var dragReduction = _aimVector.magnitude <= 0.1f ? movementSettings.DragStrength * Time.fixedDeltaTime : 0;
        var draggedVelocity = Vector3.MoveTowards(_velocity, Vector3.zero, dragReduction);
        var updatedVelocity = Vector3.MoveTowards(draggedVelocity,
            new Vector3(_aimVector.x, 0, _aimVector.y),
            movementSettings.Acceleration * Time.fixedDeltaTime);
        _velocity = Vector3.ClampMagnitude(updatedVelocity, movementSettings.MaxSpeed);
        transform.position += _velocity;
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
        if (ctx.MoveDirection.magnitude > 0.1f)
            Debug.Log($"{m_id} moving: {ctx.MoveDirection}");
        m_controlHandler.ProcessIntent(ctx);
        _aimVector = ctx.MoveDirection;
    }
}
