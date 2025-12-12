using UnityEngine;

public class WavePatrol : MonoBehaviour
{
    [Header("Centro del círculo")]
    [Tooltip("Si lo dejas vacío, el centro será la posición inicial.")]
    public Transform center;
    public bool useFixedCenter = false;
    public Vector3 fixedCenter = new Vector3(0, 0, 0);

    [Header("Órbita en XZ")]
    public float radius = 6f;
    public float minRadius = 3f;
    public float maxRadius = 12f;
    public bool clockwise = true;
    public float angularSpeed = 120f;   // grados/seg

    [Header("Onda en Y")]
    public float baseHeight = 10f;      // altura central
    public float verticalAmplitude = 2f; // cuánto sube/baja
    public float verticalFrequency = 1f; // ciclos por segundo
    public float verticalPhase = 0f;     // desfase inicial

    [Header("Orientación")]
    public bool faceMovement = true;
    public bool facePlayer = false;
    public string playerTag = "Player";
    private Transform player;

    float angle; // ángulo actual (radianes)

    void Start()
    {
        if (facePlayer)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p) player = p.transform;
        }

        // Ángulo inicial
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

        // posición circular en XZ
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * r;

        // onda en Y
        float waveY = baseHeight + Mathf.Sin((Time.time * verticalFrequency * Mathf.PI * 2f) + verticalPhase) * verticalAmplitude;

        Vector3 target = currentCenter + offset;
        target.y = waveY;

        transform.position = target;
    }

    void HandleFacing()
    {
        if (facePlayer && player)
        {
            Vector3 look = player.position - transform.position;
            look.y = 0f;
            if (look.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), Time.deltaTime * 8f);
        }
        else if (faceMovement)
        {
            // dirección tangente al círculo
            float dir = clockwise ? -1f : 1f;
            Vector3 tangent = new Vector3(-Mathf.Sin(angle) * dir, 0f, Mathf.Cos(angle) * dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(tangent), Time.deltaTime * 8f);
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 c = GetCurrentCenter();
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(c, 0.2f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(new Vector3(c.x, baseHeight, c.z), radius);
    }
}
