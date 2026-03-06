using System;
using System.Collections;
using System.Collections.Generic;
using Controllers;
using TMPro;
using UnityEngine;

public class FakeGameplay : MonoBehaviour
{
    private enum State
    {
        CantFish,
        CanFish,
        IsFishing,
    }

    [SerializeField] private FishingController fishingControllerPrefab;
    [SerializeField] private float speed = 5f;
    [SerializeField] private MeshRenderer model;
    [SerializeField] private TextMeshProUGUI statusText;

    private FishingController _fishingController;
    private State _currentState = State.CantFish;
    private GameObject _fishableShoal;

    private void OnEnable()
    {
        model.material.color = Color.red;
    }

    private void FixedUpdate()
    {
        if (_currentState == State.IsFishing)
        {
            return;
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * (speed * Time.deltaTime));
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * (speed * Time.deltaTime));
        }

        if (_currentState == State.CanFish && !_fishingController && Input.GetKeyDown(KeyCode.Space))
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

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[FakeGameplay] Entered trigger of {other.name}");
        if (other.CompareTag("Shoal") && _currentState == State.CantFish)
        {
            _fishableShoal = other.transform.parent.gameObject;
            Debug.Log("[FakeGameplay] Changing state to be able to fish");
            SetState(State.CanFish);
            model.material.color = Color.green;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"[FakeGameplay] Entered trigger of {other.name}");
        if (other.CompareTag("Shoal") && _currentState == State.CanFish)
        {
            _fishableShoal = null;
            Debug.Log("[FakeGameplay] Changing state to be unable to fish");
            SetState(State.CantFish);
            model.material.color = Color.red;
        }
    }

    private void SetState(State state)
    {
        if (state == _currentState)
        {
            return;
        }

        Debug.Log("[FakeGameplay] Setting state to " + Enum.GetName(typeof(State), _currentState));
        _currentState = state;
        statusText.text = StateStrings[_currentState];
    }

    private static readonly Dictionary<State, string> StateStrings = new()
    {
        { State.IsFishing, "FISHING" },
        { State.CantFish, "NO FISH" },
        { State.CanFish, "FISH HERE" },
    };

    // Some pretend time before kicking off the minigame
    private IEnumerator StartFishing()
    {
        SetState(State.IsFishing);
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
                SetState(State.CantFish);
                model.material.color = Color.red;
                Destroy(_fishableShoal);
                break;
            case FishingController.State.Failed:
                Debug.Log("[FakeGameplay] Fishing failed, you suck");
                Destroy(_fishingController.gameObject);
                SetState(State.CantFish);
                model.material.color = Color.red;
                break;
        }
    }
}
