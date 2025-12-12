using UnityEngine;

public class ConstantCircularPatrol : MonoBehaviour
{
    [Header("Centro del círculo")]
    [Tooltip("Si lo dejas vacío, el centro será la posición inicial.")]
    public Transform center;

    [Tooltip("Si está activo, se usará este centro fijo y se ignorará 'center'.")]
    public bool useFixedCenter = false;

    [Tooltip("Centro fijo en coordenadas de mundo.")]
    public Vector3 fixedCenter = new Vector3(0, 0, 0);

    [Header("Órbita en XZ")]
    public float radius = 6f;
    public float minRadius = 3f;
    public float maxRadius = 12f;
    public bool clockwise = true;          // sentido
    public float angularSpeed = 120f;      // grados/seg

    [Header("Altura")]
    [Tooltip("Altura constante en Y")]
    public float baseHeight = 10f;

    [Header("Orientación")]
    public bool faceMovement = false;      // mirar hacia donde avanza
    public bool facePlayer = true;         // o mirar al player
    public string playerTag = "Player";
    private Transform player;

    float angle; // radianes actuales

    void Start()
    {
        // Buscar al jugador
        if (facePlayer)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p) player = p.transform;
        }

        // Ángulo inicial relativo al centro elegido
        Vector3 currentCenter = GetCurrentCenter();
        Vector3 to = transform.position - currentCenter;
        to.y = 0f;
        if (to.sqrMagnitude < 0.01f) to = Vector3.right * Mathf.Max(radius, 1f);
        angle = Mathf.Atan2(to.z, to.x);
    }

    void Update()
    {
        StepOrbit();
        HandleFacing();
    }

    // Calcula el centro actual según settings
    Vector3 GetCurrentCenter()
    {
        if (useFixedCenter)
            return fixedCenter;
        else if (center != null)
            return center.position;
        else
            return transform.position; // fallback
    }

    void StepOrbit()
    {
        Vector3 currentCenter = GetCurrentCenter();

        float r = Mathf.Clamp(radius, minRadius, maxRadius);
        float dir = clockwise ? -1f : 1f;
        float step = angularSpeed * Mathf.Deg2Rad * Time.deltaTime * dir;
        angle += step;

        // 🔄 posición circular en XZ
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * r;

        Vector3 target = currentCenter + offset;
        target.y = baseHeight;  // altura fija

        transform.position = target;
    }

    void HandleFacing()
    {
        if (facePlayer && player)
        {
            Vector3 look = player.position - transform.position;
            look.y = 0f;
            if (look.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(look),
                    Time.deltaTime * 8f
                );
        }
        else if (faceMovement)
        {
            // dirección tangente al círculo
            float dir = clockwise ? -1f : 1f;
            Vector3 tangent = new Vector3(-Mathf.Sin(angle) * dir, 0f, Mathf.Cos(angle) * dir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(tangent),
                Time.deltaTime * 8f
            );
        }
    }

    // Gizmos para visualizar el círculo
    void OnDrawGizmosSelected()
    {
        Vector3 c = GetCurrentCenter();
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(c, 0.2f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(new Vector3(c.x, baseHeight, c.z), radius);
    }
}
