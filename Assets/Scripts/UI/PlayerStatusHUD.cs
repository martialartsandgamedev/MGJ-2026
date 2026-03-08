using UnityEngine;

public class PlayerStatusHUD : MonoBehaviour
{
    [SerializeField] private PlayerSpawner _spawner;
    [SerializeField] private PlayerSlotUI[] _slots;

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

    private void OnPlayerSpawned(int playerIndex)
    {
        if (!IsValidSlot(playerIndex)) return;
        _slots[playerIndex].SetActive($"Player {playerIndex + 1}");
    }

    private void OnPlayerDespawned(int playerIndex)
    {
        if (!IsValidSlot(playerIndex)) return;
        _slots[playerIndex].SetEmpty(playerIndex);
    }

    private void OnInventoryChanged(int playerIndex, PlayerInventory inventory)
    {
        if (!IsValidSlot(playerIndex)) return;
        _slots[playerIndex].UpdateInventory(inventory);
    }

    private bool IsValidSlot(int playerIndex) => playerIndex >= 0 && playerIndex < _slots.Length;
}
