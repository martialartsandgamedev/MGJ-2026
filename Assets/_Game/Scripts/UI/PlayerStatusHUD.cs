using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusHUD : MonoBehaviour
{
    [SerializeField] private PlayerSlotUI[] _slots;
    private Dictionary<PlayerCharacterController, PlayerSlotUI> _playerSlotUIs = new();
    private Dictionary<int, PlayerCharacterController> _characterForSlot = new();

    private void Awake()
    {
        for (int i = 0; i < _slots.Length; i++)
            _slots[i].SetEmpty(i);
    }

    private void OnEnable()
    {
        PlayerManager.Instance.OnCharacterSpawned += OnPlayerSpawned;
        PlayerManager.Instance.OnPlayerLeft += OnPlayerDespawned;
        PlayerInventory.OnInventoryChanged += OnInventoryChanged;
    }

    private void OnDisable()
    {
        if (PlayerManager.Instance == null) return;

        PlayerManager.Instance.OnCharacterSpawned -= OnPlayerSpawned;
        PlayerManager.Instance.OnPlayerLeft -= OnPlayerDespawned;
        PlayerInventory.OnInventoryChanged -= OnInventoryChanged;
    }

    private void OnPlayerSpawned(int slot, PlayerCharacterController player)
    {
        if (!IsValidSlot(slot)) return;
        _slots[slot].SetActive($"Player {slot + 1}");
        _playerSlotUIs[player] = _slots[slot];
        _characterForSlot[slot] = player;
        player.boostCooldownChanged.AddListener(OnBoostCooldownChanged);
    }

    private void OnPlayerDespawned(int slot)
    {
        if (!IsValidSlot(slot)) return;
        _slots[slot].SetEmpty(slot);

        if (_characterForSlot.TryGetValue(slot, out var player))
        {
            _playerSlotUIs.Remove(player);
            _characterForSlot.Remove(slot);
        }
    }

    private void OnInventoryChanged(int playerIndex, PlayerInventory inventory)
    {
        if (!IsValidSlot(playerIndex)) return;
        _slots[playerIndex].UpdateInventory(inventory);
    }

    private void OnBoostCooldownChanged(PlayerCharacterController player, float remainingCooldown)
    {
        // Debug.Log($"Boosting {remainingCooldown}");
        _playerSlotUIs[player].SetBoostProgress(remainingCooldown);
    }

    private bool IsValidSlot(int playerIndex) => playerIndex >= 0 && playerIndex < _slots.Length;
}
