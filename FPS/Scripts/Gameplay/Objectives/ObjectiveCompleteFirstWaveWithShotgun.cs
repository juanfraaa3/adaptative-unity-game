using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveCompleteFirstWaveWithShotgun : Objective
    {
        [Header("Configuración")]
        [Tooltip("Referencia al WaveManager que maneja las oleadas")]
        public WaveManager TargetWaveManager;

        [Tooltip("Índice de la wave a completar (empieza en 0)")]
        public int WaveIndexToComplete = 0;

        [Tooltip("Siguiente objetivo a activar después de completar este")]
        public GameObject NextObjective;

        [Header("Reward Setup")]
        [Tooltip("Prefab de la escopeta a spawnear")]
        public GameObject ShotgunPickupPrefab;

        [Tooltip("Punto de aparición de la escopeta (Empty en la escena)")]
        public Transform ShotgunSpawnPoint;

        [Header("Textos")]
        [Tooltip("Texto que aparecerá al completar el objetivo")]
        public string CompletionMessage = "¡Oleada completada!";

        private bool m_WaveCompleted = false;

        protected override void Start()
        {
            base.Start();

            if (TargetWaveManager != null)
            {
                EventManager.AddListener<WaveCompletedEvent>(OnWaveCompleted);
                EventManager.AddListener<WaveStartedEvent>(OnWaveStarted);
            }

            // Valores por defecto si están vacíos en el inspector
            if (string.IsNullOrEmpty(Title))
                Title = "Completar la primera oleada";

            if (string.IsNullOrEmpty(Description))
                Description = "Sobrevive y elimina todos los enemigos de la primera wave.";
        }

        void OnWaveStarted(WaveStartedEvent evt)
        {
            // No hace nada especial, pero dejamos el listener por si quieres expandirlo después
            if (evt.WaveIndex == WaveIndexToComplete)
            {
                Debug.Log($"Wave {evt.WaveIndex} iniciada para el objetivo {name}");
            }
        }

        void OnWaveCompleted(WaveCompletedEvent evt)
        {
            if (IsCompleted || TargetWaveManager == null)
                return;

            if (evt.WaveIndex == WaveIndexToComplete)
                m_WaveCompleted = true;

            if (m_WaveCompleted)
            {
                // Completar el objetivo
                CompleteObjective(
                    string.Empty,
                    string.Empty,
                    string.IsNullOrEmpty(CompletionMessage)
                        ? $"Oleada {WaveIndexToComplete + 1} superada."
                        : CompletionMessage
                );

                // 👉 Spawnear la escopeta
                if (ShotgunPickupPrefab != null && ShotgunSpawnPoint != null)
                {
                    Instantiate(ShotgunPickupPrefab, ShotgunSpawnPoint.position, ShotgunSpawnPoint.rotation);
                    Debug.Log("Escopeta spawneada en " + ShotgunSpawnPoint.position);
                }

                if (NextObjective != null)
                    NextObjective.SetActive(true);

                EventManager.RemoveListener<WaveCompletedEvent>(OnWaveCompleted);
                EventManager.RemoveListener<WaveStartedEvent>(OnWaveStarted);
            }
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<WaveCompletedEvent>(OnWaveCompleted);
            EventManager.RemoveListener<WaveStartedEvent>(OnWaveStarted);
        }
    }
}
