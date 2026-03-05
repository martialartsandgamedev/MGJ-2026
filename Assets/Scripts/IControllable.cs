using UnityEngine;
public interface IControllable
{
    public string ControllableID { get; }
    public ControlHandler ControlHandler { get; }

    public void AssertControlIntent(PlayerInputContext ctx);
}
