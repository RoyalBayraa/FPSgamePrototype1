using UnityEngine;

public class SphereCAst : MonoBehaviour
{

    Vector3 origin;
    Vector3 _direction = Vector3.down;
    float maxDistance = 1f;
    float curretnDistance;
    float sphereRaius = 0.5f;

    [SerializeField] LayerMask groundLayer;

    [SerializeField] Vector3 offset;


    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        origin = transform.position + offset;
        RaycastHit hit;

        if (Physics.SphereCast(origin, sphereRaius, _direction, out hit, maxDistance, groundLayer))
        {
            curretnDistance = hit.distance;
        }
        else
        {
            curretnDistance = maxDistance;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawSphere(origin, 0.2f);
        Debug.DrawLine(origin, origin + _direction * curretnDistance, Color.green);
        Gizmos.DrawWireSphere(origin + _direction * curretnDistance, sphereRaius);
    }
}
