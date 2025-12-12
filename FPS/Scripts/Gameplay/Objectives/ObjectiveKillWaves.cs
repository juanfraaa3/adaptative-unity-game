using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveKillWaves : Objective
    {
        protected override void Start()
        {
            base.Start();

            EventManager.AddListener<AllWavesCompletedEvent>(OnAllWavesCompleted);

            if (string.IsNullOrEmpty(Title))
                Title = "Survive all waves";

            if (string.IsNullOrEmpty(Description))
                Description = "Defeat all enemy waves to complete the objective.";
        }

        void OnAllWavesCompleted(AllWavesCompletedEvent evt)
        {
            if (IsCompleted)
                return;

            CompleteObjective(string.Empty, string.Empty, "All waves cleared!");
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<AllWavesCompletedEvent>(OnAllWavesCompleted);
        }
    }
}
