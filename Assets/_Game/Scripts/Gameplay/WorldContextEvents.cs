using Controllers;
using UnityEngine;
using UnityEngine.Events;
public class WorldContextEvents:MonoBehaviour
{
    public static WorldContextEvents Ins;
    
    //World context events
    public UnityEvent<FishDefinition> RareFishSpawned;
    
    public UnityEvent<FishingSpot> FishingSpotDepleted;

    private void Awake()
    {
        if (Ins == null)
        {
            Ins = this;
        }
    }
    
    private void OnEnable()
    {
        if (RareFishSpawned == null)
        {
            RareFishSpawned = new UnityEvent<FishDefinition>();
        }    
        
        if (FishingSpotDepleted == null)
        {
            FishingSpotDepleted = new UnityEvent<FishingSpot>();
        }    
    }
}
