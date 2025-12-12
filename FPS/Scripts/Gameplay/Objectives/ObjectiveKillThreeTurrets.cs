using Unity.FPS.Game;
using UnityEngine;
using System.Collections.Generic;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveKillThreeTurrets_FromSpawners : Objective
    {
        [Tooltip("Spawners que generan las torretas finales (EnemySpawnerResettable)")]
        public List<EnemySpawnerResettable> TurretSpawners = new List<EnemySpawnerResettable>();

        [Tooltip("Siguiente objetivo a activar después de completar este")]
        public GameObject NextObjective;

        private bool m_ObjectiveCompleted = false;

        // 🔹 Lista interna para marcar qué spawners ya fueron “limpiados”
        private HashSet<EnemySpawnerResettable> clearedSpawners = new HashSet<EnemySpawnerResettable>();

        protected override void Start()
        {
            base.Start();

            EventManager.AddListener<EnemyKillEvent>(OnEnemyKilled);
            foreach (var spawner in TurretSpawners)
            {
                if (spawner != null)
                    spawner.OnEnemiesReset += HandleSpawnerReset;
            }
            if (string.IsNullOrEmpty(Title))
                Title = "Elimina las torretas finales";

            if (string.IsNullOrEmpty(Description))
                Description = "Destruye las tres torretas gigantes para completar el juego.";

            UpdateObjective(string.Empty, GetUpdatedCounter(), "Torretas destruidas: 0/" + TurretSpawners.Count);
        }
        void HandleSpawnerReset()
        {
            ResetObjectiveProgress();
        }

        void ResetObjectiveProgress()
        {
            clearedSpawners.Clear();
            m_ObjectiveCompleted = false;

            UpdateObjective(string.Empty, GetUpdatedCounter(),
                "Torretas destruidas: 0/" + TurretSpawners.Count);
        }

        void OnEnemyKilled(EnemyKillEvent evt)
        {
            if (m_ObjectiveCompleted || evt.Enemy == null)
                return;

            // 🔹 Revisa a qué spawner pertenece el enemigo que murió
            foreach (var spawner in TurretSpawners)
            {
                if (spawner == null)
                    continue;

                if (evt.Enemy.transform.IsChildOf(spawner.transform))
                {
                    // Si ese spawner aún no estaba marcado, se marca ahora
                    if (!clearedSpawners.Contains(spawner))
                        clearedSpawners.Add(spawner);

                    // Actualizar UI
                    UpdateObjective(string.Empty, GetUpdatedCounter(),
                        "Torretas destruidas: " + clearedSpawners.Count + "/" + TurretSpawners.Count);

                    // Verificar si ya se destruyeron todas
                    if (clearedSpawners.Count >= TurretSpawners.Count)
                    {
                        CompleteFinalObjective();
                    }
                    return;
                }
            }
        }

        string GetUpdatedCounter()
        {
            return clearedSpawners.Count + " / " + TurretSpawners.Count;
        }

        void CompleteFinalObjective()
        {
            m_ObjectiveCompleted = true;

            CompleteObjective(string.Empty, string.Empty, "¡Has destruido todas las torretas!");

            if (NextObjective != null)
                NextObjective.SetActive(true);

            // 🔹 Registrar tramo final en PlayerStats
            if (PlayerStats.Instance != null)
                PlayerStats.Instance.RegisterCheckpoint("Final_Level");

            EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilled);
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilled);

            // 🔹 Dejar de escuchar los eventos
            foreach (var spawner in TurretSpawners)
            {
                if (spawner != null)
                    spawner.OnEnemiesReset -= HandleSpawnerReset;
            }
        }
    }
}
