using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderBubble : DebuggableMonoBehaviour
{
    private readonly int maxSlots = 3;
    public float slotSpacing = 0f; // Adjust this to control vertical distance
    public float deliveryAnimationDuration = 0.2f;
    public CustomerController customerController;
    private SpriteRenderer selfSpriteRenderer;
    // List to store ordered food items.
    private List<Food> orderedFoods = new List<Food>();

    public FoodManager FM;

    protected override void Awake()
    {
        base.Awake();
        selfSpriteRenderer = GetComponent<SpriteRenderer>();
        if (selfSpriteRenderer == null)
        {
            Debug.Log("OrderBubble: No SpriteRenderers found!");
        }
        if (customerController == null)
        {
            // try auto‑find on parent
            customerController = GetComponentInParent<CustomerController>();
        }
        if (customerController == null)
        {
            Debug.LogWarning("OrderBubble: No CustomerController assigned or found in parents!");
        }
    }

    public void Start()
    {
        var gameManager = GameObject.Find("GuruGameManager");
        if (gameManager == null)
        {
            Debug.Log("OrderBubble: GameManager not found in scene!");
            return;
        }

        FM = gameManager.GetComponent<FoodManager>();
        if (FM == null)
        {
            Debug.Log("OrderBubble: FoodManager component not found on GameManager!");
        }
    }


    // New StartOrder method that accepts a number parameter.
    public void StartOrder(int numberOfOrders = 1)
    {
        // Determine how many orders can be spawned based on remaining slots.
        int availableSlots = maxSlots - orderedFoods.Count;
        int spawnCount = Mathf.Min(numberOfOrders, availableSlots);

        for (int i = 0; i < spawnCount; i++)
        {
            // Get a new Food instance from FoodManager.
            Food food = FM.GetRandomFood();
            // Make the food a child of OrderBubble.
            food.transform.SetParent(transform);

            // Determine slot index based on the current count.
            int slotIndex = orderedFoods.Count;
            Vector3 spawnPosition = CalculateOrderPosition(slotIndex);
            food.transform.position = spawnPosition;

            // Debug.Log("OrderedFood is " + food.foodName);
            // Store the new order in the list.
            orderedFoods.Add(food);
        }
    }

    protected override void UpdateLogStatus()
    {
        isDebugEnabled = logSettings.OrderBubbleLogs;
    }

    private Vector3 CalculateOrderPosition(int slotIndex)
    {
        Vector3 spawnPosition = transform.position;
        // Adjust the x position as before.
        spawnPosition.x -= selfSpriteRenderer.bounds.size.x * 1.5f / 10;

        // Get the bubble's height and compute the bottom Y coordinate.
        float bubbleHeight = selfSpriteRenderer.bounds.size.y;
        float bottomY = transform.position.y - bubbleHeight / 2;

        // Fixed percentages for 3 slots: 10%, 40%, and 70%.
        float[] percentages = new float[3] { 0.80f, 0.50f, 0.20f };
        spawnPosition.y = bottomY + percentages[slotIndex] * bubbleHeight;

        return spawnPosition;
    }
    // Optional: a method to retrieve the current list of orders.
    public List<Food> GetOrders()
    {
        return orderedFoods;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        Debug.Log("What is this? " + other.name);

        if (other.GetComponent<PlateDraggable>() != null)
        {

            // Debug.Log("Plate draggable entered order bubble: " + other.GetComponent<PlateDraggable>().GetCurrentIngredientsListString());
            ProcessFoodDelivery(other.GetComponent<PlateDraggable>());
        }

        if (other.GetComponent<CupDraggable>() != null)
        {
            // Debug.Log("Cup draggable entered order bubble: " + other.GetComponent<CupDraggable>().GetCurrentIngredientsListString());
            ProcessFoodDeliveryCup(other.GetComponent<CupDraggable>());
        }


        // if (other.TryGetComponent<FoodDraggable>(out var foodDraggable))
        // {
        //     Log($"Food draggable entered order bubble: {foodDraggable.foodName}");
        //     ProcessFoodDelivery(foodDraggable);
        // }
    }

    // private bool ValidateOrder(List<DraggableObjectSO> inputList, out Food matchedFood, out int index)
    // {
    //     Debug.Log("Validating  Order Called");
    //     for (index = 0; index < orderedFoods.Count; index++)
    //     {
    //     }
    //     matchedFood = null;
    //     return false;
    // }

    private bool ValidateOrder(
        List<DraggableObjectSO> inputList,
        out Food matchedFood,
        out int index)
    {
        Debug.Log("Validating Order Called");

        // Loop over every possible order in your queued list
        for (index = 0; index < orderedFoods.Count; index++)
        {
            var order = orderedFoods[index];
            var ingredients = order.ingredientsDraggableObjectSOArray;

            // Quick check: must have same number of ingredients
            if (inputList.Count != ingredients.Count)
                continue;

            // Make a temp copy we can remove matches from
            var temp = new List<DraggableObjectSO>(ingredients);
            bool allFound = true;

            // Try to find each submitted ingredient in the temp list
            foreach (var submitted in inputList)
            {
                if (temp.Contains(submitted))
                {
                    temp.Remove(submitted);
                }
                else
                {
                    allFound = false;
                    break;
                }
            }

            // If we removed every expected ingredient, it's a match
            if (allFound)
            {
                matchedFood = order;
                return true;
            }
        }

        // No match found
        matchedFood = null;
        index = -1;
        return false;
    }

    private void ProcessFoodDeliveryCup(CupDraggable delivered)
    {
        List<DraggableObjectSO> Ling = delivered.GetCurrentIngredientsList();
        bool isFinal = (orderedFoods.Count == 1);

        // 1) validation / extraction
        if (ValidateOrderCup(Ling, out var matchedFood, out var idx))
        {
            // 2) remove from list & reposition
            orderedFoods.RemoveAt(idx);
            // delivered.CancelDrag();
            // 3) animate & cleanup
            StartCoroutine(AnimateDeliveryAndCleanup(delivered.gameObject, matchedFood.gameObject, isFinal));
        }
        else
        {
            // wrong item
            Log($"OrderBubble: No matching order found for '{name}'");
            customerController.OnWrongDelivery(name);
        }
    }

    private bool ValidateOrderCup(
    List<DraggableObjectSO> inputList,
    out Food matchedFood,
    out int index)
    {
        Debug.Log("Validating Cup Order Called");

        for (index = 0; index < orderedFoods.Count; index++)
        {
            var order = orderedFoods[index];
            var ingredients = order.ingredientsDraggableObjectSOArray;

            // must have same length
            if (inputList.Count != ingredients.Count)
                continue;

            // element-by-element comparison
            bool allMatch = true;
            for (int i = 0; i < inputList.Count; i++)
            {
                if (inputList[i] != ingredients[i])
                {
                    allMatch = false;
                    break;
                }
            }

            if (allMatch)
            {
                matchedFood = order;
                return true;
            }
        }

        // no exact-order match found
        matchedFood = null;
        index = -1;
        return false;
    }


    private void ProcessFoodDelivery(PlateDraggable delivered)
    {
        List<DraggableObjectSO> Ling = delivered.GetCurrentIngredientsList();
        bool isFinal = (orderedFoods.Count == 1);

        // 1) validation / extraction
        if (ValidateOrder(Ling, out var matchedFood, out var idx))
        {
            // 2) remove from list & reposition
            orderedFoods.RemoveAt(idx);
            // delivered.CancelDrag();
            // 3) animate & cleanup
            StartCoroutine(AnimateDeliveryAndCleanup(delivered.gameObject, matchedFood.gameObject, isFinal));
        }
        else
        {
            // wrong item
            Log($"OrderBubble: No matching order found for '{name}'");
            customerController.OnWrongDelivery(name);
        }
        // Debug print all ordered foods

    }


    private IEnumerator AnimateDeliveryAndCleanup(GameObject deliveredObj, GameObject iconObj, bool isFinal)
    {
        Vector3 startPos = deliveredObj.transform.position;
        Vector3 endPos = iconObj.transform.position;
        float elapsed = 0f;

        // Optional: bring the delivered sprite to front
        var sr = deliveredObj.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = selfSpriteRenderer.sortingOrder + 1;

        while (elapsed < deliveryAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / deliveryAnimationDuration);
            deliveredObj.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        // ensure exact alignment
        deliveredObj.transform.position = endPos;

        // cleanup
        Destroy(iconObj);
        Destroy(deliveredObj);



        if (isFinal)
            customerController.OnAllOrdersFulfilled();
        else
            customerController.OnCorrectDelivery();
    }
}
