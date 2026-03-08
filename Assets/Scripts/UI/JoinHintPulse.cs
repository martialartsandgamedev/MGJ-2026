using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pulses the alpha of all Graphics on this object in and out.
/// Attach to the JoinHint UI GameObject.
/// </summary>
public class JoinHintPulse : MonoBehaviour
{
    [SerializeField] private float speed = 1.5f;
    [SerializeField] private float minAlpha = 0.2f;

    private Graphic[] _graphics;

    private void Awake()
    {
        _graphics = GetComponentsInChildren<Graphic>(includeInactive: true);
    }

    private void Update()
    {
        float alpha = Mathf.Lerp(minAlpha, 1f, (Mathf.Sin(Time.time * speed * Mathf.PI) + 1f) * 0.5f);
        foreach (var g in _graphics)
        {
            var c = g.color;
            c.a = alpha;
            g.color = c;
        }
    }
}
