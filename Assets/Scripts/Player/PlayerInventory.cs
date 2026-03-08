using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Tracks a player's held fish and banked score.
/// Held fish are caught but not yet secured; banking converts them to points.
/// Everything resets when the player despawns (component is destroyed with the player GO).
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    /// <summary>Fired whenever held fish or banked score changes. Args: playerIndex, this inventory.</summary>
    public static event Action<int, PlayerInventory> OnInventoryChanged;

    public int PlayerIndex { get; private set; }

    public IReadOnlyList<Fish> HeldFish => _heldFish;
    public int HeldCount => _heldFish.Count;
    public int BankedScore { get; private set; }

    public int HeldScore
    {
        get
        {
            int total = 0;
            foreach (var fish in _heldFish)
                total += ScoreManager.CalculateFishScore(fish);
            return total;
        }
    }

    private readonly List<Fish> _heldFish = new();
    private PlayerCharacter _character;
    private bool _inBankZone;

    public void Init(int playerIndex)
    {
        PlayerIndex = playerIndex;
        _character = GetComponent<PlayerCharacter>();
    }

    public void NotifyEnterBankZone()
    {
        if (_inBankZone) return;
        _inBankZone = true;
        _character.InteractPressed += OnInteractInBankZone;
    }

    public void NotifyExitBankZone()
    {
        if (!_inBankZone) return;
        _inBankZone = false;
        _character.InteractPressed -= OnInteractInBankZone;
    }

    private void OnInteractInBankZone() => BankAll();

    /// <summary>Add a caught fish to the held inventory.</summary>
    public void AddFish(Fish fish)
    {
        _heldFish.Add(fish);
        OnInventoryChanged?.Invoke(PlayerIndex, this);
    }

    /// <summary>
    /// Convert all held fish into banked points.
    /// Call this when the player reaches a banking zone and confirms.
    /// </summary>
    public void BankAll()
    {
        if (_heldFish.Count == 0) return;

        foreach (var fish in _heldFish)
            BankedScore += ScoreManager.CalculateFishScore(fish);

        _heldFish.Clear();
        OnInventoryChanged?.Invoke(PlayerIndex, this);
    }

    /// <summary>
    /// Remove and return a random fish from the held inventory.
    /// Returns null if the inventory is empty.
    /// </summary>
    public Fish DropRandom()
    {
        if (_heldFish.Count == 0) return null;

        int index = Random.Range(0, _heldFish.Count);
        var fish = _heldFish[index];
        _heldFish.RemoveAt(index);
        OnInventoryChanged?.Invoke(PlayerIndex, this);
        return fish;
    }
}
