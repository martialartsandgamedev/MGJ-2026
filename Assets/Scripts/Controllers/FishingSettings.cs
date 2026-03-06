using UnityEngine;

namespace Controllers
{
    [CreateAssetMenu(menuName = "FishingSettings")]
    public class FishingSettings : ScriptableObject
    {
        public double Duration;
        public int ActionCount;
        public double Buffer;
    }
}