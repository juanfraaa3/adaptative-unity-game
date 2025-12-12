using UnityEngine;

public class JetpackLoggingActivator : MonoBehaviour
{
    public JetpackOrientationMetrics metrics;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("JETPACK LOGGING → ACTIVATED");
            metrics.EnableLogging = true;
        }
    }
}
