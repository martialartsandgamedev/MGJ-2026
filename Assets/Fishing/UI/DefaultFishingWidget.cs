using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Controllers
{
    public class DefaultFishingWidget : MonoBehaviour
    {
        [SerializeField] private Image actionTargetTemplate;
        [SerializeField] private RectTransform _backingContainer;
        [SerializeField] private RectTransform _actionContainer;

        public Image actionProgress;

        private Dictionary<int, Image> _actionUI;

        private float _width;

        private FishingSpot _boundSpot;

        public void Initialise(FishingSpot spot, IEnumerable<FishingAction> actions)
        {
            _boundSpot = spot;

            //_width = actionProgress.rect.width;

            _actionUI = new Dictionary<int, Image>();

            var parent = actionTargetTemplate.transform.parent;

            foreach (var action in actions)
            {
                //Figure out things for the UI
                var actionUI = Instantiate(actionTargetTemplate, _actionContainer);

                var targetSizeNormalised = (action.EndTime - action.StartTime);
                var position = (float)(action.StartTime);
                actionUI.fillAmount = (float)targetSizeNormalised;
                actionUI.rectTransform.localEulerAngles = new Vector3(0, 0, -360 * position);
                // actionUI.transform.localScale = Vector3.one;

                // /var width = targetSizeNormalised * _width;


                //Apply it to the UI
                // actionUI.anchoredPosition = new Vector2((float)position, actionUI.anchoredPosition.y);
                //actionUI.sizeDelta = new Vector2((float)width, actionUI.rect.height);
                //actionUI.name = action.Index.ToString();
                //actionUI.gameObject.SetActive(true);
                _actionUI[action.Index] = actionUI;
                //actionUI.SetSiblingIndex(action.Index);


                //Spawn the action at a point on the normalise circle

                //Angle from normalised value
                float angle = (float)position * Mathf.PI * 2f;

                //Coords on a circle
                float x = Mathf.Sin(angle);
                float z = Mathf.Cos(angle);

                //Add centre and radius
                var positionOnCircle = new Vector3(x, z, 0) * (0.8f * _backingContainer.localScale.x) / 2;
                // actionUI.rectTransform.anchoredPosition = positionOnCircle;
            }

            SetProgress(0);
            //actionProgress.SetAsLastSibling();
        }

        public void SetProgress(float progress)
        {
            actionProgress.fillAmount = progress;
        }

        public void CompleteAction(int actionIndex)
        {
            _actionUI[actionIndex].gameObject.SetActive(false);
        }

        public void ResetAction(int actionIndex)
        {
            _actionUI[actionIndex].gameObject.SetActive(true);
        }
    }
}
