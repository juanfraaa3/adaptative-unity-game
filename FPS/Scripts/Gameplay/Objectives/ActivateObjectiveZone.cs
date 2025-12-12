using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ActivateObjectiveZone : MonoBehaviour
    {
        public GameObject ObjectiveToActivate;
        private bool m_HasTriggered = false;

        private void OnTriggerEnter(Collider other)
        {
            if (m_HasTriggered || ObjectiveToActivate == null)
                return;

            if (other.CompareTag("Player"))
            {
                ObjectiveToActivate.SetActive(true);
                m_HasTriggered = true;
            }
        }
    }
}
