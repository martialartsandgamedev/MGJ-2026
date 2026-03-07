using System;
using System.Collections;
using System.Collections.Generic;
using Controllers;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class CharacterController : MonoBehaviour
{
    public enum PlayerState
    {
        CantFish,
        CanFish,
        IsFishing,
    }

    private List<FishingAction> _actions;
    
    [SerializeField] private float speed = 5f;
    
    [SerializeField] private MeshRenderer model;
    
    [SerializeField] private TextMeshProUGUI statusText;

    public FishingSpot ActiveFishingSpot;
    
    private PlayerState m_currentPlayerState = PlayerState.CantFish;
    
    private void OnEnable()
    {
        model.material.color = Color.red;
        
        // // _actions = FishingAction.Create(spotDefinition);
        // uiController.Initialise(_actions);
    }

    private void FixedUpdate()
    {
        // if (m_currentPlayerState == PlayerState.IsFishing)
        // {
        //     return;
        // }

        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * (speed * Time.deltaTime));
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * (speed * Time.deltaTime));
        }

        // //Telgraphed by the nearest fishing spot
        // if (!m_activeFishingSpot)
        // {
        //     m_activeFishingSpot.onStateChanged.AddListener(OnFishingStateChange);
        // }
        
        //Trigger the fishing attempt
        if (m_currentPlayerState == PlayerState.CanFish && ActiveFishingSpot && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("[FakeGameplay] Creating fishing controller");
            
            StartCoroutine(TryFishActive());
            
            // if (ActiveFishingSpot.CurrentFishingSpotState == FishingSpot.FishingSpotState.ReadyToStart)
            // {
            //     StartCoroutine(TryFishing(ActiveFishingSpot));
            // }
        }
    }
    
    // private FishingAction GetActionInActiveWindow()
    // {
    //     return _actions.FirstOrDefault(action =>
    //         action.Attempt == FishingAction.AttemptState.Upcoming &&
    //         _elapsed >= action.StartTime &&
    //         _elapsed <= action.EndTime);
    // }

    // private void OnTriggerEnter(Collider other)
    // {
    //     Debug.Log($"[FakeGameplay] Entered trigger of {other.name}");
    //
    //     FishingSpot fishingSpot;
    //     
    //    if(other.TryGetComponent(out fishingSpot))
    //     {
    //         if (_currentState == State.CantFish)
    //         {
    //             // _fishableShoal = other.transform.parent.gameObject;
    //             
    //             Debug.Log("[FakeGameplay] Changing state to be able to fish");
    //             SetState(State.CanFish);
    //             model.material.color = Color.green;        
    //         }
    //     }
    //     
    // }
    //
    // private void OnTriggerExit(Collider other)
    // {
    //     Debug.Log($"[FakeGameplay] Entered trigger of {other.name}");
    //     
    //     
    //     FishingSpot fishingSpot;
    //     
    //     if(other.TryGetComponent(out fishingSpot))
    //     {
    //         if (_currentState == State.CanFish)
    //         {
    //             Debug.Log("[FakeGameplay] Changing state to be unable to fish");
    //             SetState(State.CantFish);
    //             model.material.color = Color.red;   
    //         }
    //     }
    // }

    public void SetState(PlayerState playerState)
    {
        if (playerState == m_currentPlayerState)
        {
            return;
        }

        Debug.Log("[FakeGameplay] Setting state to " + Enum.GetName(typeof(PlayerState), m_currentPlayerState));
        
        m_currentPlayerState = playerState;
        statusText.text = StateStrings[m_currentPlayerState];
    }

    private static readonly Dictionary<PlayerState, string> StateStrings = new()
    {
        { PlayerState.IsFishing, "FISHING" },
        { PlayerState.CantFish, "NO FISH" },
        { PlayerState.CanFish, "FISH HERE" },
    };
    
    private FishingAction GetActionInActiveWindow()
    {
        return _actions.FirstOrDefault(action => action.Attempt == FishingAction.AttemptState.Upcoming);
    }

    private IEnumerator TryFishActive()
    {
        // /SetState(FishingSpotState.InProgress);

        FishingSpot spot = ActiveFishingSpot;
        _actions = FishingAction.Create(spot.context);
        
        var uiController = Instantiate(spot.context.UIWidget);
        uiController.Initialise(spot, _actions);
        
        float _elapsed = 0;
        
        Debug.LogFormat($"{this} started a new fishing attempt on {spot}");
        
        // spot.onStateChanged.AddListener(OnFishingStateChange(spot));

        yield return new WaitForEndOfFrame();
        
        
        
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
        
        
        while (_actions.Any(action => action.Attempt != FishingAction.AttemptState.Success))
        {
            //Check for actions
            var activeAction = GetActionInActiveWindow();

            if (activeAction != null)
            {
                if(Input.GetKeyDown(KeyCode.Space))
                {
                    activeAction.Attempt = FishingAction.AttemptState.Success;

                    uiController.CompleteAction(activeAction.Index);

                    Debug.Log($"[FishingController] Successfully hit action {activeAction.Index}");
                }
            }
            else
            {
                Debug.Log("[FishingController] Failed to hit action");
            }

            //All the actions were complete
            if (_actions.All(action => action.Attempt == FishingAction.AttemptState.Success))
            {
                //SetState(FishingSpot.FishingSpotState.Complete);
                uiController.SetProgress(1.0f);

                Debug.Log("[FishingController] Successfully hit all actions");
            }

            if (_elapsed >= 1.0f)
            {
                _elapsed -= 1.0f;
            }

            _elapsed += Time.deltaTime / spot.context.Duration;
            uiController.SetProgress((float)_elapsed);

            yield return null;
        }

        //ActiveFishingSpot.onStateChanged.RemoveListener(OnFishingStateChange(ActiveFishingSpot));
        
    }

    // private void OnFishingStateChange(FishingSpot fishingSpot)
    // {
    //     Debug.Log($"[FakeGameplay] Controller changed state to {Enum.GetName(typeof(FishingSpot.FishingSpotState), fishingSpotState)}");
    //     switch (fishingSpot.CurrentFishingSpotState)
    //     {
    //         case FishingSpot.FishingSpotState.Complete:
    //             Debug.Log("[FakeGameplay] Fishing complete");
    //             Destroy(m_activeFishingSpot.gameObject);
    //             SetState(PlayerState.CantFish);
    //             model.material.color = Color.red;
    //             break;
    //         case FishingSpot.FishingSpotState.Failed:
    //             Debug.Log("[FakeGameplay] Fishing failed, you suck");
    //             SetState(PlayerState.CantFish);
    //             model.material.color = Color.red;
    //             break;
    //     }
    // }
}
