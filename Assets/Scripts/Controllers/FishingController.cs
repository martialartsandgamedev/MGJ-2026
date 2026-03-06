using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Controllers
{
    [Serializable]
    public record FishingAction
    {
        public enum AttemptState
        {
            Upcoming,
            Missed,
            Failed,
            Success
        }

        public int Index { get; set; }
        public AttemptState Attempt { get; set; }
        public double StartTime { get; set; }
        public double EndTime { get; set; }

        public static List<FishingAction> Create(FishingSettings settings)
        {
            return Enumerable.Range(0, settings.ActionCount).Select(index =>
            {
                var targetTime = settings.Duration / (settings.ActionCount + 1) * (index + 1);
                var startTime = targetTime - settings.Buffer;
                var endTime = targetTime + settings.Buffer;
                var normalisedStartTime = startTime / settings.Duration;
                var normalisedEndTime = endTime / settings.Duration;

                return new FishingAction
                {
                    Index = index,
                    StartTime = normalisedStartTime,
                    EndTime = normalisedEndTime,
                    Attempt = AttemptState.Upcoming,
                };
            }).ToList();
        }
    }

    public class FishingController : MonoBehaviour
    {
        [SerializeField] public FishingSettings settings;

        [SerializeField] public RectTransform actionUITemplate;
        [SerializeField] public RectTransform actionProgress;

        private double _elapsed;
        private List<FishingAction> _actions;
        private Dictionary<int, RectTransform> _actionUI;
        private float _width;

        private void OnEnable()
        {
            // Hookup inputs
            // Render widget
            // Initialise minigame
            // Start
            _actions = FishingAction.Create(settings);
            InitUI();
        }


        private void InitUI()
        {
            _width = actionProgress.rect.width;

            _actionUI = new Dictionary<int, RectTransform>();
            var parent = actionUITemplate.transform.parent;
            foreach (var action in _actions)
            {
                var actionUI = Instantiate(actionUITemplate, parent, true);
                var normalisedWidth = (action.EndTime - action.StartTime);
                var width = normalisedWidth * _width;
                var position = (action.StartTime + (normalisedWidth / 2)) * _width;

                actionUI.anchoredPosition = new Vector2((float)position, actionUI.anchoredPosition.y);
                // actionUI.position = new Vector3((float)position, actionUI.position.y, actionUI.position.z);
                actionUI.sizeDelta = new Vector2((float)width, actionUI.rect.height);
                actionUI.name = action.Index.ToString();
                actionUI.gameObject.SetActive(true);
                _actionUI[action.Index] = actionUI;
                actionUI.SetSiblingIndex(action.Index);
            }

            SetProgress(0);
            actionProgress.SetAsLastSibling();
        }

        private void SetProgress(float progress)
        {
            // actionProgress.rect.Set(0, actionProgress.rect.y,_width*progress ,actionProgress.rect.y);
            actionProgress.sizeDelta = new Vector2(_width * progress, actionProgress.rect.height);
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
            SetProgress((float)_elapsed);
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
                _actionUI[activeAction.Index].gameObject.SetActive(false);
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