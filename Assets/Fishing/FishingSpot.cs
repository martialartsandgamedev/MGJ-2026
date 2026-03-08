// using Sirenix.OdinInspector;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.Splines;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Controllers
{
    public class FishingSpot : MonoBehaviour
    {
        // private FishingSpotDefinition spotDefinition;

        [SerializeField] private SplineAnimate _splineAnimate = null;

        public FishingSpotContext context;
        public UnityEvent OnFishingSpotDepleted;

        private List<FishingAction> _actions;
        private ParticleSystem _particleSystemInstance;
        private float _elapsed;
        
        [SerializeField]
        private Transform _stockRoot;
        
        [SerializeField]
        private Image _stockImage;

        [SerializeField]
        private Image _rarityCoin;
        
        
        //[Button]
        public void BindContext(FishingSpotDefinition spotDefinition)
        {
            context = new FishingSpotContext(spotDefinition);

            if (_particleSystemInstance != null)
            {
                DestroyImmediate(_particleSystemInstance.gameObject);
            }

            _particleSystemInstance = Instantiate(context.ParticleSystemTemplate, transform);

            if (context.FollowsPath)
            {
                _splineAnimate.Container = GameObject.FindWithTag("UniqueFishSpline").GetComponent<SplineContainer>();
            }
        }

        // [Button]
        public void SyncContext()
        {
            var main = _particleSystemInstance.main;
            
            main.maxParticles = (int)context.RemainingFish;

            var primaryFishable = context.Table[0];
            
            main.startSize = new ParticleSystem.MinMaxCurve(primaryFishable.SizeRange.x * 5f, primaryFishable.SizeRange.y * 5f);

            _particleSystemInstance.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            _particleSystemInstance.Play();

            if (context.FollowsPath)
            {
                _splineAnimate.Play();
            }
            
            for(int f = 0 ; f< _stockRoot.childCount ; f ++)
            {
                Destroy(_stockRoot.GetChild(f).gameObject);
            }
            
            for (int i = 0; i < context.RemainingFish; i++)
            {
                var _stock = Instantiate(_stockImage, _stockRoot);
            }

            switch (context.Table[0].Rarity)
            {
                case FishRarity.Bronze:
                    _rarityCoin.color = new Color(0.5f, 0.2f, 0);
                    break;
                
                case FishRarity.Silver:
                    _rarityCoin.color = new Color(0.5f, 0.5f, 0.5f);
                    break;
                
                case FishRarity.Gold:
                    _rarityCoin.color = new Color(0.5f, 0.5f, 0);
                    break;
                
                case FishRarity.Unique:
                    _rarityCoin.color = new Color(0.1f, 1f, 1);
                    break;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[FakeGameplay] Entered trigger of {other.name}");

            FishingController fishingController;

            if (other.TryGetComponent(out fishingController))
            {
                Debug.Log("[FakeGameplay] Changing state to be able to fish");
                fishingController.SetCanFishSpot(this, true);
                
                fishingController._playerCanvas.ShowPrompt("fish",0);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Debug.Log($"[FakeGameplay] Entered trigger of {other.name}");

            FishingController fishingController;

            if (other.TryGetComponent(out fishingController))
            {
                Debug.Log("[FakeGameplay] Changing state to be unable to fish");
                fishingController.SetCanFishSpot(this, false);
                
                fishingController._playerCanvas.HidePrompt();
            }
        }

        public Fish RemoveStock()
        {
            Debug.LogFormat("[FishingSpot] Removing stock from {0}", name);
            var fish = GetFishFromTable();
            context.RemainingFish -= 1;
            SyncContext();
            if (context.RemainingFish <= 0)
            {
                Debug.LogFormat("[FishingSpot] {0} has been depleted", name);
                OnFishingSpotDepleted.Invoke();
                Destroy(gameObject);
            }
            return fish;
        }

        public Fish GetFishFromTable()
        {
            return context.Table[Random.Range(0, context.Table.Count)].GetFromDefinition();
        }
    }
}
