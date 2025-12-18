using UnityEngine;
using UnityEngine.UI;
using Unity.FPS.Game;
namespace Unity.FPS.UI
{
    public class LowHealthFeedback : MonoBehaviour
    {
        [Header("References")]
        public Health PlayerHealth;
        public Image OverlayImage;

        [Header("Settings")]
        public float CriticalThreshold = 30f;
        public float PulseSpeed = 3f;

        private Color baseColor;

        void Start()
        {
            if (OverlayImage != null)
                baseColor = OverlayImage.color;

            // 🔹 Si no está asignado, buscar al jugador automáticamente
            if (PlayerHealth == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    PlayerHealth = player.GetComponent<Health>();
            }
        }

        void Update()
        {
            if (PlayerHealth == null || OverlayImage == null)
                return;

            float health = PlayerHealth.CurrentHealth;

            if (health <= CriticalThreshold)
            {
                float alpha = Mathf.Abs(Mathf.Sin(Time.time * PulseSpeed)) * 0.5f + 0.25f;
                OverlayImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            }
            else
            {
                OverlayImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0);
            }
        }
    }
}
