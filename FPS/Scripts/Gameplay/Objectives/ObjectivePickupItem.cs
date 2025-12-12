using UnityEngine;
using Unity.FPS.Game;

namespace Unity.FPS.Gameplay
{
    public class ObjectivePickupItemByPrefab : Objective
    {
        [Tooltip("Prefab del item a recoger (ej: Loot_Jetpack)")]
        public GameObject ItemPrefab;

        [Tooltip("Siguiente objetivo a activar después de este")]
        public GameObject NextObjective;

        protected override void Start()
        {
            base.Start();
            EventManager.AddListener<PickupEvent>(OnPickupEvent);

            if (string.IsNullOrEmpty(Title))
                Title = "Recoge el objeto";

            if (string.IsNullOrEmpty(Description))
                Description = "Consigue el objeto necesario para continuar.";
        }

        void OnPickupEvent(PickupEvent evt)
        {
            if (IsCompleted || ItemPrefab == null) return;

            // Compara por nombre del prefab (para reconocer instancias creadas en runtime)
            if (evt.Pickup != null && evt.Pickup.name.Contains(ItemPrefab.name))
            {
                CompleteObjective(string.Empty, string.Empty, "¡Has recogido: " + Title + "!");

                if (NextObjective != null)
                    NextObjective.SetActive(true);

                Destroy(gameObject);
            }
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<PickupEvent>(OnPickupEvent);
        }
    }
}
