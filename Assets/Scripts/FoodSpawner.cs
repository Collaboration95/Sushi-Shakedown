using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab; // Assign your food prefab in Inspector

    private void Start()
    {

    }

    private void Update()
    {
        
    }

    public void SpawnFoodAtCursor(Vector3 position)
    {
        if (foodPrefab == null || foodPrefab.GetComponent<DraggableObject>() == null)
        {
            Debug.LogError("FoodSpawner requires a prefab with DraggableObject.", this);
            return;
        }
        RuntimeLog.Write(foodPrefab);
        GameObject newFood = Instantiate(foodPrefab, position, Quaternion.identity);

        //auto-pickup and drag immediately
        DraggableObject draggable = newFood.GetComponent<DraggableObject>();
        if (draggable != null)
        {
            draggable.TryPickUpThis();
        }
    }
}
