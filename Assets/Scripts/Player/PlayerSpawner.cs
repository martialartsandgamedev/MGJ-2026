using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject m_playerPrefab;
    [SerializeField] private Transform[] m_spawnPoints;

    private void OnEnable()  => InputManager.ins.PlayerJoinRequestEvent.AddListener(OnJoinRequest);
    private void OnDisable() => InputManager.ins.PlayerJoinRequestEvent.RemoveListener(OnJoinRequest);

    private void OnJoinRequest(InputDevice[] devices)
    {
        int index = InputManager.ins.SlotCount;
        var spawnPoint = m_spawnPoints != null && m_spawnPoints.Length > 0
            ? m_spawnPoints[index % m_spawnPoints.Length]
            : transform;

        var go = Instantiate(m_playerPrefab, spawnPoint.position, spawnPoint.rotation);
        var player = go.GetComponent<PlayerCharacter>();
        player.Init(index);

        InputManager.ins.Register(player, devices);
    }
}
