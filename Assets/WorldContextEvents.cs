using UnityEngine;
using UnityEngine.Events;
public class WorldContextEvents:MonoBehaviour
{
    public static WorldContextEvents Ins;
    
    //World context events
    public UnityEvent<FishDefinition> RareFishSpawned;

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
    }
}
