




using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectivePressL1R1 : Objective
    {
        [Tooltip("Siguiente objetivo a activar después de completar este")]
        public GameObject NextObjective;

        [Header("Textos")]
        [Tooltip("Mensaje mostrado al completar este objetivo")]
        public string CompletionMessage = "¡Comenzando oleada!";

        private bool m_Completed = false;

        protected override void Start()
        {
            base.Start();

            if (string.IsNullOrEmpty(Title))
                Title = "Activa la oleada";

            if (string.IsNullOrEmpty(Description))
                Description = "Presiona L1 y R1 al mismo tiempo para comenzar.";
        }

        void Update()
        {
            if (m_Completed || IsCompleted)
                return;

            // 👇 Aquí se chequea la entrada del jugador
            if (Input.GetButton("L1") && Input.GetButton("R1"))
            {
                m_Completed = true;

                CompleteObjective(
                    string.Empty,
                    string.Empty,
                    string.IsNullOrEmpty(CompletionMessage) ? "¡Objetivo completado!" : CompletionMessage
                );

                if (NextObjective != null)
                    NextObjective.SetActive(true);
            }
        }
    }
}
