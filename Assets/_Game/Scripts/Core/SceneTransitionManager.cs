using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent singleton. The only class permitted to call SceneManager.LoadScene.
/// </summary>
/// 
public class SceneTransitionManager : PersistentSingleton<SceneTransitionManager>
{
    [Header("Scene Names")]
#if UNITY_EDITOR
    [SerializeField] private UnityEditor.SceneAsset lobbySceneAsset;
    [SerializeField] private UnityEditor.SceneAsset gameSceneAsset;
#endif
    [SerializeField] private string lobbyScene;
    [SerializeField] private string gameScene;

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (lobbySceneAsset != null) lobbyScene = lobbySceneAsset.name;
        if (gameSceneAsset != null) gameScene = gameSceneAsset.name;
#endif
    }

    private bool _transitioning;

    public void GoToGame()
    {
        if (_transitioning) return;
        _transitioning = true;
        SceneManager.LoadScene(gameScene);
        _transitioning = false;
    }

    public void GoToLobby()
    {
        if (_transitioning) return;
        _transitioning = true;
        SceneManager.LoadScene(lobbyScene);
        _transitioning = false;
    }
}