using UnityEngine;

namespace Unity.FPS.Game
{
    public class EnemyResetManager : MonoBehaviour
    {
        public static EnemyResetManager Instance { get; private set; }

        private EnemyRespawn[] _enemies;                    // enemigos que NO se destruyen (se reactivan)
        private EnemySpawnerResettable[] _spawners;         // enemigos que SÍ se destruyen (se re-instancian)

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            RefreshList();
        }

        public void RefreshList()
        {
            _enemies = FindObjectsOfType<EnemyRespawn>(true);
            _spawners = FindObjectsOfType<EnemySpawnerResettable>(true);
        }

        public void ResetAllEnemies()
        {
            if (_enemies != null)
                foreach (var e in _enemies) if (e != null) e.ResetEnemy();

            if (_spawners != null)
                foreach (var s in _spawners) if (s != null) s.ResetState();
        }

        public void ResetEnemiesFromCheckpoint(int checkpointId)
        {
            if (_enemies != null)
                foreach (var e in _enemies)
                    if (e != null && e.CheckpointId >= checkpointId) e.ResetEnemy();

            if (_spawners != null)
                foreach (var s in _spawners)
                    if (s != null && s.CheckpointId >= checkpointId) s.ResetState();
        }
    }
}
