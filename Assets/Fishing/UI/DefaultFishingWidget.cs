using System.Collections.Generic;
using UnityEngine;

namespace Controllers
{
    public class DefaultFishingWidget : MonoBehaviour
    {
        public RectTransform actionUITemplate;
        public RectTransform actionProgress;

        private Dictionary<int, RectTransform> _actionUI;

        private float _width;

        private FishingSpot _boundSpot;

        public void Initialise(FishingSpot spot, IEnumerable<FishingAction> actions)
        {
            _boundSpot = spot;

            _width = actionProgress.rect.width;

            _actionUI = new Dictionary<int, RectTransform>();

            var parent = actionUITemplate.transform.parent;

            foreach (var action in actions)
            {
                var actionUI = GameObject.Instantiate(actionUITemplate, parent, true);
                var normalisedWidth = (action.EndTime - action.StartTime);
                var width = normalisedWidth * _width;
                var position = (action.StartTime + (normalisedWidth / 2)) * _width;

                actionUI.anchoredPosition = new Vector2((float)position, actionUI.anchoredPosition.y);
                actionUI.sizeDelta = new Vector2((float)width, actionUI.rect.height);
                actionUI.name = action.Index.ToString();
                actionUI.gameObject.SetActive(true);
                _actionUI[action.Index] = actionUI;
                actionUI.SetSiblingIndex(action.Index);
            }

            SetProgress(0);
            actionProgress.SetAsLastSibling();
        }

        public void SetProgress(float progress)
        {
            actionProgress.sizeDelta = new Vector2(_width * progress, actionProgress.rect.height);
        }

        public void CompleteAction(int actionIndex)
        {
            _actionUI[actionIndex].gameObject.SetActive(false);
        }
    }
}
