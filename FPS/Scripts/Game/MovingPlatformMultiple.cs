using UnityEngine;

public class MovingPlatformMultiple : MonoBehaviour
{
    public Transform[] points; // Arreglo de puntos de destino
    public float speed = 2f;

    private int currentTargetIndex = 0; // Índice del punto actual
    private Vector3 lastPosition;

    void Start()
    {
        if (points.Length > 0)
        {
            lastPosition = transform.position;
        }
    }

    void Update()
    {
        if (points.Length == 0) return; // Si no hay puntos, no hacemos nada

        // Mueve la plataforma hacia el siguiente punto
        Vector3 movement = Vector3.MoveTowards(transform.position, points[currentTargetIndex].position, speed * Time.deltaTime);
        Vector3 delta = movement - transform.position;
        transform.position = movement;
        lastPosition = transform.position;

        // Si llegamos al punto, pasamos al siguiente
        if (Vector3.Distance(transform.position, points[currentTargetIndex].position) < 0.1f)
        {
            currentTargetIndex = (currentTargetIndex + 1) % points.Length; // Cicla a través de los puntos
        }
    }

    public Vector3 GetDeltaMovement()
    {
        return transform.position - lastPosition;
    }
}
