using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveReachArea : Objective
    {
        // Configuración del siguiente objetivo
        public GameObject NextObjective;

        // Zona del mapa que el jugador debe alcanzar (se asigna desde el Inspector)
        public Collider AreaToReach;

        private bool m_AreaReached = false;

        protected override void Start()
        {
            base.Start();

            if (string.IsNullOrEmpty(Title))
                Title = "Llegar a la zona";

            if (string.IsNullOrEmpty(Description))
                Description = "Llega a la zona indicada para continuar.";

            m_AreaReached = false;
        }

        void OnTriggerEnter(Collider other)
        {
            // Verifica si el objeto que entra en el trigger es el jugador
            if (other.CompareTag("Player") && !m_AreaReached)
            {
                Debug.Log("¡Jugador llegó a la zona!");
                m_AreaReached = true;
                CompleteObjective(string.Empty, string.Empty, string.Empty);

                // Activar el siguiente objetivo
                if (NextObjective != null)
                {
                    NextObjective.SetActive(true);
                }
            }
            else
            {
                Debug.Log("Collider activado por: " + other.name);
            }
        }

        void OnDestroy()
        {
            // Asegurarse de que si el objeto es destruido, no queden eventos activos.
            m_AreaReached = true;
        }
    }
}
