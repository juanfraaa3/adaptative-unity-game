using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveWelcome : Objective
    {
        public float DelayBeforeAllowingContinue = 1.5f; // Tiempo mínimo antes de permitir avanzar
        public GameObject NextObjective;

        float m_Timer = 0f;
        bool m_ReadyToContinue = false;

        protected override void Start()
        {
            base.Start();

            if (string.IsNullOrEmpty(Title))
                Title = "¡Bienvenido al entrenamiento!";

            if (string.IsNullOrEmpty(Description))
                Description = "Aprende los controles básicos antes de entrar en combate.";
        }

        void Update()
        {
            if (IsCompleted)
                return;

            m_Timer += Time.deltaTime;

            // Solo después del delay permitimos continuar
            if (m_Timer >= DelayBeforeAllowingContinue)
            {
                m_ReadyToContinue = true;
            }

            // El objetivo se completa automáticamente después del delay
            if (m_ReadyToContinue)
            {
                CompleteObjective(string.Empty, string.Empty, "Tutorial iniciado");

                if (NextObjective != null)
                    NextObjective.SetActive(true);
            }
        }
    }
}
