using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField]
    private bool _invert = false;

    private void Update()
    {
        transform.forward = (_invert ? 1 : -1) * Camera.main.transform.forward;
    }
}
