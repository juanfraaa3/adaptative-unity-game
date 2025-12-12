using UnityEngine;
using Unity.FPS.Game;   // para Health

namespace Unity.FPS.Gameplay
{
    public class KillZone : MonoBehaviour
    {
        [Tooltip("Si está activo, mata a cualquier objeto con Health. Si no, solo al Player.")]
        public bool KillAll = false;

        [Tooltip("Referencia al script del ascensor para resetearlo al morir.")]
        public ObjectiveElevatorPoints elevator;

        private void OnTriggerEnter(Collider other)
        {
            // Busca componente Health
            var health = other.GetComponentInParent<Health>();
            if (health == null) return;

            // Filtra: si KillAll está apagado, solo mata al Player
            if (!KillAll)
            {
                if (!other.transform.root.CompareTag("Player"))
                    return;
            }

            // Mata inmediatamente
            health.Kill();

            // Resetea el ascensor y desactiva KillZone
            if (elevator != null)
                elevator.ResetElevatorAndKillZone();
        }
    }
}
