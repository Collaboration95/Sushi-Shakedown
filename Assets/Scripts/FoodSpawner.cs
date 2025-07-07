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
        Debug.Log(foodPrefab);
        GameObject newFood = Instantiate(foodPrefab, position, Quaternion.identity);

        //auto-pickup and drag immediately
        DraggableObject draggable = newFood.GetComponent<DraggableObject>();
        if (draggable != null)
        {
            draggable.TryPickUpThis();
        }
    }
}
