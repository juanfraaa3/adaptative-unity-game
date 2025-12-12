using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    [RequireComponent(typeof(EnemyController))]
    public class EnemyTurretCopy : MonoBehaviour
    {
        public enum AIState
        {
            Idle,
            Attack,
        }

        [Header("Componentes del arma")]
        public Transform TurretPivot;
        public Transform TurretAimPoint;
        public Animator Animator;

        [Header("Ajustes de puntería")]
        public float AimRotationSharpness = 5f;
        public float LookAtRotationSharpness = 2.5f;
        public float DetectionFireDelay = 1f;
        public float AimingTransitionBlendTime = 1f;

        [Header("Efectos")]
        [Tooltip("Efectos visuales al recibir daño")]
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

        // --- 🔹 Predicción de movimiento / lead automático ---
        [Header("Predicción de disparo")]
        [Tooltip("Factor de suavizado de la velocidad del objetivo")]
        public float TargetVelSmoothing = 12f;

        Vector3 m_LastKnownTargetPos;
        Vector3 m_EstimatedTargetVelocity;
        bool m_HasLastTargetPos = false;
        float m_DynamicProjectileSpeed = 50f; // se ajustará automáticamente según el prefab del arma

        const string k_AnimOnDamagedParameter = "OnDamaged";
        const string k_AnimIsActiveParameter = "IsActive";

        void Start()
        {
            m_Health = GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, EnemyTurretCopy>(m_Health, this, gameObject);
            m_Health.OnDamaged += OnDamaged;

            m_EnemyController = GetComponent<EnemyController>();
            DebugUtility.HandleErrorIfNullGetComponent<EnemyController, EnemyTurretCopy>(m_EnemyController, this, gameObject);

            m_EnemyController.onDetectedTarget += OnDetectedTarget;
            m_EnemyController.onLostTarget += OnLostTarget;

            // Recordar offset entre el pivot y la dirección del arma
            m_RotationWeaponForwardToPivot =
                Quaternion.Inverse(m_EnemyController.GetCurrentWeapon().WeaponMuzzle.rotation) * TurretPivot.rotation;

            AiState = AIState.Idle;
            m_TimeStartedDetection = Mathf.NegativeInfinity;
            m_PreviousPivotAimingRotation = TurretPivot.rotation;
        }

        void Update()
        {
            UpdateCurrentAiState();
        }

        void LateUpdate()
        {
            UpdateTurretAiming();
        }

        // --- 🔹 Cálculo del tiempo de intercepción ---
        float ComputeInterceptTime(Vector3 relPos, Vector3 relVel, float projectileSpeed)
        {
            float a = Vector3.Dot(relVel, relVel) - projectileSpeed * projectileSpeed;
            float b = 2f * Vector3.Dot(relPos, relVel);
            float c = Vector3.Dot(relPos, relPos);

            float disc = b * b - 4f * a * c;
            if (Mathf.Abs(a) < 1e-6f || disc < 0f)
            {
                float fallback = relPos.magnitude / Mathf.Max(projectileSpeed, 0.01f);
                return Mathf.Max(0f, fallback);
            }

            float sqrtDisc = Mathf.Sqrt(disc);
            float t1 = (-b + sqrtDisc) / (2f * a);
            float t2 = (-b - sqrtDisc) / (2f * a);

            float t = (t1 > 0f && t2 > 0f) ? Mathf.Min(t1, t2) : Mathf.Max(t1, t2);
            return Mathf.Max(0f, t);
        }

        void UpdateCurrentAiState()
        {
            switch (AiState)
            {
                case AIState.Attack:
                    {
                        bool mustShoot = Time.time > m_TimeStartedDetection + DetectionFireDelay;

                        // --- Obtener datos base del arma ---
                        var weapon = m_EnemyController.GetCurrentWeapon();
                        Transform muzzle = weapon.WeaponMuzzle;
                        Transform targetTr = m_EnemyController.KnownDetectedTarget.transform;
                        Vector3 targetPos = targetTr.position;

                        // --- Obtener velocidad real del proyectil desde el prefab ---
                        if (weapon.ProjectilePrefab != null)
                        {
                            var proj = weapon.ProjectilePrefab.GetComponent<Unity.FPS.Game.ProjectileBase>();
                            float detectedSpeed = 0f;

                            if (proj != null)
                            {
                                // Intentar detectar diferentes nombres de campo o componentes
                                var projType = proj.GetType();

                                // 1️⃣ Campo o propiedad "InitialSpeed"
                                var fieldInitial = projType.GetField("InitialSpeed");
                                if (fieldInitial != null)
                                    detectedSpeed = (float)fieldInitial.GetValue(proj);

                                // 2️⃣ Campo o propiedad "Speed"
                                if (detectedSpeed == 0)
                                {
                                    var fieldSpeed = projType.GetField("Speed");
                                    if (fieldSpeed != null)
                                        detectedSpeed = (float)fieldSpeed.GetValue(proj);
                                }

                                // 3️⃣ Si el proyectil tiene Rigidbody, leer su velocidad inicial estimada
                                if (detectedSpeed == 0)
                                {
                                    Rigidbody rb = weapon.ProjectilePrefab.GetComponent<Rigidbody>();
                                    if (rb != null)
                                        detectedSpeed = rb.velocity.magnitude;
                                }
                            }

                            m_DynamicProjectileSpeed = detectedSpeed > 0f ? detectedSpeed : 50f;
                        }

                        // --- Estimar velocidad del objetivo (sirve aunque esté sobre una plataforma móvil) ---
                        if (m_HasLastTargetPos)
                        {
                            Vector3 rawVel = (targetPos - m_LastKnownTargetPos) / Mathf.Max(Time.deltaTime, 0.0001f);
                            m_EstimatedTargetVelocity = Vector3.Lerp(
                                m_EstimatedTargetVelocity,
                                rawVel,
                                1f - Mathf.Exp(-TargetVelSmoothing * Time.deltaTime)
                            );
                        }
                        else
                        {
                            m_EstimatedTargetVelocity = Vector3.zero;
                            m_HasLastTargetPos = true;
                        }
                        m_LastKnownTargetPos = targetPos;

                        // --- Calcular tiempo de intercepción ---
                        Vector3 relPos = targetPos - muzzle.position;
                        Vector3 relVel = m_EstimatedTargetVelocity;
                        float tIntercept = ComputeInterceptTime(relPos, relVel, Mathf.Max(m_DynamicProjectileSpeed, 0.01f));

                        // --- Posición predicha ---
                        Vector3 predictedPos = targetPos + m_EstimatedTargetVelocity * tIntercept;

                        // --- Rotación hacia el punto predicho ---
                        Vector3 dirToPredicted = (predictedPos - TurretAimPoint.position).normalized;
                        Quaternion offsettedTargetRotation =
                            Quaternion.LookRotation(dirToPredicted) * m_RotationWeaponForwardToPivot;

                        m_PivotAimingRotation = Quaternion.Slerp(
                            m_PreviousPivotAimingRotation,
                            offsettedTargetRotation,
                            (mustShoot ? AimRotationSharpness : LookAtRotationSharpness) * Time.deltaTime
                        );

                        // --- Disparo ---
                        if (mustShoot)
                        {
                            Vector3 correctedDirectionToTarget =
                                (m_PivotAimingRotation * Quaternion.Inverse(m_RotationWeaponForwardToPivot)) * Vector3.forward;

                            // Puedes alternar entre:
                            // m_EnemyController.TryAtack(predictedPos);   // Apunta directo al punto predicho
                            // o la dirección corregida (más suave):
                            m_EnemyController.TryAtack(TurretAimPoint.position + correctedDirectionToTarget);
                        }

                        break;
                    }
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
            {
                AiState = AIState.Attack;
            }

            for (int i = 0; i < OnDetectVfx.Length; i++)
            {
                OnDetectVfx[i].Play();
            }

            if (OnDetectSfx)
            {
                AudioUtility.CreateSFX(OnDetectSfx, transform.position, AudioUtility.AudioGroups.EnemyDetection, 1f);
            }

            Animator.SetBool(k_AnimIsActiveParameter, true);
            m_TimeStartedDetection = Time.time;
        }

        void OnLostTarget()
        {
            if (AiState == AIState.Attack)
            {
                AiState = AIState.Idle;
            }

            for (int i = 0; i < OnDetectVfx.Length; i++)
            {
                OnDetectVfx[i].Stop();
            }

            Animator.SetBool(k_AnimIsActiveParameter, false);
            m_TimeLostDetection = Time.time;
        }
    }
}
