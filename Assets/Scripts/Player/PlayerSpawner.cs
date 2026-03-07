using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private PlayerCharacter m_playerPrefab;
    [SerializeField] private Transform[] m_spawnPoints;

    [Header("Events")] public UnityEvent<int> PlayerSpawned;
    public UnityEvent<int> PlayerDespawned;

    [Header("Despawn")]
    [Tooltip("Seconds before a player is automatically despawned. Set to 0 to disable.")]
    [SerializeField]
    private float m_despawnDelay = 180f;

    private readonly List<(PlayerCharacter player, int index, Coroutine coroutine)> m_active = new();

    private void OnEnable() => InputManager.ins.PlayerJoinRequestEvent.AddListener(OnJoinRequest);
    private void OnDisable() => InputManager.ins.PlayerJoinRequestEvent.RemoveListener(OnJoinRequest);

    private int GetNextAvailableIndex()
    {
        int i = 0;
        while (m_active.Exists(e => e.index == i)) i++;
        return i;
    }

    private void OnJoinRequest(InputDevice[] devices)
    {
        int index = GetNextAvailableIndex();
        var spawnPoint = m_spawnPoints != null && m_spawnPoints.Length > 0
            ? m_spawnPoints[index % m_spawnPoints.Length]
            : transform;

        var playerInstance = Instantiate(m_playerPrefab, spawnPoint.position, spawnPoint.rotation);
        playerInstance.Init(index);
        var floatingUI = playerInstance.GetComponentInChildren<FloatingUI>();
        floatingUI.Init(devices);
        floatingUI.ShowPrompt("direction", 5f);

        var slot = InputManager.ins.Register(playerInstance, devices);

#if UNITY_EDITOR
        slot.DebugDespawnPressed += () => StartCoroutine(DespawnNextFrame(playerInstance));
#endif

        Coroutine coroutine = m_despawnDelay > 0f ? StartCoroutine(DespawnAfterDelay(playerInstance)) : null;
        m_active.Add((playerInstance, index, coroutine));
        PlayerSpawned?.Invoke(index);
    }

    private IEnumerator DespawnNextFrame(PlayerCharacter player)
    {
        yield return null;
        DespawnPlayer(player);
    }

    private IEnumerator DespawnAfterDelay(PlayerCharacter player)
    {
        yield return new WaitForSeconds(m_despawnDelay);
        DespawnPlayer(player);
    }

    private void DespawnPlayer(PlayerCharacter player)
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
            PlayerDespawned?.Invoke(despawnedIndex);

        if (player != null)
            Destroy(player.gameObject);
    }
}
