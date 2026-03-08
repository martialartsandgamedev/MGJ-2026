using Controllers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private float _spawnWeight = 0;

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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, _radius);
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
    List<FishingSpotDefinition> GetDefinitions(float time)
    {
        if (time > 200) return _fishingSpotDefinitions.Where(x=>x.Table[0].Rarity <= FishRarity.Unique).ToList();
        if (time > 90) return _fishingSpotDefinitions.Where(x=>x.Table[0].Rarity <= FishRarity.Gold).ToList();
        if (time > 30) return _fishingSpotDefinitions.Where(x=>x.Table[0].Rarity <= FishRarity.Silver).ToList();

        return _fishingSpotDefinitions.Where(x=>x.Table[0].Rarity <= FishRarity.Bronze).ToList();
    }

    public IEnumerator ManageFishSpawns()
    {
        while (true)
        {
            _spawnWeight = Mathf.Clamp(_spawnWeight + Time.deltaTime, 0, 300); 
            
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
                        var definitions = GetDefinitions(_spawnWeight);
                        FishingSpotDefinition definition = GetDefinitions(_spawnWeight)[Random.Range(0, definitions.Count)];
                        
                        //Reset the spawn timer on rare spawn
                        if ((int)definition.Table[0].Rarity >= 3)
                        {
                           WorldContextEvents.Ins.RareFishSpawned.Invoke(definition.Table[0]);
                           _spawnWeight = 0;
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