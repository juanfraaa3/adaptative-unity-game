using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveEnterZone : Objective
    {
        public GameObject NextObjective;

        private bool m_PlayerHasEntered = false;
        private bool m_ZoneIsActive = false;

        void Start()
        {
            base.Start();

            if (string.IsNullOrEmpty(Title))
                Title = "Ingresa a la zona";

            if (string.IsNullOrEmpty(Description))
                Description = "Llega a la zona designada para completar el objetivo.";

            // Activamos la zona después de 0.5 segundos para evitar triggers tempranos
            Invoke(nameof(ActivateZone), 0.5f);
        }

        void ActivateZone()
        {
            m_ZoneIsActive = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (IsCompleted || !m_ZoneIsActive || m_PlayerHasEntered)
                return;

            if (other.CompareTag("Player"))
            {
                m_PlayerHasEntered = true;
                CompleteObjective(string.Empty, string.Empty, string.Empty);


                if (NextObjective != null)
                    NextObjective.SetActive(true);
            }
        }
    }
}
