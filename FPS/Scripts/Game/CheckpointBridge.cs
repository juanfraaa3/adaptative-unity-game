using UnityEngine;

namespace Unity.FPS.Game
{
    // Pon este componente en el MISMO GameObject que ya tiene tu "Checkpoint (Script)"
    // Asigna un ID por orden de progreso (1, 2, 3, ...).
    [RequireComponent(typeof(Collider))]
    public class CheckpointBridge : MonoBehaviour
    {
        public int CheckpointId = 1;

        // Garantiza que el collider sea trigger
        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            // Busca el PlayerDeathRelay en el Player
            var relay = other.GetComponent<PlayerDeathRelay>() ?? other.GetComponentInChildren<PlayerDeathRelay>();
            if (relay != null)
            {
                relay.SetCheckpoint(CheckpointId);
                // Debug.Log($"CheckpointBridge: SetCheckpoint({CheckpointId})");
            }
        }
    }
}
