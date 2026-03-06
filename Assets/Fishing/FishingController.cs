using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Controllers
{
    public class FishingController : MonoBehaviour
    {
        [SerializeField] private FishingSettings settings;
        [SerializeField] private DefaultFishingUIController uiController;

        public enum State
        {
            Initialising = 0,
            ReadyToStart,
            InProgress,
            Complete,
            Failed
        }

        public UnityEvent<State> onStateChanged;
        public State CurrentState { get; private set; }

        private double _elapsed;
        private List<FishingAction> _actions;

        private void OnEnable()
        {
            // Hookup inputs
            _actions = FishingAction.Create(settings);
            uiController.Initialise(_actions);
            SetState(State.ReadyToStart);
        }

        private void Update()
        {
            if (CurrentState != State.InProgress)
            {
                return;
            }

            _elapsed += Time.deltaTime / settings.Duration;
            uiController.SetProgress((float)_elapsed);
            if (_elapsed >= 1.0)
            {
                _elapsed -= 1.0;
                return;
            }

            if (!Input.GetKeyDown(KeyCode.Space)) return;

            var activeAction = GetActionInActiveWindow();
            if (activeAction != null)
            {
                activeAction.Attempt = FishingAction.AttemptState.Success;
                uiController.CompleteAction(activeAction.Index);
                Debug.Log($"[FishingController] Successfully hit action {activeAction.Index}");
                if (_actions.All(action => action.Attempt == FishingAction.AttemptState.Success))
                {
                    SetState(State.Complete);
                    uiController.SetProgress(1.0f);
                    Debug.Log("[FishingController] Successfully hit all actions");
                }
            }
            else
            {
                Debug.Log("[FishingController] Failed to hit action");
            }
        }

        public void StartFishing()
        {
            if (CurrentState != State.InProgress)
            {
                SetState(State.InProgress);
            }
        }

        private void SetState(State state)
        {
            if (state == CurrentState)
            {
                return;
            }

            Debug.Log($"[FishingController] Setting state to {Enum.GetName(typeof(State), state)}");
            CurrentState = state;
            onStateChanged.Invoke(CurrentState);
        }

        private FishingAction GetActionInActiveWindow()
        {
            return _actions.FirstOrDefault(action =>
                action.Attempt == FishingAction.AttemptState.Upcoming &&
                _elapsed >= action.StartTime &&
                _elapsed <= action.EndTime);
        }
    }
}
