using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// Asistencia adaptativa SOLO para la sección de plataformas entre CP4 y CP5.
    /// - Cuenta muertes mientras el jugador está en CP4 (antes de alcanzar CP5).
    /// - Al superar umbrales, escala plataformas para facilitar el paso.
    /// Requiere: Player con PlayerDeathRelay + Health, y tus checkpoints notificando al Relay.
    /// </summary>
    public class Checkpoint4PlatformAssist : MonoBehaviour
    {
        [Header("Rango de la sección (fijo a esta zona)")]
        [Tooltip("Checkpoint de INICIO de la sección")]
        public int StartCheckpointId = 4;
        [Tooltip("Checkpoint FINAL (exclusivo): al alcanzarlo termina la sección")]
        public int EndCheckpointId = 5;

        [Header("Comportamiento")]
        [Tooltip("Al tocar el CP de inicio, vuelve todo a estado base y reinicia contadores")]
        public bool ResetWhenEnteringSection = true;
        [Tooltip("Al alcanzar el CP final, revierte todas las ayudas aplicadas")]
        public bool RevertWhenSectionCompleted = false;
        public bool LogDebug = true;

        [Serializable]
        public class PlatformRule
        {
            [Tooltip("Transform de la plataforma a modificar (raíz)")]
            public Transform Platform;

            [Tooltip("Número de muertes en la sección para activar esta ayuda")]
            public int DeathsThreshold = 5;

            [Tooltip("Factor de escala respecto a la escala base")]
            public float ScaleMultiplier = 2f;

            [Tooltip("Opcional: si quieres escalar un hijo específico (mesh visual) en vez de toda la plataforma")]
            public Transform ScaleTarget;

            [HideInInspector] public Vector3 BaseScale;
            [HideInInspector] public bool Applied;
        }

        [Header("Reglas (una por plataforma o por nivel de ayuda)")]
        public List<PlatformRule> Rules = new List<PlatformRule>();

        // refs
        PlayerDeathRelay _relay;
        Health _playerHealth;

        // estado
        int _deathsInSection = 0;
        int _lastCheckpointSeen = -1;

        void Awake()
        {
            _relay = FindObjectOfType<PlayerDeathRelay>();
            if (_relay != null) _lastCheckpointSeen = _relay.CurrentCheckpointId;

            _playerHealth = _relay ? _relay.GetComponent<Health>() : FindObjectOfType<Health>();
            if (_playerHealth != null) _playerHealth.OnDie += OnPlayerDie;

            // cachear escala base
            foreach (var r in Rules)
            {
                var t = r.ScaleTarget != null ? r.ScaleTarget : r.Platform;
                if (t != null) r.BaseScale = t.localScale;
            }
        }

        void OnDestroy()
        {
            if (_playerHealth != null) _playerHealth.OnDie -= OnPlayerDie;
        }

        void Update()
        {
            if (_relay == null) return;

            // Detectar cambio de checkpoint
            if (_relay.CurrentCheckpointId != _lastCheckpointSeen)
            {
                _lastCheckpointSeen = _relay.CurrentCheckpointId;

                // Entrar a la sección (tocar CP4)
                if (_lastCheckpointSeen == StartCheckpointId && ResetWhenEnteringSection)
                {
                    ResetSectionState();
                    if (LogDebug) Debug.Log("[CP4 Assist] Entraste a la sección 4→5. Reset de ayudas/contador.");
                }

                // Completar la sección (tocar CP5 o superior)
                if (_lastCheckpointSeen >= EndCheckpointId)
                {
                    if (RevertWhenSectionCompleted)
                    {
                        RevertPlatforms();
                        if (LogDebug) Debug.Log("[CP4 Assist] Sección 4→5 completada. Revert ayudas.");
                    }
                }
            }
        }

        bool IsInsideSection()
        {
            // Consideramos "dentro" si: estás en CP4 y aún no alcanzaste CP5.
            return _relay != null &&
                   _relay.CurrentCheckpointId == StartCheckpointId &&
                   _lastCheckpointSeen < EndCheckpointId;
        }

        void OnPlayerDie()
        {
            if (!IsInsideSection()) return;

            _deathsInSection++;
            if (LogDebug) Debug.Log($"[CP4 Assist] Muerte #{_deathsInSection} en sección 4→5.");

            foreach (var r in Rules)
            {
                if (r.Applied || r.Platform == null) continue;

                if (_deathsInSection >= r.DeathsThreshold)
                {
                    var t = r.ScaleTarget != null ? r.ScaleTarget : r.Platform;
                    t.localScale = r.BaseScale * r.ScaleMultiplier;
                    r.Applied = true;

                    if (LogDebug) Debug.Log($"[CP4 Assist] Ayuda aplicada a '{t.name}': x{r.ScaleMultiplier} (umbral {r.DeathsThreshold}).", t);
                }
            }
        }

        public void ResetSectionState()
        {
            _deathsInSection = 0;
            foreach (var r in Rules)
            {
                r.Applied = false;
                var t = r.ScaleTarget != null ? r.ScaleTarget : r.Platform;
                if (t != null) t.localScale = r.BaseScale;
            }
        }

        public void RevertPlatforms()
        {
            foreach (var r in Rules)
            {
                var t = r.ScaleTarget != null ? r.ScaleTarget : r.Platform;
                if (t != null) t.localScale = r.BaseScale;
            }
        }
    }
}
