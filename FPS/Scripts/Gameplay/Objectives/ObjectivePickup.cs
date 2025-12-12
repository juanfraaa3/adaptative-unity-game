using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectivePickupLauncher : Objective
    {
        [SerializeField] private GameObject LauncherPickup; // Referencia al objeto Pickup_Launcher
        [SerializeField] private GameObject NextObjective;  // Siguiente objetivo que se activa después

        private bool m_WeaponPickedUp = false;

        protected override void Start()
        {
            base.Start();

            if (string.IsNullOrEmpty(Title))
                Title = "¡Recoge el Lanzacohetes!";

            if (string.IsNullOrEmpty(Description))
                Description = "Recoge el Lanzacohetes para continuar.";
        }

        void Update()
        {
            if (IsCompleted)
                return;

            // Si el jugador ha recogido el arma, completa el objetivo
            if (LauncherPickup == null || m_WeaponPickedUp)
                return;

            // Comprobar si el jugador ha recogido el objeto específico
            if (LauncherPickup.activeInHierarchy == false)
            {
                m_WeaponPickedUp = true;
                CompleteObjective(string.Empty, string.Empty, "Lanzacohetes recogido");

                // Activar el siguiente objetivo si existe
                if (NextObjective != null)
                    NextObjective.SetActive(true);
            }
        }
    }
}
