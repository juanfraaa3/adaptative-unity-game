using UnityEngine;
using Unity.FPS.Gameplay;

namespace Unity.FPS.Gameplay
{
    public class P2P3LoggingDeactivator : MonoBehaviour
    {
        [Header("References")]
        public JetpackTrajectoryLogger Trajectory;
        public LandingMetricsLogger Landing;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            if (Trajectory != null)
            {
                Trajectory.EnableLogging = false;
                // Opcional: si quieres cerrar el segmento actual al salir de la zona:
                // Trajectory.MarkDeath();  // O dejar que se cierre por el propio vuelo
            }

            if (Landing != null)
            {
                Landing.EnableLogging = false;
                Landing.ForceClearSuppressFlag(); // limpio el flag anti-segmentos fantasma
            }

            Debug.Log("[P2P3] Logging DEACTIVATED (Trajectory + Landing)");
        }
    }
}
