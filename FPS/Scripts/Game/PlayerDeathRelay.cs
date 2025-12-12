using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// Escucha la muerte del jugador (Health.OnDie) y dispara el reset de enemigos.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class PlayerDeathRelay : MonoBehaviour
    {
        [Tooltip("Si activas esto, solo resetea enemigos con CheckpointId >= CurrentCheckpointId")]
        public bool UseCheckpointFiltering = false;

        [Tooltip("Se actualiza al tocar checkpoints (si usas CheckpointTrigger)")]
        public int CurrentCheckpointId = 0;

        private Health _playerHealth;

        private void Awake()
        {
            _playerHealth = GetComponent<Health>();
        }

        private void OnEnable()
        {
            if (_playerHealth != null)
                _playerHealth.OnDie += OnPlayerDie;
        }

        private void OnDisable()
        {
            if (_playerHealth != null)
                _playerHealth.OnDie -= OnPlayerDie;
        }

        private void OnPlayerDie()
        {
            if (EnemyResetManager.Instance == null) return;

            if (UseCheckpointFiltering)
                EnemyResetManager.Instance.ResetEnemiesFromCheckpoint(CurrentCheckpointId);
            else
                EnemyResetManager.Instance.ResetAllEnemies();
        }

        // Llama esto desde triggers de checkpoint
        public void SetCheckpoint(int checkpointId)
        {
            CurrentCheckpointId = checkpointId;
        }
    }
}
