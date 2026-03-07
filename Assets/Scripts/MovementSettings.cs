using UnityEngine;

[CreateAssetMenu(menuName = "FISH/Movement Settings", order = -1000)]
public class MovementSettings : ScriptableObject
{
    public float Acceleration;
    public float DragStrength;
    public float MaxSpeed;
}
