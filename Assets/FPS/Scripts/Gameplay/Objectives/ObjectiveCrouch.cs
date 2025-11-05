using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveCrouch : Objective
    {
        // Configuración del siguiente objetivo
        public GameObject NextObjective;

        private bool m_HasCrouched = false;

        protected override void Start()
        {
            base.Start();

            if (string.IsNullOrEmpty(Title))
                Title = "Agáchate una vez";

            if (string.IsNullOrEmpty(Description))
                Description = "Presiona el botón de agacharse para completar este objetivo.";

            m_HasCrouched = false;
        }

        void Update()
        {
            if (IsCompleted)
                return;

            // Detectar si el jugador se agacha (por ejemplo, con la tecla `C` o el botón correspondiente)
            if (Input.GetButtonDown("Crouch") && !m_HasCrouched)  // Si se presiona el botón de agacharse (modifica si es necesario)
            {
                m_HasCrouched = true;
                CompleteObjective(string.Empty, string.Empty, string.Empty);

                // Activar el siguiente objetivo
                if (NextObjective != null)
                {
                    NextObjective.SetActive(true);
                }
            }
        }
    }
}
