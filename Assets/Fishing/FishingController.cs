using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Controllers
{
    public class FishingController : MonoBehaviour
    {
        [SerializeField] private FishingSettings settings;
        [SerializeField] private DefaultFishingUIController uiController;

        private double _elapsed;
        private List<FishingAction> _actions;

        private void OnEnable()
        {
            // Hookup inputs
            _actions = FishingAction.Create(settings);
            uiController.Initialise(_actions);
        }

        private FishingAction GetActionInActiveWindow()
        {
            return _actions.FirstOrDefault(action =>
                action.Attempt == FishingAction.AttemptState.Upcoming &&
                _elapsed >= action.StartTime &&
                _elapsed <= action.EndTime);
        }

        private void OnDisable()
        {
        }

        private void Update()
        {
            _elapsed += Time.deltaTime / settings.Duration;
            uiController.SetProgress((float)_elapsed);
            if (_elapsed >= 1.0)
            {
                Debug.Log("Ran out of time, running it back?");
                _elapsed -= 1.0;
                return;
            }

            if (!Input.GetKeyDown(KeyCode.Space)) return;

            var activeAction = GetActionInActiveWindow();
            if (activeAction != null)
            {
                activeAction.Attempt = FishingAction.AttemptState.Success;
                uiController.CompleteAction(activeAction.Index);
                Debug.Log($"Successfully hit action {activeAction.Index}");
                if (_actions.All(action => action.Attempt == FishingAction.AttemptState.Success))
                {
                    Debug.Log("Successfully hit all actions");
                }
            }
            else
            {
                Debug.Log("Failed to hit action");
            }
        }
    }
}
