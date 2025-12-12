using UnityEngine;

public class MovingPlatformSquare : MonoBehaviour
{
    public Transform[] points; // Debes asignar 4 puntos en el inspector
    public float speed = 2f;

    private int currentTargetIndex = 0;
    private Vector3 lastPosition;

    void Start()
    {
        if (points.Length < 4)
        {
            Debug.LogError("Asignar 4 puntos en el array 'points' para movimiento cuadrado.");
            enabled = false;
            return;
        }

        lastPosition = transform.position;
    }

    void Update()
    {
        Transform target = points[currentTargetIndex];
        Vector3 movement = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        Vector3 delta = movement - transform.position;
        transform.position = movement;
        lastPosition = transform.position;

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            currentTargetIndex = (currentTargetIndex + 1) % points.Length;
        }
    }

    public Vector3 GetDeltaMovement()
    {
        return transform.position - lastPosition;
    }
}
