using UnityEngine;

public class JetpackLoggingDeactivator : MonoBehaviour
{
    public JetpackOrientationMetrics metrics;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("JETPACK LOGGING → DEACTIVATED");
            metrics.EnableLogging = false;

            // Cerrar cualquier segmento activo automáticamente
            metrics.StopTrackingAndLog();
        }
    }
}
