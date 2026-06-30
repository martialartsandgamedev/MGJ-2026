using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject m_playerPrefab;
    [SerializeField] private Transform[] m_spawnPoints;

    public Dictionary<int, PlayerCharacterController> SpawnPlayers(IReadOnlyDictionary<int, Player> activePlayers)
    {
        Dictionary<int, PlayerCharacterController> characters = new();
        foreach (var kvp in activePlayers)
        {
            int slotIndex = kvp.Key;
            Player player = kvp.Value;

            if (slotIndex < 0 || slotIndex >= m_spawnPoints.Length)
            {
                Debug.LogError($"No spawn point for slot {slotIndex}", this);
                continue;
            }

            var spawnPoint = m_spawnPoints[slotIndex].position;

            var character = PlayerManager.Instance.SpawnCharacter(slotIndex, m_playerPrefab, spawnPoint);
            characters.Add(slotIndex, character);
        }

        return characters;
    }
}
