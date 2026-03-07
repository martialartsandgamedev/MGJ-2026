// using Sirenix.OdinInspector;

using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Controllers
{
    public class FishingSpot : MonoBehaviour
    {
        [SerializeField] private FishingSpotDefinition spotDefinition;

        public FishingSpotContext context;

        private float _elapsed;

        private List<FishingAction> _actions;

        private void OnEnable()
        {
            BindContext();
        }

        //[Button]
        private void BindContext()
        {
            context = new FishingSpotContext(spotDefinition);
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[FakeGameplay] Entered trigger of {other.name}");

            FishingController fishingController;

            if (other.TryGetComponent(out fishingController))
            {
                Debug.Log("[FakeGameplay] Changing state to be able to fish");
                fishingController.SetActiveFishingSpot(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Debug.Log($"[FakeGameplay] Entered trigger of {other.name}");

            FishingController fishingController;

            if (other.TryGetComponent(out fishingController))
            {
                Debug.Log("[FakeGameplay] Changing state to be unable to fish");
                fishingController.SetActiveFishingSpot(null);
            }
        }

        // Method to test fish generation
        //[Button]
        private Fish GetFishFromTable()
        {
            return context.Table[Random.Range(0, context.Table.Count)].GetFromDefinition();
        }
    }
}
