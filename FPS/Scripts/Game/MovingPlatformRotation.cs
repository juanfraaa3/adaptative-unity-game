using UnityEngine;

public class SelfRotatingPlatform : MonoBehaviour
{
    public Vector3 rotationAxis = Vector3.up; // eje de rotación (por defecto, Y)
    public float rotationSpeed = 90f;         // grados por segundo

    private Vector3 initialPosition;

    void Start()
    {
        // Guarda la posición original
        initialPosition = transform.position;
    }

    void Update()
    {
        // Siempre mantén la plataforma en su posición original
        transform.position = initialPosition;

        // Rota sobre su eje local
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime, Space.Self);
    }
}
