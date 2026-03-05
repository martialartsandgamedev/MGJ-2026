using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

[ExecuteAlways]
public class SceneHandler : MonoBehaviour
{
    public static SceneHandler Ins = null;
    
    private AsyncOperationHandle m_loadedAreaHandle;
    
    private void Awake()
    {
        if (Ins == null)
        {
            Ins = this;
        }
    }
    
    public void UnloadScene(string name)
    {
        SceneManager.UnloadSceneAsync(name);
    }

    public async void LoadLevelFromAddress(string address)
    {
        //When loading areas - we specifically want to target the loaded area handle
        if (m_loadedAreaHandle.IsValid())
        {
            Addressables.UnloadSceneAsync(m_loadedAreaHandle);            
        }

        m_loadedAreaHandle= Addressables.LoadSceneAsync(address, LoadSceneMode.Additive);
        
        await m_loadedAreaHandle.Task;

        if (m_loadedAreaHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("Scene loaded successfully");
        }
        else
        {
            Debug.LogError("Scene load failed");
        }
    }
    
    public async void LoadSceneFromAddress(string address)
    {
        var loadHandle = Addressables.LoadSceneAsync(address, LoadSceneMode.Additive);


        await loadHandle.Task;

        if (loadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("Scene loaded successfully");
        }
        else
        {
            Debug.LogError("Scene load failed");
        }
    }
}