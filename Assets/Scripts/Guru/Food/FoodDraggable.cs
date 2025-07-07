using UnityEngine;
using System.Collections;

public class FoodDraggable : Food
{
    private Vector3 originalPosition;
    private Vector3 offset;
    private bool isDragging = false;

    public Guru_FoodSpawner spawner;  // set by the spawner at Instantiate()
    public void CancelDrag()
    {
        isDragging = false;
        // disable the collider so we don’t retrigger anything
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        // disable this script so OnMouseDrag/OnMouseUp no longer fire
        enabled = false;
        // if you have any SmoothReturn coroutines running, you might want to stop them:
        StopAllCoroutines();
    }

    void Start()
    {
        // Optionally store a default position at Start if you like.
        // But if you want the "exact" position on each mouse click,
        // you can record it in OnMouseDown.
        originalPosition = transform.position;
    }

    void OnMouseDown()
    {
        // Calculate the offset so the object won't jump to mouse center.
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        offset = transform.position - mouseWorldPos;
        isDragging = true;
        // Debug.Log("Clicking on food item " + foodName);
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            // Follow the mouse, respecting the offset.
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            transform.position = mouseWorldPos + offset;
        }
    }

    void OnMouseUp()
    {
        isDragging = false;

        // For the simplest approach: always return to original position
        // (or check if it’s valid to stay if it’s in a drop zone).
        StartCoroutine(SmoothReturn(originalPosition, 0.3f));
    }

    private IEnumerator SmoothReturn(Vector3 targetPos, float duration)
    {
        Vector3 startPos = transform.position;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            // Smoothstep or just Lerp:
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
    }

    // Utility to get current mouse position in world space
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = 10f; // Distance from camera. Adjust as needed.
        return Camera.main.ScreenToWorldPoint(screenPos);
    }

    public void InformSpawner()
    {
        // Inform the spawner that this food item has been delivered
        if (spawner != null)
        {
            spawner.OnFoodDestroyed(this);
        }
    }

}
