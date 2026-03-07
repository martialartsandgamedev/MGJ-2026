using Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class WorldContextManager : MonoBehaviour
{
    [SerializeField]
    private float _radius = 20f;
    

    [SerializeField]
    private float _fishSpawnInterval = 5f;
    
    private float _fishSpawnTimer = 0f;

    [SerializeField]
    private float _spawnWeight = -1;

    private Vector3 _randomPoint;

    [SerializeField]
    private int _maxFishingSpots = 4;
    
    [SerializeField]
    private FishingSpot _fishingSpotTemplate;

    [SerializeField]
    private List<FishingSpotDefinition> _fishingSpotDefinitions;

    private List<FishingSpot> _activeFishingSpots = new List<FishingSpot>();
    
    private void OnEnable()
    {
        StartCoroutine(ManageFishSpawns());
    }

    // private void Update()
    // {
    //     _fishSpawnTimer += Time.deltaTime;
    //
    //     if (_fishSpawnTimer >= _fishSpawnInterval)
    //     {
    //         _fishSpawnTimer = 0f;
    //
    //         Vector3 randomDirection = Random.insideUnitSphere * _radius;
    //         randomDirection += transform.position;
    //
    //         NavMeshHit hit;
    //         if (NavMesh.SamplePosition(randomDirection, out hit, _radius, NavMesh.AllAreas))
    //         {
    //             _randomPoint = hit.position;
    //         }
    //     }
    //
    //     if (_randomPoint != Vector3.zero)
    //     {
    //         DebugDrawSphere(_randomPoint, 5f, Color.red);
    //     }
    // }
    
    //0 - Equal - -1 favours uncommon - 1 favors common
    FishingSpotDefinition GetWeightedSpotDefinition(float bias)
    {
        float totalWeight = 0;

        foreach (var definition in _fishingSpotDefinitions)
        {
            totalWeight += Mathf.Pow((float)definition.Table[0].Rarity, bias);   
        }
        

        float roll = UnityEngine.Random.value * totalWeight;

        foreach (var definition in _fishingSpotDefinitions)
        {
            roll -=  Mathf.Pow((float)definition.Table[0].Rarity, bias);

            if (roll <= 0)
            {
                return definition; 
            }
               
        }

        return _fishingSpotDefinitions[0];
    }

    public IEnumerator ManageFishSpawns()
    {
        while (true)
        {
            _spawnWeight = Mathf.Clamp(_spawnWeight + Time.deltaTime/100, -1, 10); 
            
            //Spawn fishing spots
            if (_activeFishingSpots.Count <_maxFishingSpots)
            {
                _fishSpawnTimer += Time.deltaTime;

                if (_fishSpawnTimer >= _fishSpawnInterval)
                {
                    _fishSpawnTimer = 0f;

                    Vector3 randomDirection = Random.insideUnitSphere * _radius;
                    randomDirection += transform.position;

                    NavMeshHit hit;

                    if (NavMesh.SamplePosition(randomDirection, out hit, _radius, NavMesh.AllAreas))
                    {
                        _randomPoint = hit.position;

                        var _fishingSpotInstance = Instantiate(_fishingSpotTemplate);
                        _fishingSpotInstance.transform.position = _randomPoint;

                        //Pick a definiton
                        FishingSpotDefinition definition = GetWeightedSpotDefinition(-1);
                        
                        //Reset the spawn timer on rare spawn
                        if ((int)definition.Table[0].Rarity >= 3)
                        {
                            _spawnWeight = -1;
                           WorldContextEvents.Ins.RareFishSpawned.Invoke(definition.Table[0]);
                        }
                        _activeFishingSpots.Add(_fishingSpotInstance);
                        
                        //Base it on rarity
                        _fishingSpotInstance.BindContext(definition);
                        _fishingSpotInstance.SyncContext();
                    }
                }

                if (_randomPoint != Vector3.zero)
                {
                    DebugDrawSphere(_randomPoint, 5f, Color.red);
                }
            }

            yield return null;
        }
    }

    private void DebugDrawSphere(Vector3 position, float radius, Color color)
    {
        Debug.DrawLine(position + Vector3.up * radius, position - Vector3.up * radius, color);
        Debug.DrawLine(position + Vector3.right * radius, position - Vector3.right * radius, color);
        Debug.DrawLine(position + Vector3.forward * radius, position - Vector3.forward * radius, color);
    }
}