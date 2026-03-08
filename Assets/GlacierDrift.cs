using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteAlways]
public class GlacierDrift : MonoBehaviour
{
    [SerializeField]
    private float _rotateSpeed = 1;
    // Update is called once per frame
    
    [SerializeField]
    private bool _randomRotate = true;

    [SerializeField]
    private bool _doBob = true;
    
    [SerializeField]
    private float _updownSpeed = 0.1f;

    [SerializeField]
    private float _updownAmp = 0.1f;

    [SerializeField]
    private float _buoyantHeight = 0.3f;
    
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
        
        if (_doBob)
        {
            transform.position = new Vector3(transform.position.x,_buoyantHeight + (Mathf.Sin(Time.deltaTime * _updownSpeed)*_updownAmp),transform.position.z);
        }
    }
}
