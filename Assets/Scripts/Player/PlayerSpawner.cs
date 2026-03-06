using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject m_playerPrefab;
    [SerializeField] private Transform[] m_spawnPoints;

    [Header("Despawn")]
    [Tooltip("Seconds before a player is automatically despawned. Set to 0 to disable.")]
    [SerializeField] private float m_despawnDelay = 180f;

    private readonly List<(PlayerCharacter player, int index, Coroutine coroutine)> m_active = new();

    private void OnEnable()  => InputManager.ins.PlayerJoinRequestEvent.AddListener(OnJoinRequest);
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

        var go = Instantiate(m_playerPrefab, spawnPoint.position, spawnPoint.rotation);
        var player = go.GetComponent<PlayerCharacter>();
        player.Init(index);

        var slot = InputManager.ins.Register(player, devices);

#if UNITY_EDITOR
        slot.DebugDespawnPressed += () => StartCoroutine(DespawnNextFrame(player));
#endif

        Coroutine coroutine = m_despawnDelay > 0f ? StartCoroutine(DespawnAfterDelay(player)) : null;
        m_active.Add((player, index, coroutine));
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
        for (int i = m_active.Count - 1; i >= 0; i--)
        {
            if (m_active[i].player == player)
            {
                if (m_active[i].coroutine != null)
                    StopCoroutine(m_active[i].coroutine);
                m_active.RemoveAt(i);
                break;
            }
        }


        if (player != null)
            Destroy(player.gameObject);
    }
}
