using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private PlayerSpawner playerSpawner;
    private Dictionary<int, PlayerCharacterController> _characters = new();

    public IReadOnlyDictionary<int, PlayerCharacterController> Characters => _characters;

    private void Start()
    {
        _characters = playerSpawner.SpawnPlayers(PlayerManager.Instance.GetActivePlayers());
    }
}