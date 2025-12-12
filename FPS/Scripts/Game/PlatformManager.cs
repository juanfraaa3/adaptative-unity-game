using UnityEngine;

public class PlatformManager : MonoBehaviour
{
    public static PlatformManager Instance;

    public Transform[] Platforms; // Se llena automáticamente
    public int CurrentPlatformIndex = -1; // -1 = aún no partimos

    void Awake()
    {
        Instance = this;

        // Detectar todas las plataformas automáticamente
        Platforms = new Transform[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            var p = transform.GetChild(i);
            Platforms[i] = p;

            var id = p.gameObject.AddComponent<PlatformIdentifier>();
            id.PlatformIndex = i;
        }
    }

    public Transform GetNextPlatform()
    {
        int next = CurrentPlatformIndex + 1;

        if (next >= Platforms.Length)
            return null;

        return Platforms[next];
    }

    public void RegisterLanding(int platformIndex)
    {
        CurrentPlatformIndex = platformIndex;
    }
}
