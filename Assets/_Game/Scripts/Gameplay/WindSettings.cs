using UnityEngine;

[CreateAssetMenu(fileName = "WindSettings", menuName = "Game/Wind Settings", order = -1000)]
public class WindSettings : ScriptableObject
{
    public float MinimumCooldown = 10f;
    public float WarningDuration = 2f;

    public AnimationCurve strengthCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public float GustDuration => strengthCurve.keys[^1].time;
    public float MaximumStrength = 1f;
}
