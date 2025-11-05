using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveCompleteFirstWave : Objective
    {
        [Header("Configuración")]
        [Tooltip("Referencia al WaveManager que maneja las oleadas")]
        public WaveManager TargetWaveManager;

        [Tooltip("Índice de la wave a completar (empieza en 0)")]
        public int WaveIndexToComplete = 0;

        [Tooltip("Siguiente objetivo a activar después de completar este")]
        public GameObject NextObjective;

        [Header("Textos")]
        [Tooltip("Texto que aparecerá al completar el objetivo")]
        public string CompletionMessage = "¡Oleada completada!";

        private bool m_WaveCompleted = false;
        private bool m_WaveStarted = false;

        // Guardamos los textos para mostrarlos en el momento correcto
        private string defaultTitle;
        private string defaultDescription;

        protected override void Start()
        {
            base.Start();

            if (TargetWaveManager != null)
            {
                EventManager.AddListener<WaveCompletedEvent>(OnWaveCompleted);
                EventManager.AddListener<WaveStartedEvent>(OnWaveStarted);
            }

            // Guardar los textos originales y vaciar mientras no haya empezado
            defaultTitle = Title;
            defaultDescription = Description;

            // Ocultar título/desc hasta que se empiece la wave
            Title = string.Empty;
            Description = string.Empty;
        }

        void OnWaveStarted(WaveStartedEvent evt)
        {
            if (evt.WaveIndex == WaveIndexToComplete && !m_WaveStarted)
            {
                m_WaveStarted = true;

                // 👉 Ahora sí mostramos el título y descripción
                UpdateObjective(defaultTitle, defaultDescription, string.Empty);
            }
        }

        void OnWaveCompleted(WaveCompletedEvent evt)
        {
            if (IsCompleted || TargetWaveManager == null)
                return;

            if (evt.WaveIndex == WaveIndexToComplete)
            {
                m_WaveCompleted = true;
            }

            if (m_WaveCompleted)
            {
                CompleteObjective(
                    string.Empty,
                    string.Empty,
                    string.IsNullOrEmpty(CompletionMessage)
                        ? $"¡Objetivo completado! Oleada {WaveIndexToComplete + 1} superada."
                        : CompletionMessage
                );

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
