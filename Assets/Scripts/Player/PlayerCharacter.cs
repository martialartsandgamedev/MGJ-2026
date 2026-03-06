using UnityEngine;

public class PlayerCharacter : MonoBehaviour, IControllable
{
    [SerializeField] private ControlHandler m_controlHandler;

    private string m_id;
    public string ControllableID => m_id;

    public void Init(int playerIndex)
    {
        m_id = $"Player {playerIndex + 1}";
        gameObject.name = m_id;
    }
    public ControlHandler ControlHandler => m_controlHandler;

    private void OnDisable() => InputManager.ins.Unregister(this);

    public void AssertControlIntent(PlayerInputContext ctx)
    {
        if (ctx.MoveDirection.magnitude > 0.1f)
            Debug.Log($"{m_id} moving: {ctx.MoveDirection}");
        m_controlHandler.ProcessIntent(ctx);
    }
}