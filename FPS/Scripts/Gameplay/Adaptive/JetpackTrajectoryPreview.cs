using UnityEngine;
using System.Collections.Generic;

namespace Unity.FPS.Gameplay
{
    public class JetpackTrajectoryPreview : MonoBehaviour
    {
        [Header("References")]
        public Transform Player;
        public PlayerCharacterController PlayerController;
        public Jetpack Jetpack;
        public LineRenderer LineNormal;   // 🔹 Línea sin sprint
        public LineRenderer LineSprint;   // 🔹 Línea con sprint
        public LayerMask GroundMask;

        [Header("Simulation Settings")]
        public float SimulatedThrust = 15f;
        public float FuelDuration = 2.5f;
        public float TimeStep = 0.02f;
        public float Gravity = -9.81f;
        public int MaxPoints = 600;

        [Header("Marker Settings")]
        public GameObject MarkerPrefab;
        private GameObject markerNormal;
        private GameObject markerSprint;

        [Header("Direction Settings")]
        public Transform CameraTransform;
        public float ForwardBoost = 10f;       // normal
        public float SprintForwardBoost = 20f; // con sprint

        void Start()
        {
            // 🔹 Instanciar marcadores
            if (MarkerPrefab != null)
            {
                markerNormal = Instantiate(MarkerPrefab);
                markerNormal.transform.localScale = Vector3.one * 3f;
                markerNormal.GetComponent<Renderer>().material.color = Color.green;
                markerNormal.SetActive(false);

                markerSprint = Instantiate(MarkerPrefab);
                markerSprint.transform.localScale = Vector3.one * 3f;
                markerSprint.GetComponent<Renderer>().material.color = Color.cyan;
                markerSprint.SetActive(false);
            }

            if (LineNormal != null)
            {
                LineNormal.enabled = false;
                LineNormal.startColor = Color.green;
                LineNormal.endColor = Color.green;
            }

            if (LineSprint != null)
            {
                LineSprint.enabled = false;
                LineSprint.startColor = Color.cyan;
                LineSprint.endColor = Color.cyan;
            }
        }

        void Update()
        {
            if (Input.GetButtonDown("L1"))
            {
                ClearTrajectories();

                if (LineNormal != null) LineNormal.enabled = true;
                if (LineSprint != null) LineSprint.enabled = true;
                if (markerNormal != null) markerNormal.SetActive(true);
                if (markerSprint != null) markerSprint.SetActive(true);

                // 🔹 Determinar punto objetivo desde la mira
                Vector3 targetPoint = GetTargetPoint();

                // 🔹 Calcular trayectorias hacia ese punto
                bool reachedNormal = DrawPredictedTrajectory(LineNormal, markerNormal, ForwardBoost, targetPoint);
                bool reachedSprint = DrawPredictedTrajectory(LineSprint, markerSprint, SprintForwardBoost, targetPoint);

                // 🔹 Cambiar color si el punto no es alcanzable
                if (!reachedNormal)
                    markerNormal.GetComponent<Renderer>().material.color = Color.red;
                else
                    markerNormal.GetComponent<Renderer>().material.color = Color.green;

                if (!reachedSprint)
                    markerSprint.GetComponent<Renderer>().material.color = new Color(1f, 0.5f, 0f); // naranja si no llega
                else
                    markerSprint.GetComponent<Renderer>().material.color = Color.cyan;
            }
        }

        Vector3 GetTargetPoint()
        {
            if (CameraTransform == null)
                return Player.position + Player.forward * 20f;

            Ray ray = new Ray(CameraTransform.position, CameraTransform.forward);
            RaycastHit hit;

            // 🔹 Detectar todo excepto el jugador
            int layerMask = ~LayerMask.GetMask("Player");

            if (Physics.Raycast(ray, out hit, 200f, layerMask))
            {
                // Si golpea algo, devolver el punto de impacto
                Debug.DrawLine(ray.origin, hit.point, Color.yellow, 2f);
                return hit.point;
            }
            else
            {
                // Si no golpea nada, un punto lejano en la dirección de la cámara
                return CameraTransform.position + CameraTransform.forward * 100f;
            }
        }


        void ClearTrajectories()
        {
            if (LineNormal != null) LineNormal.positionCount = 0;
            if (LineSprint != null) LineSprint.positionCount = 0;
        }

        bool DrawPredictedTrajectory(LineRenderer line, GameObject marker, float boost, Vector3 targetPoint)
        {
            if (Player == null || line == null)
                return false;

            Vector3 forwardDir = (targetPoint - Player.position).normalized;
            List<Vector3> points = new List<Vector3>();

            Vector3 pos = Player.position + Vector3.up * 1.2f;
            // 🔹 Ajuste: menos empuje inicial horizontal para sprint, más realista
            float horizontalFactor = (boost == SprintForwardBoost) ? 0.55f : 1f;

            // 🔹 Ajuste: impulso inicial
            Vector3 vel = forwardDir * (boost * horizontalFactor) + Vector3.up * (SimulatedThrust * 0.5f);

            float fuel = FuelDuration;
            RaycastHit hit;
            bool hitGround = false;
            bool reachedTarget = false;

            for (int i = 0; i < MaxPoints; i++)
            {
                float thrustFactor = Mathf.Clamp01(fuel / FuelDuration);
                if (fuel > 0f)
                {
                    // 🔹 Aplicar empuje vertical normal, pero empuje horizontal reducido para sprint
                    vel += (Vector3.up * SimulatedThrust * thrustFactor) * TimeStep;

                    // 🔹 Reducir el “acelerón constante” horizontal
                    if (boost == SprintForwardBoost)
                        vel += forwardDir * (boost * 0.015f) * TimeStep;
                    else
                        vel += forwardDir * (boost * 0.05f) * TimeStep;

                    fuel -= TimeStep;
                }

                vel += Vector3.up * Gravity * TimeStep;
                pos += vel * TimeStep;
                points.Add(pos + Vector3.up * 0.05f);


                // ✅ Si está lo suficientemente cerca del objetivo
                if (Vector3.Distance(pos, targetPoint) < 0.8f)
                {
                    marker.transform.position = targetPoint + Vector3.up * 0.5f;
                    reachedTarget = true;
                    break;
                }

                // ✅ Si toca el suelo antes de llegar
                if (Physics.Raycast(pos, Vector3.down, out hit, 0.5f, GroundMask))
                {
                    marker.transform.position = hit.point + Vector3.up * 0.5f;
                    break;
                }
            }

            if (!reachedTarget && marker != null && points.Count > 0)
                marker.transform.position = points[points.Count - 1];

            line.positionCount = points.Count;
            line.SetPositions(points.ToArray());
            line.bounds = new Bounds(Player.position, Vector3.one * 200f);

            return reachedTarget;
        }

    }
}
