using UnityEngine;

public class GlacierDrift : MonoBehaviour
{
    [SerializeField]
    private float _rotateSpeed = 1;
    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(transform.position, transform.up,_rotateSpeed);
    }
}
