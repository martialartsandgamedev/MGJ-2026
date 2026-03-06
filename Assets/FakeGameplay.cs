using System;
using System.Collections;
using Controllers;
using UnityEngine;

public class FakeGameplay : MonoBehaviour
{
    [SerializeField] private FishingController fishingControllerPrefab;

    private FishingController _fishingController;

    private void Update()
    {
        if (!_fishingController && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("[FakeGameplay] Creating fishing controller");
            _fishingController = Instantiate(fishingControllerPrefab);
            _fishingController.onStateChanged.AddListener(OnFishingStateChange);
            if (_fishingController.CurrentState == FishingController.State.ReadyToStart)
            {
                StartCoroutine(StartFishing());
            }
        }
    }

    // Some pretend time before kicking off the minigame
    private IEnumerator StartFishing()
    {
        Debug.Log("[FakeGameplay] Controller waiting, will start soon");
        yield return new WaitForSeconds(2);
        Debug.Log("[FakeGameplay] Finished waiting, starting fishing");
        _fishingController.StartFishing();
    }

    private void OnFishingStateChange(FishingController.State state)
    {
        Debug.Log($"[FakeGameplay] Controller changed state to {Enum.GetName(typeof(FishingController.State), state)}");
        switch (state)
        {
            case FishingController.State.Complete:
                Debug.Log("[FakeGameplay] Fishing complete");
                Destroy(_fishingController.gameObject);
                break;
            case FishingController.State.Failed:
                Debug.Log("[FakeGameplay] Fishing failed, you suck");
                Destroy(_fishingController.gameObject);
                break;
        }
    }
}
