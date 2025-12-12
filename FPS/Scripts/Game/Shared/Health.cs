using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
    public class Health : MonoBehaviour
    {
        [Tooltip("Maximum amount of health")] public float MaxHealth = 10f;

        [Tooltip("Health ratio at which the critical health vignette starts appearing")]
        public float CriticalHealthRatio = 0.3f;

        public UnityAction<float, GameObject> OnDamaged;
        public UnityAction<float> OnHealed;
        public UnityAction OnDie;

        public float CurrentHealth { get; set; }
        public bool Invincible { get; set; }
        public bool CanPickup() => CurrentHealth < MaxHealth;

        public float GetRatio() => CurrentHealth / MaxHealth;
        public bool IsCritical() => GetRatio() <= CriticalHealthRatio;

        bool m_IsDead;

        // 🔹 NUEVO: Guardar última fuente de daño
        GameObject lastDamageSource;

        void Start()
        {
            CurrentHealth = MaxHealth;
            //Debug.Log($"{gameObject.name} inicia con {CurrentHealth}/{MaxHealth} de vida");
        }

        public void Heal(float healAmount)
        {
            float healthBefore = CurrentHealth;
            CurrentHealth += healAmount;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);

            // call OnHeal action
            float trueHealAmount = CurrentHealth - healthBefore;
            if (trueHealAmount > 0f)
            {
                OnHealed?.Invoke(trueHealAmount);
            }
        }

        public void TakeDamage(float damage, GameObject damageSource)
        {
            if (Invincible)
                return;

            float healthBefore = CurrentHealth;
            CurrentHealth -= damage;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);

            // Guardar la última fuente de daño (enemigo, proyectil, etc.)
            lastDamageSource = damageSource;

            // call OnDamage action
            float trueDamageAmount = healthBefore - CurrentHealth;
            if (trueDamageAmount > 0f)
            {
                OnDamaged?.Invoke(trueDamageAmount, damageSource);
            }

            HandleDeath();
        }

        public void Kill()
        {
            CurrentHealth = 0f;

            // Al morir por vacío u otras causas, no hay atacante
            lastDamageSource = null;

            // call OnDamage action
            OnDamaged?.Invoke(MaxHealth, null);

            HandleDeath();
        }

        void HandleDeath()
        {
            if (m_IsDead)
                return;

            if (CurrentHealth <= 0f)
            {
                m_IsDead = true;
                //Debug.Log($"{gameObject.name} murió");

                OnDie?.Invoke();

                // 🔹 Registrar muerte solo si es el jugador
                if (CompareTag("Player") && PlayerStats.Instance != null)
                {
                    // Diferenciar tipo de muerte
                    if (lastDamageSource == null)
                        PlayerStats.Instance.RegisterVoidDeath();
                    else
                        PlayerStats.Instance.RegisterEnemyDeath();
                }
                // 🔹 Si es un enemigo
                else if (CompareTag("Enemy") && PlayerStats.Instance != null)
                {
                    PlayerStats.Instance.RegisterEnemyKill();
                }
            }
        }

    }
}
