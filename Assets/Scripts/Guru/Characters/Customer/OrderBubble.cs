using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderBubble : DebuggableMonoBehaviour
{
    private const int MaxSlots = 3;
    public float slotSpacing;
    public float deliveryAnimationDuration = 0.2f;
    public CustomerController customerController;
    private SpriteRenderer selfSpriteRenderer;
    private readonly List<Food> orderedFoods = new List<Food>(MaxSlots);
    private readonly HashSet<GameObject> deliveries = new HashSet<GameObject>();
    private int pendingDeliveries;
    public FoodManager FM;

    protected override void Awake()
    {
        base.Awake();
        selfSpriteRenderer = GetComponent<SpriteRenderer>();
        if (customerController == null) customerController = GetComponentInParent<CustomerController>();
        ResolveFoodManager();
    }

    private void ResolveFoodManager()
    {
        if (FM == null) FM = FindFirstObjectByType<FoodManager>();
    }

    public void StartOrder(int numberOfOrders = 1)
    {
        ResolveFoodManager();
        if (FM == null || selfSpriteRenderer == null || customerController == null)
        {
            Debug.LogError("OrderBubble requires FoodManager, SpriteRenderer and CustomerController.", this);
            return;
        }
        int spawnCount = Mathf.Clamp(numberOfOrders, 0, MaxSlots - orderedFoods.Count - pendingDeliveries);
        for (int i = 0; i < spawnCount; i++)
        {
            Food food = FM.GetRandomFood();
            if (food == null) return; // FoodManager reports invalid configuration before spawning.
            food.transform.SetParent(transform);
            food.transform.position = CalculateOrderPosition(orderedFoods.Count);
            orderedFoods.Add(food);
        }
    }

    protected override void UpdateLogStatus() => isDebugEnabled = logSettings != null && logSettings.OrderBubbleLogs;

    private Vector3 CalculateOrderPosition(int slotIndex)
    {
        Vector3 position = transform.position;
        position.x -= selfSpriteRenderer.bounds.size.x * 0.15f;
        float height = selfSpriteRenderer.bounds.size.y;
        position.y = transform.position.y - height / 2 + (0.8f - slotIndex * 0.3f) * height;
        return position;
    }

    public List<Food> GetOrders() => orderedFoods;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null || customerController == null || customerController.isWalkingOffScreen ||
            deliveries.Contains(other.gameObject)) return;
        if (other.TryGetComponent<PlateDraggable>(out var plate))
            ProcessDelivery(plate, plate.GetCurrentIngredientsList(), false);
        else if (other.TryGetComponent<CupDraggable>(out var cup))
            ProcessDelivery(cup, cup.GetCurrentIngredientsList(), true);
    }

    private void ProcessDelivery(DraggableObject delivered, List<DraggableObjectSO> ingredients, bool ordered)
    {
        for (int i = 0; i < orderedFoods.Count; i++)
        {
            Food food = orderedFoods[i];
            // A plate cannot fulfill a drink order or vice versa.
            if (food == null || (food.GetComponent<CupDraggable>() != null) != ordered ||
                !OrderRules.Matches(ingredients, food.ingredientsDraggableObjectSOArray, ordered)) continue;
            orderedFoods.RemoveAt(i);
            deliveries.Add(delivered.gameObject);
            pendingDeliveries++;
            // The icon belongs to the bubble; the physical dish is consumed and cannot reenter.
            foreach (var collider in delivered.GetComponents<Collider2D>()) collider.enabled = false;
            delivered.enabled = false;
            if (GameManager.Instance != null && GameManager.Instance.currentlyDragging == delivered)
                GameManager.Instance.currentlyDragging = null;
            StartCoroutine(AnimateDeliveryAndCleanup(delivered.gameObject, food.gameObject));
            return;
        }
        customerController.OnWrongDelivery(delivered.name);
    }

    private IEnumerator AnimateDeliveryAndCleanup(GameObject delivered, GameObject icon)
    {
        Vector3 start = delivered.transform.position;
        Vector3 end = icon.transform.position;
        var sprite = delivered.GetComponent<SpriteRenderer>();
        if (sprite != null && selfSpriteRenderer != null) sprite.sortingOrder = selfSpriteRenderer.sortingOrder + 1;
        float elapsed = 0;
        while (elapsed < deliveryAnimationDuration && delivered != null && icon != null)
        {
            elapsed += Time.deltaTime;
            delivered.transform.position = Vector3.Lerp(start, end, Mathf.Clamp01(elapsed / deliveryAnimationDuration));
            yield return null;
        }
        deliveries.Remove(delivered);
        pendingDeliveries--;
        if (icon != null) Destroy(icon);
        if (delivered != null) Destroy(delivered);
        if (customerController == null || customerController.isWalkingOffScreen) yield break;
        // Only the last completed animation resolves the customer, independent of delivery order.
        if (orderedFoods.Count == 0 && pendingDeliveries == 0) customerController.OnAllOrdersFulfilled();
        else customerController.OnCorrectDelivery();
    }

    protected override void OnDisable()
    {
        StopAllCoroutines();
        foreach (var delivered in deliveries) if (delivered != null) Destroy(delivered);
        deliveries.Clear();
        pendingDeliveries = 0;
        foreach (var food in orderedFoods) if (food != null) Destroy(food.gameObject);
        orderedFoods.Clear();
        // Accepted icons remain children until their animation ends or the bubble is disabled.
        foreach (var food in GetComponentsInChildren<Food>(true)) Destroy(food.gameObject);
        base.OnDisable();
    }
}
