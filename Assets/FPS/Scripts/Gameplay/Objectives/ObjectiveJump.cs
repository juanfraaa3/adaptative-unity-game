using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveJump : Objective
    {
        bool m_HasJumped = false;

        [Header("Secuencia")]
        [Tooltip("Próximo objetivo que se activará después de completar este")]
        [SerializeField] GameObject NextObjective;

        protected override void Start()
        {
            base.Start();

            if (string.IsNullOrEmpty(Title))
                Title = "¡Salta una vez!";

            if (string.IsNullOrEmpty(Description))
                Description = "Presiona el botón de salto para continuar.";
        }

        void Update()
        {
            if (IsCompleted)
                return;

            if (!m_HasJumped && Input.GetButtonDown("Jump"))
            {
                m_HasJumped = true;

                CompleteObjective(string.Empty, string.Empty, string.Empty);

                if (NextObjective != null)
                    NextObjective.SetActive(true);
            }
        }
    }
}
