using UnityEngine;
using UnityEngine.AI;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private float _radius = 20f;
    
    [SerializeField]
    private float _interval = 5f;
    
    private float _timer = 0f;

    private Vector3 _randomPoint;

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= _interval)
        {
            _timer = 0f;

            Vector3 randomDirection = Random.insideUnitSphere * _radius;
            randomDirection += transform.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, _radius, NavMesh.AllAreas))
            {
                _randomPoint = hit.position;
            }
        }

        if (_randomPoint != Vector3.zero)
        {
            DebugDrawSphere(_randomPoint, 5f, Color.red);
        }
    }

    private void DebugDrawSphere(Vector3 position, float radius, Color color)
    {
        Debug.DrawLine(position + Vector3.up * radius, position - Vector3.up * radius, color);
        Debug.DrawLine(position + Vector3.right * radius, position - Vector3.right * radius, color);
        Debug.DrawLine(position + Vector3.forward * radius, position - Vector3.forward * radius, color);
    }
}
