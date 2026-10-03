using UnityEngine;

public class SpawnZoneData : MonoBehaviour
{
    private float minY;
    private float maxY;

    public float MinY => minY;
    public float MaxY => maxY;

    void Awake()
    {
        if (TryGetComponent<BoxCollider2D>(out var col))
        {
            minY = col.bounds.min.y;
            maxY = col.bounds.max.y;
        }
        else
        {
            Debug.LogError($"[SpawnZoneData] No BoxCollider2D found on {gameObject.name}");
        }
    }
}
