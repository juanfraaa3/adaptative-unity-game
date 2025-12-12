using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
    // Controla directamente MovingPlatform.speed según muertes entre StartCheckpointId (incl) y EndCheckpointId (excl).
    // Requiere Player con PlayerDeathRelay + Health y que tus checkpoints actualicen el Relay (CheckpointBridge).
    public class MovingPlatformSpeedAssist : MonoBehaviour
    {
        [Header("Sección por checkpoints")]
        public int StartCheckpointId = 6;   // pon aquí el inicio de tu sección
        public int EndCheckpointId = 7;   // y aquí el fin (exclusivo)

        [Header("Comportamiento")]
        public bool ResetWhenEnteringSection = true;
        public bool RevertWhenSectionCompleted = true;
        public bool LogDebug = true;

        [Header("Plataformas a controlar (MovingPlatform)")]
        public List<MovingPlatform> Platforms = new List<MovingPlatform>();

        [Serializable]
        public class SpeedRule
        {
            public int DeathsThreshold = 2;
            [Tooltip("Multiplicador de la velocidad base. 0.85 = 85%")]
            public float SpeedMultiplier = 0.85f;
            [HideInInspector] public bool Applied;
        }

        [Header("Escalones (recomendado: 2/4/6/8 muertes)")]
        public List<SpeedRule> Rules = new List<SpeedRule>()
        {
            new SpeedRule{DeathsThreshold=2, SpeedMultiplier=0.85f},
            new SpeedRule{DeathsThreshold=4, SpeedMultiplier=0.75f},
            new SpeedRule{DeathsThreshold=6, SpeedMultiplier=0.65f},
            new SpeedRule{DeathsThreshold=8, SpeedMultiplier=0.55f},
        };

        // cache de velocidades base
        private readonly List<float> _baseSpeeds = new List<float>();

        // refs jugador
        PlayerDeathRelay _relay;
        Health _playerHealth;
        int _lastCheckpoint = -1;
        int _deathsInSection = 0;

        void Awake()
        {
            _relay = FindObjectOfType<PlayerDeathRelay>();
            if (_relay != null) _lastCheckpoint = _relay.CurrentCheckpointId;

            _playerHealth = _relay ? _relay.GetComponent<Health>() : FindObjectOfType<Health>();
            if (_playerHealth) _playerHealth.OnDie += OnPlayerDie;

            // guarda velocidades base
            _baseSpeeds.Clear();
            foreach (var p in Platforms)
                _baseSpeeds.Add(p ? p.speed : 0f);
        }

        void OnDestroy()
        {
            if (_playerHealth) _playerHealth.OnDie -= OnPlayerDie;
        }

        void Update()
        {
            if (_relay == null) return;

            if (_relay.CurrentCheckpointId != _lastCheckpoint)
            {
                _lastCheckpoint = _relay.CurrentCheckpointId;

                if (_lastCheckpoint == StartCheckpointId && ResetWhenEnteringSection)
                {
                    ResetSectionState();
                    if (LogDebug) Debug.Log("[MP SpeedAssist] Entraste a la sección. Reset.");
                }

                if (_lastCheckpoint >= EndCheckpointId)
                {
                    if (RevertWhenSectionCompleted)
                    {
                        RevertAll();
                        if (LogDebug) Debug.Log("[MP SpeedAssist] Sección completada. Revert.");
                    }
                }
            }
        }

        bool InsideSection()
        {
            return _relay != null &&
                   _relay.CurrentCheckpointId == StartCheckpointId &&
                   _lastCheckpoint < EndCheckpointId;
        }

        void OnPlayerDie()
        {
            if (!InsideSection()) return;

            _deathsInSection++;
            if (LogDebug) Debug.Log($"[MP SpeedAssist] Muerte #{_deathsInSection}.");

            SpeedRule toApply = null;
            foreach (var r in Rules)
                if (_deathsInSection >= r.DeathsThreshold && !r.Applied)
                    toApply = r;

            if (toApply != null)
            {
                for (int i = 0; i < Platforms.Count; i++)
                {
                    var p = Platforms[i];
                    if (!p) continue;
                    var baseSpeed = _baseSpeeds[i];
                    p.speed = Mathf.Max(0.05f, baseSpeed * toApply.SpeedMultiplier); // clamp simple
                }

                toApply.Applied = true;
                if (LogDebug) Debug.Log($"[MP SpeedAssist] Aplicado x{toApply.SpeedMultiplier} (umbral {toApply.DeathsThreshold}).");
            }
        }

        public void ResetSectionState()
        {
            _deathsInSection = 0;
            foreach (var r in Rules) r.Applied = false;
            for (int i = 0; i < Platforms.Count; i++)
                if (Platforms[i]) Platforms[i].speed = _baseSpeeds[i];
        }

        public void RevertAll()
        {
            for (int i = 0; i < Platforms.Count; i++)
                if (Platforms[i]) Platforms[i].speed = _baseSpeeds[i];
        }
    }
}
