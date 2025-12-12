using UnityEngine;
using Unity.FPS.Gameplay;

namespace Unity.FPS.Gameplay
{
    public class P2P3LoggingActivator : MonoBehaviour
    {
        [Header("References")]
        public JetpackTrajectoryLogger Trajectory;
        public LandingMetricsLogger Landing;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            if (Trajectory != null)
                Trajectory.EnableLogging = true;

            if (Landing != null)
                Landing.EnableLogging = true;

            Debug.Log("[P2P3] Logging ACTIVATED (Trajectory + Landing)");
        }
    }
}
