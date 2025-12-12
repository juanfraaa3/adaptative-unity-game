using UnityEngine;
using Unity.FPS.Game;  // para acceder a Objective
using System.Linq;

public class ObjectiveWaveSync : MonoBehaviour
{
    public void SyncObjectiveToWave(int currentWave)
    {
        // Buscar todos los objectives en la escena
        var allObjectives = FindObjectsOfType<Objective>(true);

        // Desactivar todos los objectives activos
        foreach (var obj in allObjectives)
        {
            if (obj != null)
                obj.gameObject.SetActive(false);
        }

        // Buscar el correspondiente a la wave actual
        string targetName = $"ObjectiveComplete{GetWaveName(currentWave)}Wave";
        var target = allObjectives.FirstOrDefault(o => o.name == targetName);

        if (target != null)
        {
            target.gameObject.SetActive(true);
            Debug.Log($"🎯 Resync: activado {targetName}");
        }
        else
        {
            Debug.LogWarning($"⚠️ No se encontró Objective para la wave {currentWave} ({targetName})");
        }
    }

    private string GetWaveName(int wave)
    {
        switch (wave)
        {
            case 1: return "First";
            case 2: return "Second";
            case 3: return "Third";
            case 4: return "Fourth";
            case 5: return "Fifth";
            case 6: return "Sixth";
            default: return "Unknown";
        }
    }
}
