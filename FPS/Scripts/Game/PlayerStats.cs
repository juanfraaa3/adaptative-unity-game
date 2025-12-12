using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Collections;

namespace Unity.FPS.Game
{
    public class PlayerStats : MonoBehaviour
    {
        public static PlayerStats Instance;

        private float lastCheckpointTime = 0f;
        private List<string> csvLines = new List<string>();
        private string filePath;
        private bool firstCheckpointActivated = false;

        // ===== Checkpoints =====
        private HashSet<string> checkpointsRegistrados = new HashSet<string>();

        // ===== Muertes =====
        private int deathsSinceLastCheckpoint = 0;
        private int totalDeaths = 0;

        // ===== Tipos de muertes =====
        private int deathsByEnemiesSinceLastCheckpoint = 0;
        private int deathsByVoidSinceLastCheckpoint = 0;
        private int totalDeathsByEnemies = 0;
        private int totalDeathsByVoid = 0;

        // ===== Ayuda adaptativa =====
        public static int ConsecutiveVoidDeaths = 0;


        // ===== Enemigos eliminados =====
        private int enemiesKilledSinceLastCheckpoint = 0;
        private int totalEnemiesKilled = 0;

        // ===== Blaster =====
        private int blasterShotsFired = 0;
        private int blasterHits = 0;

        // ===== Escopeta =====
        private int shotgunShotsFired = 0;
        private int shotgunHits = 0;
        private int shotgunClusterHits = 0;
        private float lastShotgunHitTime = 0f;
        private int shotgunCount = 0;

        // ===== Configuración =====
        private const float CLUSTER_WINDOW = 0.25f;
        private const int SHOTGUN_PELLETS = 24;
        private const float SHOTGUN_THRESHOLD = 0.5f;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            DontDestroyOnLoad(gameObject);

            string folderPath = Path.Combine(Application.dataPath, "FPS/Scripts/Analytics/Registers");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fileName = "PlayerStats_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".csv";
            filePath = Path.Combine(folderPath, fileName);

            csvLines.Add(
                "Checkpoint,Tiempo Absoluto (seg),Tiempo Desde Anterior (seg)," +
                "Muertes entre checkpoints,Muertes totales,Muertes por enemigos,Muertes por vacío," +
                "Enemigos eliminados," +
                "Disparos Blaster,Acertados Blaster,Fallados Blaster,Precisión Blaster (%)," +
                "Disparos Escopeta,Acertados Escopeta,Fallados Escopeta,Precisión Escopeta (%)," +
                "Disparos Totales,Acertados Totales,Fallados Totales,Precisión Total (%)"
            );

            //Debug.Log("📊 PlayerStats inicializado con columna de enemigos eliminados (vFinal).");
        }

        // ===============================
        // REGISTROS PRINCIPALES
        // ===============================

        public void RegisterShot(string weaponName)
        {
            if (!weaponName.ToLower().Contains("shotgun"))
                blasterShotsFired++;
        }

        public void RegisterHit(string weaponName)
        {
            if (weaponName.ToLower().Contains("shotgun"))
            {
                float currentTime = Time.time;

                // 🔹 Si el cluster anterior ya pasó su ventana, ciérralo
                if (currentTime - lastShotgunHitTime > CLUSTER_WINDOW && shotgunClusterHits > 0)
                {
                    FinalizeShotgunCluster();
                }

                // Contar impacto actual (solo se llama desde ProjectileStandard cuando el impacto es válido)
                shotgunClusterHits++;
                lastShotgunHitTime = currentTime;
            }
            else
            {
                blasterHits++;
            }
        }

        // 🔹 Registrar escopetazo aunque no haya tenido impactos válidos
        public void RegisterShotgunMiss()
        {
            // Cerrar clusters viejos si ya expiran
            if (shotgunClusterHits > 0 && Time.time - lastShotgunHitTime > CLUSTER_WINDOW)
            {
                FinalizeShotgunCluster();
            }

            // Iniciar verificación diferida del fallo total
            StartCoroutine(VerifyShotgunMissAfterDelay());
        }

        private IEnumerator VerifyShotgunMissAfterDelay()
        {
            float startTime = Time.time;

            // Esperar un poco más que la ventana de agrupamiento
            yield return new WaitForSeconds(CLUSTER_WINDOW + 0.1f);

            // Si no hubo impactos válidos desde que se disparó, registrar fallo completo
            if (Time.time - lastShotgunHitTime > CLUSTER_WINDOW && shotgunClusterHits == 0)
            {
                shotgunShotsFired++;
                shotgunCount++;
                //Debug.Log($"[Shotgun #{shotgunCount}] ❌ Escopetazo completamente fallado (0/{SHOTGUN_PELLETS})");
                SaveCSV();
            }
        }

        private void FinalizeShotgunCluster()
        {
            if (shotgunClusterHits <= 0) return;

            shotgunShotsFired++;
            shotgunCount++;

            float ratio = (float)shotgunClusterHits / SHOTGUN_PELLETS;
            bool acertado = ratio >= SHOTGUN_THRESHOLD;

            if (acertado)
            {
                shotgunHits++;
                //Debug.Log($"[Shotgun #{shotgunCount}] ✅ Escopetazo acertado ({shotgunClusterHits}/{SHOTGUN_PELLETS}) → {ratio * 100f:F1}%");
            }
            else
            {
                //Debug.Log($"[Shotgun #{shotgunCount}] ❌ Escopetazo fallado ({shotgunClusterHits}/{SHOTGUN_PELLETS}) → {ratio * 100f:F1}%");
            }

            shotgunClusterHits = 0;
            SaveCSV();
        }

        // ===============================
        // MUERTES
        // ===============================

        public void RegisterDeath()
        {
            deathsSinceLastCheckpoint++;
            totalDeaths++;
        }

        public void RegisterEnemyDeath()
        {
            deathsByEnemiesSinceLastCheckpoint++;
            totalDeathsByEnemies++;
            RegisterDeath();

            // Reiniciar racha de muertes por vacío
            ConsecutiveVoidDeaths = 0;
        }

        public void RegisterVoidDeath()
        {
            deathsByVoidSinceLastCheckpoint++;
            totalDeathsByVoid++;
            RegisterDeath();

            ConsecutiveVoidDeaths++;
            //Debug.Log($"💀 Muerte por vacío #{ConsecutiveVoidDeaths} consecutiva.");
        }

        // ===============================
        // ENEMIGOS ELIMINADOS
        // ===============================

        public void RegisterEnemyKill()
        {
            enemiesKilledSinceLastCheckpoint++;
            totalEnemiesKilled++;
        }

        // ===============================
        // CHECKPOINTS
        // ===============================

        public void RegisterCheckpoint(string checkpointName)
        {
            if (checkpointsRegistrados.Contains(checkpointName))
                return;

            // Cerrar clusters activos antes de guardar
            if (shotgunClusterHits > 0)
                FinalizeShotgunCluster();

            checkpointsRegistrados.Add(checkpointName);

            float currentTime = Time.time;
            float delta = firstCheckpointActivated ? currentTime - lastCheckpointTime : 0f;

            // Calcular estadísticas
            int blasterMisses = Mathf.Max(0, blasterShotsFired - blasterHits);
            int shotgunMisses = Mathf.Max(0, shotgunShotsFired - shotgunHits);

            int totalShots = blasterShotsFired + shotgunShotsFired;
            int totalHits = blasterHits + shotgunHits;
            int totalMisses = blasterMisses + shotgunMisses;

            float accBlaster = CalculateAccuracy(blasterShotsFired, blasterHits);
            float accShotgun = CalculateAccuracy(shotgunShotsFired, shotgunHits);
            float accTotal = CalculateAccuracy(totalShots, totalHits);

            // Guardar línea CSV
            csvLines.Add(string.Format(
                CultureInfo.InvariantCulture,
                "{0},{1:F2},{2:F2},{3},{4},{5},{6},{7}," +
                "{8},{9},{10},{11:F1}," +
                "{12},{13},{14},{15:F1}," +
                "{16},{17},{18},{19:F1}",
                checkpointName, currentTime, delta,
                deathsSinceLastCheckpoint, totalDeaths,
                deathsByEnemiesSinceLastCheckpoint, deathsByVoidSinceLastCheckpoint,
                enemiesKilledSinceLastCheckpoint,
                blasterShotsFired, blasterHits, blasterMisses, accBlaster,
                shotgunShotsFired, shotgunHits, shotgunMisses, accShotgun,
                totalShots, totalHits, totalMisses, accTotal
            ));

            // Reiniciar contadores
            firstCheckpointActivated = true;
            lastCheckpointTime = currentTime;
            deathsSinceLastCheckpoint = 0;
            deathsByEnemiesSinceLastCheckpoint = 0;
            deathsByVoidSinceLastCheckpoint = 0;
            enemiesKilledSinceLastCheckpoint = 0;
            blasterShotsFired = 0;
            blasterHits = 0;
            shotgunShotsFired = 0;
            shotgunHits = 0;

            SaveCSV();
            //Debug.Log($"✅ Checkpoint '{checkpointName}' registrado con métricas y enemigos eliminados.");
        }

        // ===============================
        // UTILIDADES
        // ===============================

        private float CalculateAccuracy(int fired, int hits)
        {
            if (fired == 0) return 0f;
            return ((float)hits / fired) * 100f;
        }

        private void SaveCSV()
        {
            File.WriteAllText(filePath, string.Join("\n", csvLines).Replace(",", ";"));

        }

        void OnApplicationQuit()
        {
            if (shotgunClusterHits > 0)
                FinalizeShotgunCluster();

            SaveCSV();
            //Debug.Log("💾 CSV guardado correctamente (auto y en cierre).");
        }
    }
}
