using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveUseJetpack : Objective
    {
        // Configuración del siguiente objetivo
        public GameObject NextObjective;

        private bool m_UsedJetpack = false;
        private bool m_JetpackActive = false;
        private float m_JetpackTime = 0f;  // Tiempo que se mantiene el jetpack activado

        protected override void Start()
        {
            base.Start();

            if (string.IsNullOrEmpty(Title))
                Title = "Usa el Jetpack";

            if (string.IsNullOrEmpty(Description))
                Description = "Mantén presionado el botón de salto para activar el jetpack y elevarte.";

            // Verificamos que el jetpack esté disponible (esto puede depender de tu configuración de jetpack)
            if (IsJetpackAvailable())
            {
                m_UsedJetpack = false;
                m_JetpackActive = false;
                m_JetpackTime = 0f;
            }
        }

        void Update()
        {
            if (IsCompleted)
                return;

            // Aquí verificamos si el jugador está usando el jetpack (suponiendo que sea con el botón de salto)
            if (Input.GetButton("Jump") && !m_UsedJetpack && IsPlayerInAir())  // Si se presiona el botón de salto (y el jugador está en el aire)
            {
                m_JetpackActive = true;
                m_JetpackTime += Time.deltaTime;
            }
            else
            {
                m_JetpackActive = false;
                m_JetpackTime = 0f;
            }

            // Solo completamos el objetivo si el jugador ha usado el jetpack durante un tiempo mínimo
            if (m_JetpackTime >= 1f && m_JetpackActive) // 1 segundo de uso del jetpack
            {
                m_UsedJetpack = true;
                CompleteObjective(string.Empty, string.Empty, string.Empty);


                // Activamos el siguiente objetivo
                if (NextObjective != null)
                {
                    NextObjective.SetActive(true);
                }
            }
        }

        // Esta función verifica si el jetpack está disponible en el jugador
        bool IsJetpackAvailable()
        {
            // Aquí puedes agregar tu lógica para verificar si el jetpack está habilitado o si el jugador tiene acceso al mismo
            // De momento asumimos que el jetpack está disponible siempre.
            return true;
        }

        // Verifica si el jugador está en el aire
        bool IsPlayerInAir()
        {
            // Esto puede ser ajustado dependiendo de cómo esté configurado el sistema de física de tu juego
            return !Physics.Raycast(transform.position, Vector3.down, 1f); // Verifica si el jugador está en el aire
        }
    }
}
