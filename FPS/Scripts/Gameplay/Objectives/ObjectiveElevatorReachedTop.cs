using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveElevatorReachedTop : Objective
    {
        [Tooltip("Siguiente objetivo a activar después de completar este")]
        public GameObject NextObjective;

        [Header("Textos")]
        [Tooltip("Mensaje mostrado al completar este objetivo")]
        public string CompletionMessage = "¡El ascensor llegó a su destino!";

        protected override void Start()
        {
            base.Start();

            EventManager.AddListener<ElevatorReachedTopEvent>(OnElevatorReachedTop);

            if (string.IsNullOrEmpty(Title))
                Title = "Usar el ascensor";

            if (string.IsNullOrEmpty(Description))
                Description = "Activa el ascensor y espera a que llegue a la parte superior.";
        }

        void OnElevatorReachedTop(ElevatorReachedTopEvent evt)
        {
            if (IsCompleted)
                return;

            CompleteObjective(
                string.Empty,
                string.Empty,
                string.IsNullOrEmpty(CompletionMessage) ? "¡Objetivo completado!" : CompletionMessage
            );

            if (NextObjective != null)
                NextObjective.SetActive(true);

            EventManager.RemoveListener<ElevatorReachedTopEvent>(OnElevatorReachedTop);
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<ElevatorReachedTopEvent>(OnElevatorReachedTop);
        }
    }
}
