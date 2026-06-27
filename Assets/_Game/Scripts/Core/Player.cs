using UnityEngine.InputSystem;

/// <summary>
/// Single model representing one connected player.
/// Replaces the previous PlayerData/PlayerInput/PlayerGameplayData split
/// across three parallel dictionaries in PlayerManager — everything about
/// a connected player now lives in one object, keyed once.
///
/// Does not include ready state — that's lobby-specific and owned by
/// LobbyManager, since it has no meaning outside the lobby scene.
///
/// Owned and mutated only by PlayerManager.
/// </summary>
public class Player
{
    public int Slot { get; }
    public PlayerInput Input { get; }
    public CharacterInputManager Controller { get; }

    public Player(int slot, PlayerInput input)
    {
        Slot = slot;
        Input = input;
        Controller = input.GetComponent<CharacterInputManager>();
    }
}