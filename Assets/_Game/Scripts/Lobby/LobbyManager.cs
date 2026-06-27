using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Scene-level coordinator for the lobby.
/// Ready state comes from players standing in a PlayerTriggerZone (wired in the
/// Inspector to OnPlayerEnteredReadyZone/OnPlayerExitedReadyZone), not from an
/// input button — see PlayerTriggerZone.
///
/// Owns ready state directly. PlayerManager has no concept of "ready" —
/// it's a lobby-only idea, and tracking it here means it naturally resets
/// every time the lobby scene reloads (e.g. returning from a finished game),
/// with no separate reset call needed.
///
/// Spawn flow:
///   Start() spawns characters for already-connected players (returning from game).
///   HandlePlayerJoined() spawns characters for new joins after Start.
///   OnEnable() subscribes events only — never spawns, to avoid racing with Start().
/// </summary>
public class LobbyManager : Singleton<LobbyManager>
{
    [Header("Spawning")]
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private Transform[] spawnPoints;

    private readonly Dictionary<int, GameObject> _characters = new();
    private bool _transitioning;

    private void OnEnable()
    {
        PlayerInputManager.instance.EnableJoining();
        PlayerManager.Instance.OnPlayerJoined += HandlePlayerJoined;
        PlayerManager.Instance.OnPlayerLeft += HandlePlayerLeft;

        // Subscribe PlayerController events for players already connected
        // (returning from game scene). New joins are handled in HandlePlayerJoined.
        foreach (var slot in PlayerManager.Instance.GetActiveSlots())
            SubscribeToPlayerController(slot);
    }

    private void OnDisable()
    {
        if (PlayerManager.Instance == null) return;

        PlayerInputManager.instance.DisableJoining();
        PlayerManager.Instance.OnPlayerJoined -= HandlePlayerJoined;
        PlayerManager.Instance.OnPlayerLeft -= HandlePlayerLeft;

        foreach (var slot in PlayerManager.Instance.GetActiveSlots())
            UnsubscribeFromPlayerController(slot);
    }

    private void Start()
    {
        // Spawn characters for already-connected players.
        // OnEnable has already subscribed events, so new joins after this
        // point are handled by HandlePlayerJoined — no double-spawn risk.
        foreach (var slot in PlayerManager.Instance.GetActiveSlots())
        {
            SpawnCharacterForSlot(slot);
            LobbyUI.Instance?.SetSlotOccupied(slot, true);
        }
    }

    // -------------------------------------------------------------------------
    // PlayerManager event handlers
    // -------------------------------------------------------------------------

    private void HandlePlayerJoined(int slot)
    {
        // Only fires for joins that happen after Start() — safe to spawn here
        SubscribeToPlayerController(slot);
        SpawnCharacterForSlot(slot);
        LobbyUI.Instance?.SetSlotOccupied(slot, true);
    }

    private void HandlePlayerLeft(int slot)
    {
        UnsubscribeFromPlayerController(slot);
        DestroyCharacterForSlot(slot);
        readyPlayers.Remove(slot);
        LobbyUI.Instance?.SetSlotOccupied(slot, false);
    }

    // -------------------------------------------------------------------------
    // PlayerController event handlers
    // -------------------------------------------------------------------------

    private readonly HashSet<int> readyPlayers = new();

    public void OnPlayerEnteredReadyZone(SlotIdentifier slot)
    {
        readyPlayers.Add(slot.Slot);
        LobbyUI.Instance?.SetSlotReady(slot.Slot, true);
        CheckAllReady();
    }

    public void OnPlayerExitedReadyZone(SlotIdentifier slot)
    {
        readyPlayers.Remove(slot.Slot);
        LobbyUI.Instance?.SetSlotReady(slot.Slot, false);
    }

    private void CheckAllReady()
    {
        if (_transitioning) return;

        var activeSlots = PlayerManager.Instance.GetActiveSlots();

        if (activeSlots.Count == 0)
            return;

        foreach (var slot in activeSlots)
        {
            if (!readyPlayers.Contains(slot))
                return;
        }

        _transitioning = true;
        SceneTransitionManager.Instance.GoToGame();
    }

    private void HandleLeave(int slot)
    {
        if (_transitioning) return;
        PlayerManager.Instance.RemovePlayer(slot);
    }

    // -------------------------------------------------------------------------

    private void SubscribeToPlayerController(int slot)
    {
        var pc = GetPlayerController(slot);
        if (pc == null) return;
        pc.OnLeavePressed += HandleLeave;
    }

    private void UnsubscribeFromPlayerController(int slot)
    {
        var pc = GetPlayerController(slot);
        if (pc == null) return;
        pc.OnLeavePressed -= HandleLeave;
    }

    private CharacterInputManager GetPlayerController(int slot) =>
        PlayerManager.Instance.GetInput(slot)?.GetComponent<CharacterInputManager>();

    private void SpawnCharacterForSlot(int slot)
    {
        if (_characters.ContainsKey(slot)) return;

        Transform spawn = spawnPoints.Length > slot ? spawnPoints[slot] : transform;
        var character = PlayerManager.Instance.SpawnCharacter(slot, characterPrefab, spawn.position);
        _characters[slot] = character.gameObject;
    }

    private void DestroyCharacterForSlot(int slot)
    {
        if (!_characters.TryGetValue(slot, out var go)) return;
        Destroy(go);
        _characters.Remove(slot);
    }
}
