using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class JetpackLandingPredictor : MonoBehaviour
    {
        [Header("References")]
        public Transform Player;                     // Transform del jugador
        public PlayerCharacterController PlayerController;  // tu script de movimiento
        public GameObject MarkerPrefab;              // Prefab del marcador

        [Header("Prediction Settings")]
        public int MaxIterations = 200;
        public float TimeStep = 0.05f;
        public float Gravity = -9.81f;
        public LayerMask GroundMask;

        private GameObject _markerInstance;

        void Start()
        {
            if (MarkerPrefab)
                _markerInstance = Instantiate(MarkerPrefab);
        }

        void Update()
        {
            Vector3 predictedPoint = PredictLandingPoint();
            if (predictedPoint != Vector3.zero)
            {
                _markerInstance.SetActive(true);
                _markerInstance.transform.position = predictedPoint + Vector3.up * 0.05f;
            }
            else
            {
                _markerInstance.SetActive(false);
            }
        }

        Vector3 PredictLandingPoint()
        {
            if (PlayerController == null)
                return Vector3.zero;

            Vector3 pos = Player.position;
            Vector3 vel = PlayerController.CharacterVelocity; // usa la velocidad del script de movimiento

            for (int i = 0; i < MaxIterations; i++)
            {
                vel += Vector3.up * Gravity * TimeStep;
                pos += vel * TimeStep;

                if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 0.3f, GroundMask))
                {
                    return hit.point;
                }
            }

            return Vector3.zero;
        }
    }
}
