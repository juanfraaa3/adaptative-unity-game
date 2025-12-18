using System.Globalization;
using System.IO;
using UnityEngine;

public class MultitaskMetricsLogger : MonoBehaviour
{
    public PlatformPathAdapter path;
    public bool loggingActive = true;

    private int AttemptID = 1;
    private float attemptStartTime;

    private float devSum = 0f;
    private int devSamples = 0;

    private string fullPath;
    private CultureInfo CI = CultureInfo.InvariantCulture;
    // ---- Lap / Attempt state
    private int lapsCompleted = 0;
    private float lapProgress01 = 0f;
    private bool deathOccurred = false;
    private Vector3 _lastPos;
    private Vector3 _flatVel;
    private bool _hasLastPos = false;
    // ---- Speed variability
    private float speedSum = 0f;
    private float speedSqSum = 0f;
    private int speedSamples = 0;
    // ---- Micro-corrections
    private int microCorrectionsCount = 0;
    private Vector3 lastMoveDir = Vector3.zero;
    private bool hasLastMoveDir = false;
    // ---- Micro-correction thresholds
    private const float MIN_SPEED_FOR_MICRO = 0.3f;   // m/s
    private const float MIN_DIR_CHANGE_DEG = 8f;      // grados
    private const float MAX_DIR_CHANGE_DEG = 45f;     // grados

    // ---- Obstacles
    private int obstacleHits = 0;
    private int obstacleOpportunities = 0;


    void Start()
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string fileName = $"Multitasking_{timestamp}.csv";

        string basePath = Path.Combine(
            Application.dataPath,
            "FPS/Scripts/MultitaskSystem"
        );

        Directory.CreateDirectory(basePath);

        fullPath = Path.Combine(basePath, fileName);

        if (!File.Exists(fullPath))
        {
            File.WriteAllText(
                fullPath,
                "AttemptID;AttemptDuration_s;LapsCompleted;LapProgress_0_1;DeathOccurred;MeanDeviation_m;SpeedSD_mps;MicroCorrectionsRate_per_s;ObstacleHitRate\n"
            );
        }

        // 🔴 CONTAR OBSTÁCULOS UNA SOLA VEZ
        obstacleOpportunities = FindObjectsOfType<ObstacleHitReporter>().Length;

        StartAttempt();

        Debug.Log($"[MULTITASK] Obstacles detected: {obstacleOpportunities}");
        Debug.Log("CSV PATH: " + fullPath);
    }




    void Update()
    {
        if (!loggingActive || path == null)
            return;

        Vector3 pos = transform.position;

        /* =====================================================
         * 1) VELOCIDAD PLANA (XZ)
         * ===================================================== */
        if (_hasLastPos)
        {
            Vector3 delta = pos - _lastPos;
            delta.y = 0f;

            _flatVel = delta / Mathf.Max(Time.deltaTime, 0.0001f);
        }
        else
        {
            _flatVel = Vector3.zero;
            _hasLastPos = true;
        }

        _lastPos = pos;

        float speed = _flatVel.magnitude;

        // ---- Speed Variability accumulation
        speedSum += speed;
        speedSqSum += speed * speed;
        speedSamples++;

        /* =====================================================
         * 2) MICRO-CORRECTIONS DETECTION
         * ===================================================== */
        if (speed > MIN_SPEED_FOR_MICRO)
        {
            Vector3 dir = _flatVel.normalized;

            if (hasLastMoveDir)
            {
                float angle = Vector3.Angle(lastMoveDir, dir);

                if (angle > MIN_DIR_CHANGE_DEG && angle < MAX_DIR_CHANGE_DEG)
                {
                    microCorrectionsCount++;
                }
            }

            lastMoveDir = dir;
            hasLastMoveDir = true;
        }
        else
        {
            hasLastMoveDir = false;
        }

        /* =====================================================
         * 3) MEAN DEVIATION (ignorando Y)
         * ===================================================== */
        Vector3 closest = path.GetClosestPoint(pos);

        Vector3 flatPos = new Vector3(pos.x, 0f, pos.z);
        Vector3 flatClosest = new Vector3(closest.x, 0f, closest.z);

        float deviation = Vector3.Distance(flatPos, flatClosest);

        devSum += deviation;
        devSamples++;

        /* =====================================================
         * 4) PROGRESO DE VUELTA (0–1)
         * ===================================================== */
        lapProgress01 = path.GetProgress01(pos);
    }



    public void StartAttempt()
    {
        attemptStartTime = Time.time;

        // ---- Deviation
        devSum = 0f;
        devSamples = 0;

        // ---- Speed
        speedSum = 0f;
        speedSqSum = 0f;
        speedSamples = 0;

        // ---- Micro-corrections
        microCorrectionsCount = 0;
        lastMoveDir = Vector3.zero;
        hasLastMoveDir = false;

        // ---- Lap state
        lapsCompleted = 0;
        lapProgress01 = 0f;
        deathOccurred = false;

        // ---- Obstacles (SOLO HITS, no opportunities)
        obstacleHits = 0;

        // Resetear estado interno de cada obstáculo
        foreach (var obs in FindObjectsOfType<ObstacleHitReporter>())
        {
            obs.ResetForNewAttempt();
        }
    }


    public void EndAttempt(bool died)
    {
        deathOccurred = died;

        float duration = Time.time - attemptStartTime;

        float meanDev = devSamples > 0 ? devSum / devSamples : 0f;

        float speedSD = 0f;
        if (speedSamples > 1)
        {
            float meanSpeed = speedSum / speedSamples;
            float variance = (speedSqSum / speedSamples) - (meanSpeed * meanSpeed);
            speedSD = Mathf.Sqrt(Mathf.Max(variance, 0f));
        }

        float microRate = duration > 0f
            ? microCorrectionsCount / duration
            : 0f;

        // 🔴 OBSTACLE HIT RATE
        float obstacleHitRate = obstacleOpportunities > 0
            ? (float)obstacleHits / obstacleOpportunities
            : 0f;

        string line =
            AttemptID + ";" +
            duration.ToString("F3", CI) + ";" +
            lapsCompleted + ";" +
            lapProgress01.ToString("F3", CI) + ";" +
            (deathOccurred ? 1 : 0) + ";" +
            meanDev.ToString("F4", CI) + ";" +
            speedSD.ToString("F4", CI) + ";" +
            microRate.ToString("F4", CI) + ";" +
            obstacleHitRate.ToString("F3", CI);

        File.AppendAllText(fullPath, line + "\n");

        AttemptID++;
        StartAttempt();
    }

    public void NotifyLapCrossed()
    {
        lapsCompleted++;
        lapProgress01 = 0f;
        Debug.Log("Lap crossed. Total laps: " + lapsCompleted);
    }
    public Vector3 GetFlatVelocity()
    {
        return _flatVel;
    }
    public void NotifyObstacleSpawned()
    {
        obstacleOpportunities++;
    }

    public void NotifyObstacleHit()
    {
        obstacleHits++;
    }


}
