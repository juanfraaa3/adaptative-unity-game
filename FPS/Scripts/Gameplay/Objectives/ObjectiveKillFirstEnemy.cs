using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveKillFromSpawner : Objective
    {
        [Tooltip("Spawner que genera el enemigo de este objetivo")]
        public EnemySpawnerResettable SpawnerToKill;

        [Tooltip("Siguiente objetivo a activar después de completar este")]
        public GameObject NextObjective;

        private GameObject m_SpawnedEnemy;
        private bool m_EnemyKilled = false;

        protected override void Start()
        {
            base.Start();

            if (SpawnerToKill != null)
            {
                // Suscribirse al evento global de muerte de enemigos
                EventManager.AddListener<EnemyKillEvent>(OnEnemyKilled);

                // Buscar al enemigo instanciado por el spawner
                CacheSpawnedEnemy();
            }

            if (string.IsNullOrEmpty(Title))
                Title = "Eliminar al enemigo del spawner";

            if (string.IsNullOrEmpty(Description))
                Description = "Derrota al enemigo que genera el spawner.";
        }

        void CacheSpawnedEnemy()
        {
            // Si el spawner ya instanció algo en Start, tomamos al primer hijo como referencia
            if (SpawnerToKill != null && SpawnerToKill.transform.childCount > 0)
            {
                m_SpawnedEnemy = SpawnerToKill.transform.GetChild(0).gameObject;
            }
        }

        void OnEnemyKilled(EnemyKillEvent evt)
        {
            if (IsCompleted || SpawnerToKill == null)
                return;

            // Comprobar si el enemigo muerto salió del spawner indicado
            if (evt.Enemy != null && evt.Enemy.transform.IsChildOf(SpawnerToKill.transform))
            {
                m_EnemyKilled = true;
            }

            if (m_EnemyKilled)
            {
                CompleteObjective(string.Empty, string.Empty, "Enemigo eliminado");

                if (NextObjective != null)
                    NextObjective.SetActive(true);

                EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilled);
            }
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilled);
        }
    }
}
