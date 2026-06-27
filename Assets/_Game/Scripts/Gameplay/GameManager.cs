using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerSpawner playerSpawner;

    private void Start()
    {
        playerSpawner.SpawnPlayers(PlayerManager.Instance.GetActivePlayers());
    }
}