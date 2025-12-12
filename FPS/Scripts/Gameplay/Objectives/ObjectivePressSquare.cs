using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectivePressSquare : Objective
    {
        [Tooltip("Siguiente objetivo a activar después de completar este")]
        public GameObject NextObjective;

        [Header("Textos")]
        [Tooltip("Mensaje mostrado al completar este objetivo")]

        private bool m_Completed = false;

        protected override void Start()
        {
            base.Start();

            if (string.IsNullOrEmpty(Title))
                Title = "Presiona Cuadrado";

            if (string.IsNullOrEmpty(Description))
                Description = "Pulsa el botón Cuadrado para continuar.";
        }

        void Update()
        {
            if (m_Completed || IsCompleted)
                return;

            // 👇 Aquí se chequea la entrada del jugador
            if (Input.GetButtonDown("Square"))
            {
                m_Completed = true;

                CompleteObjective(string.Empty, string.Empty, string.Empty);


                if (NextObjective != null)
                    NextObjective.SetActive(true);
            }
        }
    }
}
