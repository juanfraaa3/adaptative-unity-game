using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;

    private Transform target;
    private Vector3 lastPosition;

    void Start()
    {
        target = pointB;
        lastPosition = transform.position;
    }

    void Update()
    {
        Vector3 movement = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        Vector3 delta = movement - transform.position;
        transform.position = movement;
        lastPosition = transform.position;

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            target = (target == pointA) ? pointB : pointA;
        }
    }

    public Vector3 GetDeltaMovement()
    {
        return transform.position - lastPosition;
    }
}
