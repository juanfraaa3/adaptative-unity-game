using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    private Vector3 lastCheckpointPosition;
    private Quaternion lastCheckpointRotation;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Opcional si hay cambios de escena
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Guarda posición y rotación del jugador al alcanzar un checkpoint
    public void SetCheckpoint(Vector3 position, Quaternion rotation)
    {
        lastCheckpointPosition = position;
        lastCheckpointRotation = rotation;
    }

    public Vector3 GetCheckpoint()
    {
        return lastCheckpointPosition;
    }

    public Quaternion GetCheckpointRotation()
    {
        return lastCheckpointRotation;
    }
}
