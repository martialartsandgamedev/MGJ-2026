using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject m_playerPrefab;
    [SerializeField] private Transform[] m_spawnPoints;

    [Header("Events")]
    public UnityEvent<int, PlayerCharacterController> PlayerSpawned;
    public UnityEvent<int, PlayerCharacterController> PlayerDespawned;

    [Header("Despawn")]
    [Tooltip("Seconds before a player is automatically despawned. Set to 0 to disable.")]
    [SerializeField] private float m_despawnDelay = 180f;

    private readonly List<(PlayerCharacterController player, int index, Coroutine coroutine)> m_active = new();

    public void OnPlayerJoinedEvent(PlayerInput input)
    {
        Debug.Log("Player joined");
        OnJoinRequest(input.devices.ToArray());
    }

    public void OnPlayerLeftEvent(PlayerInput input)
    {
        Debug.Log("Player left");
    }

    private int GetNextAvailableIndex()
    {
        int i = 0;
        while (m_active.Exists(e => e.index == i)) i++;
        return i;
    }

    public void OnJoinRequest(InputDevice[] devices)
    {
        int index = GetNextAvailableIndex();
        var spawnPoint = m_spawnPoints != null && m_spawnPoints.Length > 0
            ? m_spawnPoints[index % m_spawnPoints.Length]
            : transform;

        var go = Instantiate(m_playerPrefab, spawnPoint.position, spawnPoint.rotation);
        var player = go.GetComponent<PlayerCharacterController>();
        player.Init(index, devices);


        Coroutine coroutine = m_despawnDelay > 0f ? StartCoroutine(DespawnAfterDelay(player)) : null;
        m_active.Add((player, index, coroutine));
        PlayerSpawned?.Invoke(index, player);
    }

    public IEnumerator DespawnNextFrame(PlayerCharacterController player)
    {
        yield return null;
        DespawnPlayer(player);
    }

    private IEnumerator DespawnAfterDelay(PlayerCharacterController player)
    {
        yield return new WaitForSeconds(m_despawnDelay);
        DespawnPlayer(player);
    }

    private void DespawnPlayer(PlayerCharacterController player)
    {
        int despawnedIndex = -1;
        for (int i = m_active.Count - 1; i >= 0; i--)
        {
            if (m_active[i].player == player)
            {
                if (m_active[i].coroutine != null)
                    StopCoroutine(m_active[i].coroutine);
                despawnedIndex = m_active[i].index;
                m_active.RemoveAt(i);
                break;
            }
        }

        if (despawnedIndex >= 0)
            PlayerDespawned?.Invoke(despawnedIndex, player);

        if (player != null)
            Destroy(player.gameObject);
    }
}
