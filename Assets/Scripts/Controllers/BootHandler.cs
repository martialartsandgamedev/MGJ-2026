using UnityEngine;

public class BootHandler : MonoBehaviour
{
    private void OnEnable()
    {
        Boot();
    }

    private async void Boot()
    {
        SceneHandler.Ins.LoadSceneFromAddress("scene_mainmenu");
    }
}
