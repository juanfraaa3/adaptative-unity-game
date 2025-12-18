using Unity.FPS.Game;
using UnityEngine;
using System.Collections;

namespace Unity.FPS.Game
{
    public class ObjectiveElevatorPoints : MonoBehaviour
    {
        [Header("Platform Setup")]
        public Transform PlatformToMove;
        public Transform PointA;
        public Transform PointB;
        public float MoveDuration = 1.0f;
        public AnimationCurve Easing = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public bool PlaceAtAOnPlay = true;

        [Header("Player Setup")]
        public Health PlayerHealth;
        public GameObject PlayerObject;
        public CharacterController PlayerController;
        public Collider PlayerCollider;
        public float PlayerYOffset = 1.7f;

        [Header("Kill Zone Setup")]
        public GameObject KillZone;
        public float KillZoneActivationDelay = 0.75f;

        bool _movingUp;
        bool _movingDown;
        float _time;
        bool _alreadyUsed;
        bool _canGoDown;

        public static bool ElevatorIsMoving = false;
        bool _transportingPlayer = false;

        // 🆕 Offset exacto del jugador respecto al ascensor
        private Vector3 _playerLocalOffset;

        public bool HasAlreadyUsed => _alreadyUsed;

        void Start()
        {
            if (PlatformToMove == null)
                PlatformToMove = transform;

            if (PlaceAtAOnPlay && PointA != null)
                PlatformToMove.position = PointA.position;

            if (KillZone != null)
                KillZone.SetActive(false);

            if (PlayerHealth != null)
                PlayerHealth.OnDie += OnPlayerDeath;

            EventManager.AddListener<AllWavesCompletedEvent>(OnAllWavesCompleted);
        }

        void OnDestroy()
        {
            if (PlayerHealth != null)
                PlayerHealth.OnDie -= OnPlayerDeath;

            EventManager.RemoveListener<AllWavesCompletedEvent>(OnAllWavesCompleted);
        }

        void OnPlayerDeath()
        {
            ResetElevatorAndKillZone();
        }

        void OnAllWavesCompleted(AllWavesCompletedEvent evt)
        {
            Debug.Log("✔ Todas las waves completadas → ascensor puede bajar");
            _canGoDown = true;
        }

        // ===== SUBIR =====
        public void StartMoveUp()
        {
            if (!_alreadyUsed && PointA != null && PointB != null)
            {
                _alreadyUsed = true;
                _movingUp = true;
                _movingDown = false;
                _time = 0f;

                ElevatorIsMoving = true;
                Debug.Log("🔒 Inputs bloqueados (ascensor subiendo)");

                // 🚀 Transporte seguro activado
                _transportingPlayer = true;

                // 🆕 Guardar el offset exacto del jugador
                _playerLocalOffset = PlayerObject.transform.position - PlatformToMove.position;

                if (PlayerController != null)
                    PlayerController.enabled = false;

                if (PlayerCollider != null)
                    PlayerCollider.enabled = false;
            }
        }

        // ===== BAJAR =====
        public void StartMoveDown()
        {
            if (_canGoDown && PointA != null && PointB != null)
            {
                _movingDown = true;
                _movingUp = false;
                _time = 0f;

                ElevatorIsMoving = true;

                if (KillZone != null)
                    KillZone.SetActive(false);
            }
            else
            {
                Debug.Log("⛔ Intento de bajar, pero aún no se puede");
            }
        }

        // ===== UPDATE =====
        void Update()
        {
            // SUBIENDO
            if (_movingUp && PointA != null && PointB != null)
            {
                _time += Time.deltaTime;
                float t = Mathf.Clamp01(_time / MoveDuration);
                float e = Easing.Evaluate(t);

                // Mover plataforma
                PlatformToMove.position = Vector3.Lerp(PointA.position, PointB.position, e);
                Physics.SyncTransforms();

                // 🚀 Transporte seguro con offset exacto
                if (_transportingPlayer && PlayerObject != null)
                {
                    Vector3 targetPos = PlatformToMove.position + _playerLocalOffset;

                    // Altura mínima sobre el ascensor
                    float minY = PlatformToMove.position.y + PlayerYOffset;

                    // Si el offset Y original era mayor (por ejemplo porque el jugador estaba saltando),
                    // lo respetamos.
                    if (targetPos.y < minY)
                        targetPos.y = minY;

                    PlayerObject.transform.position = targetPos;

                }

                // Llegó arriba
                if (t >= 1f)
                {
                    _movingUp = false;
                    _time = 0f;
                    OnReachedTop();

                    ElevatorIsMoving = false;
                }
            }

            // BAJANDO
            else if (_movingDown && PointA != null && PointB != null)
            {
                _time += Time.deltaTime;
                float t = Mathf.Clamp01(_time / MoveDuration);
                float e = Easing.Evaluate(t);

                PlatformToMove.position = Vector3.Lerp(PointB.position, PointA.position, e);
                Physics.SyncTransforms();

                if (t >= 1f)
                {
                    _movingDown = false;
                    _time = 0f;

                    ElevatorIsMoving = false;
                }
            }
        }

        // ===== LLEGÓ ARRIBA =====
        void OnReachedTop()
        {
            if (KillZone != null)
                StartCoroutine(EnableKillZoneAfterDelay());

            EventManager.Broadcast(new ElevatorReachedTopEvent());
            Debug.Log("📢 Evento lanzado: Ascensor llegó al punto B");

            // 🔓 FIN transporte seguro
            _transportingPlayer = false;

            if (PlayerController != null)
                PlayerController.enabled = true;

            if (PlayerCollider != null)
                PlayerCollider.enabled = true;
        }

        IEnumerator EnableKillZoneAfterDelay()
        {
            yield return new WaitForSeconds(KillZoneActivationDelay);
            if (KillZone != null)
            {
                KillZone.SetActive(true);
                Debug.Log("KillZone ACTIVADA");
            }
        }

        // ===== RESET =====
        public void ResetElevatorAndKillZone()
        {
            if (PlatformToMove != null && PointA != null)
                PlatformToMove.position = PointA.position;

            _movingUp = _movingDown = false;
            _time = 0f;

            if (KillZone != null)
                KillZone.SetActive(false);

            _alreadyUsed = false;
            _canGoDown = false;

            ElevatorIsMoving = false;
            _transportingPlayer = false;

            if (PlayerController != null)
                PlayerController.enabled = true;

            if (PlayerCollider != null)
                PlayerCollider.enabled = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                other.transform.SetParent(PlatformToMove);
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
                other.transform.SetParent(null);
        }
    }
}
