using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusHUD : MonoBehaviour
{
    [SerializeField] private PlayerSpawner _spawner;
    [SerializeField] private PlayerSlotUI[] _slots;
    private Dictionary<PlayerCharacter, PlayerSlotUI> _playerSlotUIs = new();

    private void Awake()
    {
        for (int i = 0; i < _slots.Length; i++)
            _slots[i].SetEmpty(i);
    }

    private void OnEnable()
    {
        _spawner.PlayerSpawned.AddListener(OnPlayerSpawned);
        _spawner.PlayerDespawned.AddListener(OnPlayerDespawned);
        PlayerInventory.OnInventoryChanged += OnInventoryChanged;
    }

    private void OnDisable()
    {
        _spawner.PlayerSpawned.RemoveListener(OnPlayerSpawned);
        _spawner.PlayerDespawned.RemoveListener(OnPlayerDespawned);
        PlayerInventory.OnInventoryChanged -= OnInventoryChanged;
    }

    private void OnPlayerSpawned(int playerIndex, PlayerCharacter player)
    {
        if (!IsValidSlot(playerIndex)) return;
        _slots[playerIndex].SetActive($"Player {playerIndex + 1}");
        _playerSlotUIs[player] = _slots[playerIndex];
        player.boostCooldownChanged.AddListener(OnBoostCooldownChanged);
    }

    private void OnPlayerDespawned(int playerIndex, PlayerCharacter player)
    {
        if (!IsValidSlot(playerIndex)) return;
        _slots[playerIndex].SetEmpty(playerIndex);
        _playerSlotUIs.Remove(player);
    }

    private void OnInventoryChanged(int playerIndex, PlayerInventory inventory)
    {
        if (!IsValidSlot(playerIndex)) return;
        _slots[playerIndex].UpdateInventory(inventory);
    }

    private void OnBoostCooldownChanged(PlayerCharacter player, float remainingCooldown)
    {
        Debug.Log($"Boosting {remainingCooldown}");
        _playerSlotUIs[player].SetBoostProgress(remainingCooldown);
    }

    private bool IsValidSlot(int playerIndex) => playerIndex >= 0 && playerIndex < _slots.Length;
}
