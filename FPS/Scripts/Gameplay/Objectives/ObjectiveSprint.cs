using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveSprint : Objective
    {
        bool m_HasSprinted = false;

        [Header("Secuencia")]
        [Tooltip("Próximo objetivo que se activará después de completar este")]
        [SerializeField] GameObject NextObjective;

        protected override void Start()
        {
            base.Start();

            if (string.IsNullOrEmpty(Title))
                Title = "¡Corre una vez!";

            if (string.IsNullOrEmpty(Description))
                Description = "Mantén presionada la tecla de sprint mientras te mueves.";
        }

        void Update()
        {
            if (IsCompleted)
                return;

            // Sprint = Shift por defecto ("Sprint" definido en Input Manager)
            if (!m_HasSprinted && Input.GetButton("Sprint"))
            {
                m_HasSprinted = true;

                CompleteObjective(string.Empty, string.Empty, "¡Eso es correr!");

                if (NextObjective != null)
                    NextObjective.SetActive(true);
            }
        }
    }
}
