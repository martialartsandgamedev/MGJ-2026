using System;
using System.Collections;
using System.Collections.Generic;
using Controllers;
using System.Linq;
using TMPro;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public enum PlayerState
    {
        CantFish,
        CanFish,
        IsFishing,
    }

    [SerializeField] private float speed = 5f;
    [SerializeField] private MeshRenderer model;
    [SerializeField] private TextMeshProUGUI statusText;

    private List<FishingAction> _actions;
    private FishingSpot _activeFishingSpot;
    private PlayerState m_currentPlayerState = PlayerState.CantFish;

    private void OnEnable()
    {
        model.material.color = Color.red;
    }

    private void Update()
    {
        // If we are already fishing, don't permit movement
        if (m_currentPlayerState == PlayerState.IsFishing)
        {
            return;
        }

        // Pretend movement to test trigger zones
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * (speed * Time.deltaTime));
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * (speed * Time.deltaTime));
        }

        // Trigger the fishing attempt
        if (m_currentPlayerState == PlayerState.CanFish && _activeFishingSpot && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("[FakeGameplay] Creating fishing controller");
            StartCoroutine(TryFishActive());
        }
    }

    public void SetActiveFishingSpot(FishingSpot spot)
    {
        _activeFishingSpot = spot;
        var newState = spot == null ? PlayerState.CantFish : PlayerState.CanFish;
        SetState(newState);
    }

    public void SetState(PlayerState playerState)
    {
        if (playerState == m_currentPlayerState)
        {
            return;
        }

        Debug.Log("[FakeGameplay] Setting state to " + Enum.GetName(typeof(PlayerState), m_currentPlayerState));

        m_currentPlayerState = playerState;
        statusText.text = StateStrings[m_currentPlayerState];
        switch (playerState)
        {
            case PlayerState.CantFish:
                model.material.color = Color.red;
                break;
            case PlayerState.CanFish:
                model.material.color = Color.yellow;
                break;
            case PlayerState.IsFishing:
                model.material.color = Color.green;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(playerState), playerState, null);
        }
    }

    private static readonly Dictionary<PlayerState, string> StateStrings = new()
    {
        { PlayerState.IsFishing, "FISHING" },
        { PlayerState.CantFish, "NO FISH" },
        { PlayerState.CanFish, "FISH HERE" },
    };

    private FishingAction GetActionInActiveWindow(float elapsed) =>
        _actions.FirstOrDefault(action =>
            action.Attempt == FishingAction.AttemptState.Upcoming
            && elapsed >= action.StartTime
            && elapsed <= action.EndTime);

    private IEnumerator TryFishActive()
    {
        SetState(PlayerState.IsFishing);

        FishingSpot spot = _activeFishingSpot;
        _actions = FishingAction.Create(spot.context);

        var uiController = Instantiate(spot.context.UIWidget);
        uiController.Initialise(spot, _actions);

        float elapsed = 0;

        Debug.LogFormat($"{this} started a new fishing attempt on {spot}");

        // TODO: Confirm if this wait is needed
        yield return new WaitForEndOfFrame();

        // Loop until all actions are successful
        while (_actions.Any(action => action.Attempt != FishingAction.AttemptState.Success))
        {
            // Find action that we are in the window of
            var activeAction = GetActionInActiveWindow(elapsed);

            // If we are in an action window
            if (activeAction != null)
            {
                Debug.Log($"[FakeGameplay] Action {activeAction.Index} is still active");
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    activeAction.Attempt = FishingAction.AttemptState.Success;

                    uiController.CompleteAction(activeAction.Index);

                    Debug.Log($"[FishingController] Successfully hit action {activeAction.Index}");
                }
                else
                {
                    Debug.Log("[FishingController] Failed to hit action");
                }
            }

            // Check if all actions have now been completed
            if (_actions.All(action => action.Attempt == FishingAction.AttemptState.Success))
            {
                uiController.SetProgress(1.0f);
                Debug.Log("[FishingController] Successfully hit all actions");
                yield break;
            }

            elapsed += Time.deltaTime / spot.context.Duration;
            uiController.SetProgress(elapsed);

            // Loop the elapsed timer
            if (elapsed >= 1.0f)
            {
                elapsed -= 1.0f;
            }

            yield return null;
        }
    }
}
