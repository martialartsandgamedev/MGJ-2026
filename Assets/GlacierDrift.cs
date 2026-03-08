using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteAlways]
public class GlacierDrift : MonoBehaviour
{
    [SerializeField]
    private float _rotateSpeed = 1;
    // Update is called once per frame
    
    private bool _randomRotate = true;

    private void OnEnable()
    {
        if (_randomRotate)
        {
            _rotateSpeed = Random.Range(-0.2f, 0.2f);   
        }
    }


    private void Update()
    {
        
        transform.RotateAround(transform.position, transform.up,_rotateSpeed);
    }
}
