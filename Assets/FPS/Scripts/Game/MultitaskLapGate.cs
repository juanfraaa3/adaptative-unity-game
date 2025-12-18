using UnityEngine;

public class MultitaskLapGate : MonoBehaviour
{
    [Tooltip("Dot mínimo para considerar cruce en dirección correcta. 0.2–0.5 suele ir bien.")]
    public float minForwardDot = 0.3f;

    [Tooltip("Cooldown por seguridad (evita doble conteo por jitter)")]
    public float cooldownSeconds = 0.75f;

    private float _lastCountTime = -999f;

    private void OnTriggerEnter(Collider other)
    {
        var logger = other.GetComponent<MultitaskMetricsLogger>();
        if (logger == null) return;

        // Dirección de movimiento del jugador (plano XZ)
        Vector3 v = logger.GetFlatVelocity(); // la sacamos del logger para que sea consistente
        if (v.sqrMagnitude < 0.0001f) return;

        // Dirección "válida" del gate (plano XZ)
        Vector3 gateForward = transform.forward;
        gateForward.y = 0f;
        gateForward.Normalize();

        v.y = 0f;
        v.Normalize();

        float dot = Vector3.Dot(v, gateForward);

        // 1) Dirección correcta
        if (dot < minForwardDot) return;

        // 2) Cooldown simple
        if (Time.time - _lastCountTime < cooldownSeconds) return;

        _lastCountTime = Time.time;
        logger.NotifyLapCrossed();
    }
}
