using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveDisplayText : Objective
    {
        // Tiempo en segundos que el texto estará visible
        public float DisplayTime = 5f;  // Cambia el valor según el tiempo que quieras
        private float m_Timer = 0f;

        // Siguiente objetivo a activar
        public GameObject NextObjective;

        protected override void Start()
        {
            base.Start();

            // Título y descripción por defecto
            if (string.IsNullOrEmpty(Title))
                Title = "Texto en pantalla";

            if (string.IsNullOrEmpty(Description))
                Description = "Este es un mensaje en pantalla que desaparecerá después de algunos segundos.";
        }

        void Update()
        {
            // Solo se activa si el objetivo no está completado
            if (IsCompleted)
                return;

            // Sumar el tiempo cada frame
            m_Timer += Time.deltaTime;

            // Si ha pasado el tiempo, se marca el objetivo como completado
            if (m_Timer >= DisplayTime)
            {
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
