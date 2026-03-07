using System;
using System.Collections;
using System.Collections.Generic;
using Controllers;
using System.Linq;
using TMPro;
using UnityEngine;

/**
 * Fishing Controller is in charge of the player's interaction with fishing spots,
 * owning the progression of the minigame and managing the UI
 */
public class FishingController : MonoBehaviour
{
    public enum PlayerState
    {
        CantFish,
        CanFish,
        IsFishing,
    }

    [SerializeField] private PlayerCharacter _playerCharacter;

    [Header("Debug")]
    [Tooltip("A MeshRenderer that acts as a debug way to show that the player is over a fishing spot")]
    [SerializeField]
    private MeshRenderer model;

    [Tooltip("Optional TMP Text that displays the players current state")] [SerializeField]
    private TextMeshProUGUI statusText;

    private List<FishingAction> _actions;
    private FishingSpot _activeFishingSpot;
    private PlayerState m_currentPlayerState = PlayerState.CantFish;
    private bool m_interactPending;
    private Canvas _playerCanvas;
    private DefaultFishingWidget _uiController;
    private Coroutine _fishingCoroutine;

    private void OnEnable()
    {
        model.material.color = Color.red;
        _playerCharacter.InteractPressed += OnInteract;
    }

    private void OnDisable()
    {
        _playerCharacter.InteractPressed -= OnInteract;
    }

    private void OnInteract() => m_interactPending = true;

    private bool ConsumeInteract()
    {
        var val = m_interactPending;
        m_interactPending = false;
        return val;
    }

    private void Update()
    {
        // If we are already fishing, don't permit movement
        if (m_currentPlayerState == PlayerState.IsFishing)
        {
            return;
        }

        // Trigger the fishing attempt
        if (m_currentPlayerState == PlayerState.CanFish && _fishableSpots.Any() && ConsumeInteract())
        {
            Debug.Log("[FakeGameplay] Creating fishing controller");
            _fishingCoroutine = StartCoroutine(TryFishActive());
        }
    }

    private void LateUpdate()
    {
        m_interactPending = false;
    }

    private readonly HashSet<FishingSpot> _fishableSpots = new();

    // Tell this fishing controller whether if can or cannot fish from a spot
    public void SetCanFishSpot(FishingSpot spot, bool canFish)
    {
        if (canFish)
        {
            _fishableSpots.Add(spot);
        }
        else
        {
            _fishableSpots.Remove(spot);
        }

        // If we are fishing and our spot has been restricted to us, stop all fishing activities and cleanup UI
        if (!canFish && _activeFishingSpot == spot)
        {
            _activeFishingSpot.OnFishingSpotDepleted.RemoveListener(OnFishingSpotDepleted);
            if (_fishingCoroutine != null)
            {
                StopCoroutine(_fishingCoroutine);
            }

            if (_uiController)
            {
                Destroy(_uiController.gameObject);
            }

            _activeFishingSpot = null;
            SetState(_fishableSpots.Any() ? PlayerState.CanFish : PlayerState.CantFish);
        }

        // Determine if we can start fishing
        if (m_currentPlayerState != PlayerState.IsFishing)
        {
            SetState(_fishableSpots.Any() ? PlayerState.CanFish : PlayerState.CantFish);
        }
    }

    private void OnFishingSpotDepleted()
    {
        _fishableSpots.Remove(_activeFishingSpot);
        _activeFishingSpot = null;
        SetState(PlayerState.CantFish);
    }

    public void SetState(PlayerState playerState)
    {
        if (playerState == m_currentPlayerState)
        {
            return;
        }

        Debug.Log("[FakeGameplay] Setting state to " + Enum.GetName(typeof(PlayerState), playerState));
        m_currentPlayerState = playerState;
        if (statusText) statusText.text = StateStrings[m_currentPlayerState];
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
        _activeFishingSpot = _fishableSpots.First();
        _activeFishingSpot.OnFishingSpotDepleted.AddListener(OnFishingSpotDepleted);
        SetState(PlayerState.IsFishing);

        FishingSpot spot = _activeFishingSpot;
        _actions = FishingAction.Create(spot.context);

        _uiController = Instantiate(spot.context.UIWidget, _playerCanvas.transform);
        _uiController.Initialise(spot, _actions);

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
            bool interacted = ConsumeInteract();
            if (interacted && activeAction == null)
            {
                Debug.Log("[FishingController] Failed to hit action");
            }
            else if (interacted && activeAction != null)
            {
                activeAction.Attempt = FishingAction.AttemptState.Success;

                _uiController.CompleteAction(activeAction.Index);

                Debug.Log($"[FishingController] Successfully hit action {activeAction.Index}");
            }

            // Check if all actions have now been completed
            if (_actions.All(action => action.Attempt == FishingAction.AttemptState.Success))
            {
                Debug.Log("[FishingController] Successfully hit all actions");
                SetState(PlayerState.CanFish);

                var fish = _activeFishingSpot.RemoveStock();

                ScoreManager.ins?.ReportCatch(_playerCharacter.PlayerIndex, fish);
               
                _activeFishingSpot = null;
                Destroy(_uiController.gameObject);
                yield break;
            }

            elapsed += Time.deltaTime / spot.context.Duration;
            _uiController.SetProgress(elapsed);

            // Loop the elapsed timer
            if (elapsed >= 1.0f)
            {
                elapsed -= 1.0f;
            }

            yield return null;
        }
    }
}
