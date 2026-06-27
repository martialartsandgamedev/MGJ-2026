using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the lifecycle of players
/// </summary>
[RequireComponent(typeof(PlayerInputManager))]
public class PlayerManager : PersistentSingleton<PlayerManager>
{
    [Header("Config")]
    [SerializeField] private int maxPlayers = 4;

    private readonly Dictionary<int, Player> _players = new();
    private PlayerInputManager _playerInputManager;

    public event Action<int> OnPlayerJoined;
    public event Action<int> OnPlayerLeft;
    public event Action<int, PlayerCharacterController> OnCharacterSpawned;

    protected override void Awake()
    {
        base.Awake();

        _playerInputManager = GetComponent<PlayerInputManager>();
        _playerInputManager.onPlayerJoined += HandlePlayerJoined;
        _playerInputManager.onPlayerLeft += HandlePlayerLeft;
    }

    private void HandlePlayerJoined(PlayerInput input)
    {
        int slot = NextFreeSlot();
        if (slot == -1)
        {
            Destroy(input.gameObject);
            return;
        }

        _players[slot] = new Player(slot, input);

        DontDestroyOnLoad(input.gameObject);
        input.transform.SetParent(transform);
        input.name = $"PlayerInput_Slot_{slot}";

        var pc = input.GetComponent<CharacterInputManager>();
        if (pc != null) pc.Initialise(slot);

        OnPlayerJoined?.Invoke(slot);
    }

    private void HandlePlayerLeft(PlayerInput input)
    {
        int slot = SlotForInput(input);
        if (slot == -1)
        {
            return;
        }

        _players.Remove(slot);
        OnPlayerLeft?.Invoke(slot);
    }

    private int NextFreeSlot()
    {
        for (int i = 0; i < maxPlayers; i++)
        {
            if (!_players.ContainsKey(i))
            {
                return i;
            }
        }

        return -1;
    }

    private int SlotForInput(PlayerInput input)
    {
        foreach (var kvp in _players)
        {
            if (kvp.Value.Input == input)
            {

                return kvp.Key;
            }
        }

        return -1;
    }

    public void RemovePlayer(int slot)
    {
        if (!_players.TryGetValue(slot, out var player)) return;
        Destroy(player.Input.gameObject);
    }

    public IReadOnlyList<int> GetActiveSlots() => new List<int>(_players.Keys);

    public IReadOnlyDictionary<int, Player> GetActivePlayers() => _players;

    public PlayerInput GetInput(int slot) =>
        _players.TryGetValue(slot, out var player) ? player.Input : null;

    public PlayerCharacterController SpawnCharacter(int slot, GameObject prefab, Vector3 position)
    {
        var go = Instantiate(prefab, position, Quaternion.identity);

        var id = go.GetComponent<SlotIdentifier>();
        if (id == null)
        {
            id = go.AddComponent<SlotIdentifier>();
        }
        id.Initialise(slot);

        var character = go.GetComponent<PlayerCharacterController>();

        var input = GetInput(slot);
        if (input != null)
        {
            var controller = input.GetComponent<CharacterInputManager>();
            if (controller != null)
            {
                controller.AssignCharacter(character);
            }

            character.Init(slot, input.devices.ToArray());
        }

        OnCharacterSpawned?.Invoke(slot, character);
        return character;
    }
}

