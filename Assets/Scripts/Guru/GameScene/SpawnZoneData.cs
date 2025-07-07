using UnityEngine;

public class SpawnZoneData : MonoBehaviour
{
    private float minY;
    private float maxY;

    public float MinY => minY;
    public float MaxY => maxY;

    void Start()
    {
        if (TryGetComponent<BoxCollider2D>(out var col))
        {
            float halfHeight = col.size.y * 0.5f;
            float centerY = transform.position.y + col.offset.y;

            minY = centerY - halfHeight;
            maxY = centerY + halfHeight;
        }
        else
        {
            Debug.LogError($"[SpawnZoneData] No BoxCollider2D found on {gameObject.name}");
        }
    }
}
