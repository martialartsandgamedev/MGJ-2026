using UnityEngine;

[CreateAssetMenu(menuName = "FISH/Boost Settings", order = 1)]
public class BoostSettings : ScriptableObject
{
    public AnimationCurve SpeedCurve;
    public float Cooldown;
    public float Duration;
}
