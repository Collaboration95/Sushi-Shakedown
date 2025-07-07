using System.Collections.Specialized;
using UnityEngine;
public class OrderArea : MonoBehaviour
{
    private bool isOccupied = false;

    private float x, y;

    public void Awake()
    {
        x = transform.position.x;
        y = transform.position.y;
    }

    // Simply returns whether this area is free.
    public bool IsFree()
    {
        return !isOccupied;
    }

    // Returns the coordinates of this order area
    public Vector2 GetCoordinates()
    {
        return new Vector2(x, y);
    }

    public void UpdateState(bool occupied)
    {
        isOccupied = occupied;
        Debug.Log($"Order area {gameObject.name} is now {(occupied ? "occupied" : "free")}.");
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Customer"))
        {
            Debug.Log($"Customer entered order area: {gameObject.name}");
        }
    }
}
