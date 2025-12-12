using UnityEngine;
using UnityEngine.AI;

namespace Unity.FPS.Game
{
    /// <summary>
    /// Adjuntar al GameObject raíz del enemigo.
    /// Restaura posición/rotación, vida y estados básicos.
    /// </summary>
    public class EnemyRespawn : MonoBehaviour
    {
        [Header("Opcional: checkpoint al que pertenece este enemigo")]
        public int CheckpointId = 0;

        // Estado inicial
        private Vector3 _startPos;
        private Quaternion _startRot;

        // Componentes (se buscan en hijos o en el mismo objeto)
        private Health _health;
        private Damageable _damageable;
        private NavMeshAgent _agent;
        private Animator _anim;
        private Collider[] _colliders;

        private void Awake()
        {
            _startPos = transform.position;
            _startRot = transform.rotation;

            _health = GetComponentInChildren<Health>() ?? GetComponent<Health>();
            _damageable = GetComponentInChildren<Damageable>() ?? GetComponent<Damageable>();
            _agent = GetComponentInChildren<NavMeshAgent>() ?? GetComponent<NavMeshAgent>();
            _anim = GetComponentInChildren<Animator>() ?? GetComponent<Animator>();
            _colliders = GetComponentsInChildren<Collider>(true);
        }

        public void ResetEnemy()
        {
            // Reactivar si estaba apagado por "muerte"
            if (!gameObject.activeSelf) gameObject.SetActive(true);

            // Posición/rotación
            transform.SetPositionAndRotation(_startPos, _startRot);

            // Navegación
            if (_agent != null)
            {
                _agent.Warp(_startPos);
                _agent.isStopped = false;
                _agent.ResetPath();
            }

            // Vida (FPS Microgame: Health expone MaxHealth / CurrentHealth / Invincible)
            if (_health != null)
            {
                _health.Invincible = false;
                _health.CurrentHealth = _health.MaxHealth;
            }

            // Rehabilitar daño y colisiones
            if (_damageable != null) _damageable.enabled = true;

            if (_colliders != null)
            {
                for (int i = 0; i < _colliders.Length; i++)
                    _colliders[i].enabled = true;
            }

            // Animación
            if (_anim != null)
            {
                _anim.Rebind();
                _anim.Update(0f);
            }
        }
    }
}
