using UnityEngine;
using System.Collections;

public class PerfectSyncedTwoPoints_WithMargin : MonoBehaviour
{
    [Header("Plataformas a sincronizar")]
    public Transform platformA;
    public Transform platformB;

    [Header("Configuración")]
    public float margin = 2f;        // distancia desde el punto medio donde se detendrán
    public float travelTime = 2f;    // tiempo de ida/vuelta
    public float pauseTime = 0.5f;   // pausa al cambiar de dirección

    private Vector3 startA;
    private Vector3 startB;
    private Vector3 leftLimitA;
    private Vector3 rightLimitB;
    private Vector3 midPoint;

    void Start()
    {
        startA = platformA.position;
        startB = platformB.position;

        // Calcular el punto medio entre ambas plataformas
        float midX = (startA.x + startB.x) / 2f;
        midPoint = new Vector3(midX, startA.y, startA.z);

        // Calcular los puntos donde se detendrán según el margen
        leftLimitA = new Vector3(midX - margin, startA.y, startA.z);
        rightLimitB = new Vector3(midX + margin, startB.y, startB.z);

        // Iniciar el ciclo de movimiento
        StartCoroutine(MoveCycle());
    }

    IEnumerator MoveCycle()
    {
        while (true)
        {
            // 1️⃣ Ir desde posición inicial hacia el punto de margen (acercamiento)
            yield return MovePlatforms(startA, leftLimitA, startB, rightLimitB);

            // 2️⃣ Pausa breve cuando llegan al margen
            yield return new WaitForSeconds(pauseTime);

            // 3️⃣ Regresar a las posiciones iniciales
            yield return MovePlatforms(leftLimitA, startA, rightLimitB, startB);

            yield return new WaitForSeconds(pauseTime);
        }
    }

    IEnumerator MovePlatforms(Vector3 fromA, Vector3 toA, Vector3 fromB, Vector3 toB)
    {
        float elapsed = 0f;

        while (elapsed < travelTime)
        {
            float t = elapsed / travelTime;
            platformA.position = Vector3.Lerp(fromA, toA, t);
            platformB.position = Vector3.Lerp(fromB, toB, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        platformA.position = toA;
        platformB.position = toB;
    }

#if UNITY_EDITOR
    // 🔹 Dibuja líneas guía en la escena para visualizar los puntos de margen
    void OnDrawGizmos()
    {
        if (platformA == null || platformB == null) return;

        Vector3 startApos = Application.isPlaying ? startA : platformA.position;
        Vector3 startBpos = Application.isPlaying ? startB : platformB.position;

        float midX = (startApos.x + startBpos.x) / 2f;
        Vector3 mid = new Vector3(midX, startApos.y, startApos.z);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(mid.x, mid.y + 5, mid.z), new Vector3(mid.x, mid.y - 5, mid.z)); // línea del punto medio

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(mid.x - margin, mid.y + 5, mid.z), new Vector3(mid.x - margin, mid.y - 5, mid.z)); // margen A
        Gizmos.DrawLine(new Vector3(mid.x + margin, mid.y + 5, mid.z), new Vector3(mid.x + margin, mid.y - 5, mid.z)); // margen B
    }
#endif
}
