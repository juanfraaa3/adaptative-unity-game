using UnityEngine;
using Unity.FPS.Game;

namespace Unity.FPS.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class ObjectiveReachZone : Objective
    {
        [Tooltip("Siguiente objetivo a activar después de completar este")]
        public GameObject NextObjective;

        protected override void Start()
        {
            base.Start();

            if (string.IsNullOrEmpty(Title))
                Title = "Alcanza la zona";

            if (string.IsNullOrEmpty(Description))
                Description = "Llega al área marcada para continuar.";

            // Asegurar que el collider es trigger
            Collider col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (IsCompleted)
                return;

            if (other.CompareTag("Player"))
            {
                CompleteObjective(string.Empty, string.Empty, "¡Zona alcanzada!");

                if (NextObjective != null)
                    NextObjective.SetActive(true);
            }
        }
    }
}
