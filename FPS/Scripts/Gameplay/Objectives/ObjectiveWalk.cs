using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveWalk : Objective
    {
        float m_MoveTimer = 0f;
        float m_RequiredMoveTime = 1.5f;

        [Header("Secuencia")]
        [Tooltip("Próximo objetivo que se activará después de completar este")]
        [SerializeField] GameObject NextObjective;  // ✅ Esta línea es clave

        protected override void Start()
        {
            base.Start();

            if (string.IsNullOrEmpty(Title))
                Title = "Muévete";

            if (string.IsNullOrEmpty(Description))
                Description = "Muévete en cualquier dirección durante 3 segundos";
        }

        void Update()
        {
            if (IsCompleted)
                return;

            // Captura movimiento en cualquier dirección
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            if (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f)
            {
                m_MoveTimer += Time.deltaTime;

                if (m_MoveTimer >= m_RequiredMoveTime)
                {
                    // ✅ Si quieres mostrar título + descripción centrado, necesitas adaptar esto:
                    // Pero de momento lo dejamos fuera por el error en DisplayMessageEvent

                    CompleteObjective(string.Empty, string.Empty, "¡Buen trabajo!");

                    if (NextObjective != null)
                        NextObjective.SetActive(true);
                }
            }
            else
            {
                m_MoveTimer = 0f;
            }
        }
    }
}
