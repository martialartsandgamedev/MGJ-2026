using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    void Update()
    {
        transform.forward = -Camera.main.transform.forward;
    }
}
