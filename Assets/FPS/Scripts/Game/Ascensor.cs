using Unity.FPS.Game;
using UnityEngine;
using System.Collections;

namespace Unity.FPS.Game
{
    public class ObjectiveElevatorPoints : MonoBehaviour
    {
        [Header("Platform Setup")]
        public Transform PlatformToMove;
        public Transform PointA; // abajo
        public Transform PointB; // arriba
        public float MoveDuration = 1.0f;
        public AnimationCurve Easing = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public bool PlaceAtAOnPlay = true;

        [Header("Player Setup")]
        public Health PlayerHealth; // arrastra aquí el Health del jugador
        public GameObject PlayerObject; // arrastra aquí el Player (opcional)

        [Header("Kill Zone Setup")]
        public GameObject KillZone;
        public float KillZoneActivationDelay = 0.75f;

        bool _movingUp;
        bool _movingDown;
        float _time;
        bool _alreadyUsed;
        bool _canGoDown;

        // 🔒 Flag global para bloquear el crouch mientras el ascensor se mueve
        public static bool ElevatorIsMoving = false;

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

        // ===== MOVER HACIA ARRIBA =====
        public void StartMoveUp()
        {
            if (!_alreadyUsed && PointA != null && PointB != null)
            {
                _alreadyUsed = true;
                _movingUp = true;
                _movingDown = false;
                _time = 0f;

                ElevatorIsMoving = true; // 🚫 bloquear crouch mientras sube
                Debug.Log("🔒 Inputs bloqueados (ascensor subiendo)");
            }
        }

        // ===== MOVER HACIA ABAJO =====
        public void StartMoveDown()
        {
            if (_canGoDown && PointA != null && PointB != null)
            {
                _movingDown = true;
                _movingUp = false;
                _time = 0f;

                ElevatorIsMoving = true; // también bloquea mientras baja (puedes quitarlo si no quieres)

                if (KillZone != null)
                {
                    KillZone.SetActive(false);
                    Debug.Log("KillZone DESACTIVADA al comenzar descenso");
                }
            }
            else
            {
                Debug.Log("⛔ Intento de bajar, pero aún no se puede (waves no completadas)");
            }
        }

        void Update()
        {
            if (_movingUp && PointA != null && PointB != null)
            {
                _time += Time.deltaTime;
                float t = Mathf.Clamp01(_time / MoveDuration);
                float e = Easing.Evaluate(t);
                PlatformToMove.position = Vector3.Lerp(PointA.position, PointB.position, e);
                Physics.SyncTransforms(); // 🧱 fuerza actualización de físicas del ascensor

                if (t >= 1f)
                {
                    _movingUp = false;
                    _time = 0f;
                    OnReachedTop();

                    ElevatorIsMoving = false; // ✅ liberar input
                    Debug.Log("🔓 Inputs desbloqueados (ascensor arriba)");
                }
            }
            else if (_movingDown && PointA != null && PointB != null)
            {
                _time += Time.deltaTime;
                float t = Mathf.Clamp01(_time / MoveDuration);
                float e = Easing.Evaluate(t);
                PlatformToMove.position = Vector3.Lerp(PointB.position, PointA.position, e);
                Physics.SyncTransforms(); // 🧱 sincroniza físicas en el descenso también


                if (t >= 1f)
                {
                    _movingDown = false;
                    _time = 0f;

                    ElevatorIsMoving = false; // ✅ liberar input
                    Debug.Log("✔ Ascensor volvió a bajar y desbloqueó inputs");
                }
            }
        }

        // ===== LÓGICA KILLZONE =====
        void OnReachedTop()
        {
            if (KillZone != null)
                StartCoroutine(EnableKillZoneAfterDelay());

            EventManager.Broadcast(new ElevatorReachedTopEvent());
            Debug.Log("📢 Evento lanzado: Ascensor llegó al punto B");
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

        // ===== RESETEO =====
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

            ElevatorIsMoving = false; // ✅ asegurarse de liberar input

            Debug.Log("Ascensor reseteado y KillZone desactivada");
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.transform.SetParent(PlatformToMove);
                Debug.Log("👣 Jugador ahora hijo del ascensor");
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.transform.SetParent(null);
                Debug.Log("👣 Jugador liberado del ascensor");
            }
        }
    }
}
