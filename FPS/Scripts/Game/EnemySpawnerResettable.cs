using System.Collections.Generic;
using UnityEngine;
using System;

namespace Unity.FPS.Game
{
    // Spawnea una oleada inicial y, cuando se pide ResetState, destruye vivos y vuelve a spawnear.
    public class EnemySpawnerResettable : MonoBehaviour
    {
        [Header("Checkpoint al que pertenece esta zona")]
        public int CheckpointId = 0;

        [System.Serializable]
        public struct SpawnInfo
        {
            public GameObject Prefab;  // Prefab del enemigo
            public Transform Point;    // Punto de aparición (si es null, usa este transform)
        }

        [Header("Oleada inicial")]
        public List<SpawnInfo> InitialWave = new List<SpawnInfo>();

        private readonly List<GameObject> _live = new List<GameObject>();
        public event Action OnEnemiesReset;
        private void Start()
        {
            SpawnInitial();
        }

        private void SpawnInitial()
        {
            foreach (var s in InitialWave)
            {
                if (s.Prefab == null) continue;
                var p = s.Point != null ? s.Point : transform;
                var e = Instantiate(s.Prefab, p.position, p.rotation, transform);
                _live.Add(e);
            }
        }

        public void ResetState()
        {
            for (int i = _live.Count - 1; i >= 0; i--)
            {
                if (_live[i] != null) Destroy(_live[i]);
                _live.RemoveAt(i);
            }
            SpawnInitial();

            OnEnemiesReset?.Invoke();
        }
    }
}
