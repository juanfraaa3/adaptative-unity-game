using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    [RequireComponent(typeof(EnemyController))]
    public class EnemyTurret : MonoBehaviour
    {
        public enum AIState
        {
            Idle,
            Attack,
        }

        [Header("Turret Components")]
        public Transform TurretPivot;
        public Transform TurretAimPoint;
        public Animator Animator;

        [Header("Aiming Settings")]
        [Tooltip("Altura relativa donde la torreta apunta (1.6 = cabeza, 0.8 = pecho, 0 = pies)")]
        public float AimVerticalOffset = 0.8f;
        public float AimRotationSharpness = 5f;
        public float LookAtRotationSharpness = 2.5f;
        public float DetectionFireDelay = 1f;
        public float AimingTransitionBlendTime = 1f;

        [Header("Prediction Settings")]
        [Tooltip("Cuánto tiempo anticipa la torreta el movimiento del jugador (en segundos)")]
        public float PredictionTime = 0.3f;

        [Header("FX & Audio")]
        public ParticleSystem[] RandomHitSparks;
        public ParticleSystem[] OnDetectVfx;
        public AudioClip OnDetectSfx;

        public AIState AiState { get; private set; }

        EnemyController m_EnemyController;
        Health m_Health;
        Quaternion m_RotationWeaponForwardToPivot;
        float m_TimeStartedDetection;
        float m_TimeLostDetection;
        Quaternion m_PreviousPivotAimingRotation;
        Quaternion m_PivotAimingRotation;

        Transform m_PlayerTransform;
        Vector3 m_LastPlayerPos;
        Vector3 m_PlayerVelocity;

        const string k_AnimOnDamagedParameter = "OnDamaged";
        const string k_AnimIsActiveParameter = "IsActive";

        void Start()
        {
            m_Health = GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, EnemyTurret>(m_Health, this, gameObject);
            m_Health.OnDamaged += OnDamaged;

            m_EnemyController = GetComponent<EnemyController>();
            DebugUtility.HandleErrorIfNullGetComponent<EnemyController, EnemyTurret>(m_EnemyController, this, gameObject);

            m_EnemyController.onDetectedTarget += OnDetectedTarget;
            m_EnemyController.onLostTarget += OnLostTarget;

            // 🔹 Recalcular el offset de rotación correctamente para cada torreta
            var muzzle = m_EnemyController.GetCurrentWeapon().WeaponMuzzle;
            m_RotationWeaponForwardToPivot = Quaternion.Inverse(muzzle.rotation) * TurretPivot.rotation;

            AiState = AIState.Idle;
            m_TimeStartedDetection = Mathf.NegativeInfinity;
            m_PreviousPivotAimingRotation = TurretPivot.rotation;

            // 🔹 Buscar al jugador real
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player)
            {
                m_PlayerTransform = player.transform;
                m_LastPlayerPos = m_PlayerTransform.position;
            }
        }

        void LateUpdate()
        {
            // Calcular velocidad del jugador
            if (m_PlayerTransform)
            {
                m_PlayerVelocity = (m_PlayerTransform.position - m_LastPlayerPos) / Time.deltaTime;
                m_LastPlayerPos = m_PlayerTransform.position;
            }

            // Apuntar y disparar sincronizado
            UpdateCurrentAiState();
            UpdateTurretAiming();
        }

        void UpdateCurrentAiState()
        {
            switch (AiState)
            {
                case AIState.Attack:
                    bool mustShoot = Time.time > m_TimeStartedDetection + DetectionFireDelay;

                    if (!m_PlayerTransform)
                        return;

                    // 🎯 Predicción del movimiento del jugador
                    Vector3 predictedPos = m_PlayerTransform.position + m_PlayerVelocity * PredictionTime;
                    predictedPos += Vector3.up * AimVerticalOffset;

                    // Calcular dirección al punto predicho
                    Vector3 directionToTarget = (predictedPos - TurretAimPoint.position).normalized;

                    Quaternion offsettedTargetRotation =
                        Quaternion.LookRotation(directionToTarget) * m_RotationWeaponForwardToPivot;

                    m_PivotAimingRotation = Quaternion.Slerp(
                        m_PreviousPivotAimingRotation,
                        offsettedTargetRotation,
                        (mustShoot ? AimRotationSharpness : LookAtRotationSharpness) * Time.deltaTime
                    );

                    // ====================== DEBUG VISUAL ======================
                    Debug.DrawLine(TurretAimPoint.position, predictedPos, Color.red);  // hacia el punto futuro
                    Debug.DrawLine(TurretAimPoint.position, m_PlayerTransform.position, Color.green); // posición actual
                    Debug.DrawRay(TurretAimPoint.position, directionToTarget * 10f, Color.blue);
                    // ==========================================================

                    // 🚀 Disparo sincronizado (al punto futuro del jugador)
                    if (mustShoot)
                    {
                        m_EnemyController.TryAtack(predictedPos);
                    }

                    break;
            }
        }

        void UpdateTurretAiming()
        {
            switch (AiState)
            {
                case AIState.Attack:
                    TurretPivot.rotation = m_PivotAimingRotation;
                    break;

                default:
                    TurretPivot.rotation = Quaternion.Slerp(
                        m_PivotAimingRotation,
                        TurretPivot.rotation,
                        (Time.time - m_TimeLostDetection) / AimingTransitionBlendTime
                    );
                    break;
            }

            m_PreviousPivotAimingRotation = TurretPivot.rotation;
        }

        void OnDamaged(float dmg, GameObject source)
        {
            if (RandomHitSparks.Length > 0)
            {
                int n = Random.Range(0, RandomHitSparks.Length - 1);
                RandomHitSparks[n].Play();
            }
            Animator.SetTrigger(k_AnimOnDamagedParameter);
        }

        void OnDetectedTarget()
        {
            if (AiState == AIState.Idle)
                AiState = AIState.Attack;

            foreach (var vfx in OnDetectVfx)
                vfx.Play();

            if (OnDetectSfx)
                AudioUtility.CreateSFX(OnDetectSfx, transform.position, AudioUtility.AudioGroups.EnemyDetection, 1f);

            Animator.SetBool(k_AnimIsActiveParameter, true);
            m_TimeStartedDetection = Time.time;
        }

        void OnLostTarget()
        {
            if (AiState == AIState.Attack)
                AiState = AIState.Idle;

            foreach (var vfx in OnDetectVfx)
                vfx.Stop();

            Animator.SetBool(k_AnimIsActiveParameter, false);
            m_TimeLostDetection = Time.time;
        }
    }
}
