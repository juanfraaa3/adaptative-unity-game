using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Setup")]
    public GameObject enemyPrefab;   // Prefab por defecto
    public Transform spawnPoint;     // Punto exacto de spawn (si no, usa la posición del spawner)
    public Transform orbitCenter;    // Centro para rotadores/wavers

    public GameObject SpawnEnemy(GameObject overridePrefab = null)
    {
        // Usa el prefab pasado por parámetro si existe, si no usa el que ya tiene asignado
        GameObject prefabToSpawn = overridePrefab != null ? overridePrefab : enemyPrefab;
        if (prefabToSpawn == null)
        {
            Debug.LogWarning("[Spawner] ⚠️ No hay prefab asignado.");
            return null;
        }

        Vector3 p = spawnPoint ? spawnPoint.position : transform.position;

        // Instanciar primero
        var go = Instantiate(prefabToSpawn, p, Quaternion.identity);

        // 🔥 Debug inicial (antes de ajustar al NavMesh)
        Debug.Log($"[Spawner] Enemigo {go.name} instanciado en {p}");

        // Si el enemigo usa NavMeshAgent → ajustarlo al suelo
        var agent = go.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            if (NavMesh.SamplePosition(p, out var hit, 3f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
                Debug.Log($"[Spawner] Enemigo {go.name} ajustado al NavMesh en {hit.position}");
            }
            else
            {
                Debug.LogWarning($"[Spawner] ❌ No hay NavMesh cerca de {p}. El enemigo puede quedar en el aire.");
            }
        }

        // Si es circular (ConstantCircularPatrol)
        var patrol = go.GetComponent<ConstantCircularPatrol>();
        if (patrol != null && orbitCenter != null)
        {
            patrol.useFixedCenter = false;
            patrol.center = orbitCenter;
        }

        // Si es Waver (WavePatrol)
        var wave = go.GetComponent<WavePatrol>();
        if (wave != null && orbitCenter != null)
        {
            wave.useFixedCenter = false;
            wave.center = orbitCenter;
        }

        return go;
    }

    // 🔴 Dibujar un Gizmo en la escena para ver dónde spawnean los enemigos
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 pos = spawnPoint ? spawnPoint.position : transform.position;
        Gizmos.DrawWireSphere(pos, 0.5f); // círculo rojo
    }
}
