using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveDoorOpener : MonoBehaviour
    {
        public Objective ObjectiveToWatch;
        public Transform WallToMove;
        public Vector3 MoveOffset = new Vector3(0, 5, 0);
        public float MoveDuration = 1.0f;

        private bool m_DoorOpened = false;
        private Vector3 m_InitialPosition;
        private float m_TimeElapsed = 0f;

        void Start()
        {
            m_InitialPosition = WallToMove.position;
            Objective.OnObjectiveCompleted += OnObjectiveCompleted; // 👈 evento estático
        }

        void OnDestroy()
        {
            Objective.OnObjectiveCompleted -= OnObjectiveCompleted; // buena práctica
        }

        void OnObjectiveCompleted(Objective obj)
        {
            if (obj == ObjectiveToWatch && !m_DoorOpened)
            {
                m_DoorOpened = true;
            }
        }

        void Update()
        {
            if (m_DoorOpened && WallToMove != null && m_TimeElapsed < MoveDuration)
            {
                m_TimeElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(m_TimeElapsed / MoveDuration);
                WallToMove.position = Vector3.Lerp(m_InitialPosition, m_InitialPosition + MoveOffset, t);
            }
        }
    }
}
