using UnityEngine;

public class EnemyCircularMovement : MonoBehaviour
{
    [Header("Movimiento Circular")]
    public Transform centerPoint;   // El punto alrededor del cual girará
    public float radius = 5f;       // Radio del círculo
    public float angularSpeed = 2f; // Velocidad de giro

    [Header("Jugador y Ataque")]
    public Transform player;        // Referencia al jugador
    public float detectionRange = 10f;
    public GameObject bulletPrefab; // Prefab de la bala
    public Transform firePoint;     // Punto desde donde dispara
    public float fireRate = 1f;     // Balas por segundo

    private float angle = 0f;
    private float fireCooldown = 0f;

    void Update()
    {
        // --- Movimiento circular ---
        angle += angularSpeed * Time.deltaTime;
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;
        transform.position = centerPoint.position + new Vector3(x, 0, z);

        // --- Siempre mirar al jugador ---
        if (player != null)
        {
            Vector3 dir = player.position - transform.position;
            dir.y = 0; // No inclinar hacia arriba/abajo
            if (dir.magnitude > 0.1f)
                transform.rotation = Quaternion.LookRotation(dir);

            // --- Disparo si está en rango ---
            if (dir.magnitude <= detectionRange)
            {
                Shoot();
            }
        }
    }

    void Shoot()
    {
        if (Time.time > fireCooldown)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            fireCooldown = Time.time + 1f / fireRate;
        }
    }
}
