using System;
using UnityEngine;

[Serializable]
public class EnemyInWave
{
    public GameObject enemyPrefab; // el prefab específico
    public int count = 1;          // cuántos de este prefab
}

[Serializable]
public class Wave
{
    public string waveName = "Nueva Oleada";
    public float spawnInterval = 2f;

    // 👇 ahora puedes mezclar varios prefabs en una misma wave
    public EnemyInWave[] enemies;
}
