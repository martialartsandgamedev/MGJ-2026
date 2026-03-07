using System.Collections.Generic;
using UnityEngine;

namespace Controllers
{
    [CreateAssetMenu(menuName = "FishingSpotDefinition")]
    public class FishingSpotDefinition : ScriptableObject
    {
        public List<FishDefinition> Table;
        public DefaultFishingWidget UIWidget;
        
        public ParticleSystem ParticleSystemTemplate;
        
        public string ID = "Fishing Spot Default";
        
        public float Duration;
        public int ActionCount;
        public double Buffer;

        public int Capacity = 5;

        public bool FollowsPath = false;
    }
}