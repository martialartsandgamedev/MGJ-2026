using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Serialization;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

namespace Controllers
{
    [ExecuteAlways]
    public class FishingSpot : MonoBehaviour
    {
        [SerializeField] private FishingSpotDefinition spotDefinition;

        public FishingSpotContext context;
        
        private float _elapsed;
        
        private List<FishingAction> _actions;
        
        private ParticleSystem _particleSystemInstance;
        
        [SerializeField]
        private SplineAnimate _splineAnimate = null;
        
        private void OnEnable()
        {
            BindContext();
        }

        [Button]
        private void BindContext()
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

        [Button]
        public void SyncContext()
        {
            var main = _particleSystemInstance.main;
            main.maxParticles = (int)context.RemainingFish;

            var primaryFishable = context.Table[0];
            main.startSize = new ParticleSystem.MinMaxCurve(primaryFishable.SizeRange.x * 5f, primaryFishable.SizeRange.y * 5f);
            
            _particleSystemInstance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _particleSystemInstance.Play();
            
            if (context.FollowsPath)
            {
                _splineAnimate.Play();
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[FakeGameplay] Entered trigger of {other.name}");

            CharacterController characterController;
        
            if(other.TryGetComponent(out characterController))
            {
                    Debug.Log("[FakeGameplay] Changing state to be able to fish");
                    characterController.SetState(CharacterController.PlayerState.CanFish);
                    characterController.ActiveFishingSpot = this;
            }
        
        }

        private void OnTriggerExit(Collider other)
        {
            Debug.Log($"[FakeGameplay] Entered trigger of {other.name}");
            
            CharacterController characterController;
        
            if(other.TryGetComponent(out characterController))
            {
                Debug.Log("[FakeGameplay] Changing state to be unable to fish");
                characterController.SetState(CharacterController.PlayerState.CantFish);
                characterController.ActiveFishingSpot = null;
            }
        }

        //[Button]
        private Fish GetFishFromTable()
        {
            return context.Table[Random.Range(0, context.Table.Count)].GetFromDefinition();
        }
        
        // private void Update()
        // {
        //     if (CurrentState != State.InProgress)
        //     {
        //         return;
        //     }
        //
        //     _elapsed += Time.deltaTime / spotDefinition.Duration;
        //     
        //     uiController.SetProgress((float)_elapsed);
        //     
        //     if (_elapsed >= 1.0)
        //     {
        //         _elapsed -= 1.0;
        //         return;
        //     }
        //
        //     // /_elapsed = Mathf.Repeat(_elapsed + Time.deltaTime / spotDefinition.Duration, 1f);
        //
        //     if (!Input.GetKeyDown(KeyCode.Space)) return;
        //
        //     var activeAction = GetActionInActiveWindow();
        //
        //     if (activeAction != null)
        //     {
        //         activeAction.Attempt = FishingAction.AttemptState.Success;
        //         uiController.CompleteAction(activeAction.Index);
        //         Debug.Log($"[FishingController] Successfully hit action {activeAction.Index}");
        //         if (_actions.All(action => action.Attempt == FishingAction.AttemptState.Success))
        //         {
        //             SetState(State.Complete);
        //             uiController.SetProgress(1.0f);
        //             Debug.Log("[FishingController] Successfully hit all actions");
        //         }
        //     }
        //     else
        //     {
        //         Debug.Log("[FishingController] Failed to hit action");
        //     }
        // }

        // public void StartFishing(CharacterController controller)
        // {
        //     // if (CurrentFishingSpotState != FishingSpotState.InProgress)
        //     // {
        //         //SetState(FishingSpotState.InProgress);
        //     //}
        // }

        // private void SetState(FishingSpotState fishingSpotState)
        // {
        //     if (fishingSpotState == CurrentFishingSpotState)
        //     {
        //         return;
        //     }
        //
        //     Debug.Log($"[FishingController] Setting state to {Enum.GetName(typeof(FishingSpotState), fishingSpotState)}");
        //    
        //     CurrentFishingSpotState = fishingSpotState;
        //     
        //     onStateChanged.Invoke(CurrentFishingSpotState);
        // }

        // private FishingAction GetActionInActiveWindow()
        // {
        //     return _actions.FirstOrDefault(action =>
        //         action.Attempt == FishingAction.AttemptState.Upcoming &&
        //         _elapsed >= action.StartTime &&
        //         _elapsed <= action.EndTime);
        // }
        
        // Some pretend time before kicking off the minigame
        // private IEnumerator DoFishing(CharacterController controller)
        // {
        //     // /SetState(FishingSpotState.InProgress);
        //
        //     while (CurrentFishingSpotState == FishingSpotState.InProgress)
        //     {
        //         //Check for actions
        //         var activeAction = GetActionInActiveWindow();
        //
        //         if (activeAction != null)
        //         {
        //             activeAction.Attempt = FishingAction.AttemptState.Success;
        //
        //             uiController.CompleteAction(activeAction.Index);
        //
        //             Debug.Log($"[FishingController] Successfully hit action {activeAction.Index}");
        //         }
        //         else
        //         {
        //             Debug.Log("[FishingController] Failed to hit action");
        //         }
        //
        //         //All the actions were complete
        //         if (_actions.All(action => action.Attempt == FishingAction.AttemptState.Success))
        //         {
        //             SetState(FishingSpotState.Complete);
        //             uiController.SetProgress(1.0f);
        //
        //             Debug.Log("[FishingController] Successfully hit all actions");
        //         }
        //
        //         if (_elapsed >= 1.0)
        //         {
        //             _elapsed -= 1.0;
        //         }
        //
        //         _elapsed += Time.deltaTime / spotDefinition.Duration;
        //         uiController.SetProgress((float)_elapsed);
        //
        //         yield return null;
        //     }
        //
        //
        // }
    }
}
