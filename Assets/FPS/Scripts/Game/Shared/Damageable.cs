using UnityEngine;

namespace Unity.FPS.Game
{
    public class Damageable : MonoBehaviour
    {
        [Tooltip("Multiplier to apply to the received damage")]
        public float DamageMultiplier = 1f;

        [Range(0, 1)]
        [Tooltip("Multiplier to apply to self damage")]
        public float SensibilityToSelfdamage = 0.5f;

        public Health Health { get; private set; }

        void Awake()
        {
            // find the health component either at the same level, or higher in the hierarchy
            Health = GetComponent<Health>();
            if (!Health)
            {
                Health = GetComponentInParent<Health>();
            }
        }

        public void InflictDamage(float damage, bool isExplosionDamage, GameObject damageSource)
        {
            if (Health)
            {
                var totalDamage = damage;

                // skip the crit multiplier if it's from an explosion
                if (!isExplosionDamage)
                {
                    totalDamage *= DamageMultiplier;
                }

                // potentially reduce damages if inflicted by self
                if (Health.gameObject == damageSource)
                {
                    totalDamage *= SensibilityToSelfdamage;
                }

                //Debug.Log($"{gameObject.name} recibió {totalDamage} (daño base={damage}, multiplier={DamageMultiplier}) de {damageSource.name}");

                // apply the damages
                Health.TakeDamage(totalDamage, damageSource);

                // 🔥 Time-to-Target por PRIMER IMPACTO (cualquier daño al ENEMIGO)
                if (Health.CompareTag("Enemy"))
                {
                    Debug.Log("🔥 DAMAGEABLE recibió daño → enemigo: " + Health.name);

                    var logger = GameObject.FindObjectOfType<JitterMetricsLogger>();
                    if (logger != null)
                    {
                        Debug.Log("🔥 NOTIFY → enviando NotifyEnemyHit()");
                        logger.NotifyEnemyHit(Health.transform);
                    }
                    else
                    {
                        Debug.Log("❌ Logger NO encontrado en escena");
                    }

                }
            }
        }

    }
}