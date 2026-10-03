using System;
using UnityEngine;

public class FoodManager : MonoBehaviour
{
    [SerializeField] private DraggableObjectSO[] riceDraggableObjectSOArray;
    [SerializeField] private DraggableObjectSO[] fishDraggableObjectSOArray;
    [SerializeField] private DraggableObjectSO[] condimentDraggableObjectSOArray;
    [SerializeField] private DraggableObjectSO[] drinkDraggableObjectSOArray;
    [SerializeField] private GameObject platePrefab;
    [SerializeField] private GameObject cupPrefab;
    public GameObject foodPrefab;

    public Food GetRandomFood() => GetRandomFood(UnityEngine.Random.Range);

    // Small deterministic randomness seam for tests; Unity gameplay still uses Random.Range.
    public Food GetRandomFood(Func<int, int, int> choose)
    {
        if (!IsConfigured())
        {
            Debug.LogError("FoodManager requires ingredient arrays and plate/cup prefabs with Food, visuals and BoxCollider2D.", this);
            return null;
        }
        if (choose(0, 2) == 0)
        {
            var rice = riceDraggableObjectSOArray[choose(0, riceDraggableObjectSOArray.Length)];
            var fish = fishDraggableObjectSOArray[choose(0, fishDraggableObjectSOArray.Length)];
            var condiment = condimentDraggableObjectSOArray[choose(0, condimentDraggableObjectSOArray.Length)];
            var plate = Instantiate(platePrefab);
            var visual = plate.GetComponent<PlateDraggable>();
            visual.SetRiceSprite(rice.sprite);
            visual.SetFishSprite(fish.sprite);
            visual.SetCondimentSprite(condiment.sprite);
            plate.GetComponent<BoxCollider2D>().enabled = false;
            var food = plate.GetComponent<Food>();
            food.ingredientsDraggableObjectSOArray.Clear();
            food.ingredientsDraggableObjectSOArray.Add(rice);
            food.ingredientsDraggableObjectSOArray.Add(fish);
            food.ingredientsDraggableObjectSOArray.Add(condiment);
            return food;
        }
        var cup = Instantiate(cupPrefab);
        var drinkVisual = cup.GetComponent<CupDraggable>();
        cup.GetComponent<BoxCollider2D>().enabled = false;
        var drink = cup.GetComponent<Food>();
        drink.ingredientsDraggableObjectSOArray.Clear();
        int count = choose(1, 4);
        for (int i = 0; i < count; i++)
        {
            var ingredient = drinkDraggableObjectSOArray[choose(0, drinkDraggableObjectSOArray.Length)];
            drinkVisual.SetDrinkSprite(ingredient.sprite, i);
            drink.ingredientsDraggableObjectSOArray.Add(ingredient);
        }
        return drink;
    }

    private bool IsConfigured()
    {
        if (!ValidIngredients(riceDraggableObjectSOArray) || !ValidIngredients(fishDraggableObjectSOArray) ||
            !ValidIngredients(condimentDraggableObjectSOArray) || !ValidIngredients(drinkDraggableObjectSOArray) ||
            platePrefab == null || cupPrefab == null) return false;
        var plate = platePrefab.GetComponent<PlateDraggable>();
        var cup = cupPrefab.GetComponent<CupDraggable>();
        return plate != null && plate.riceSprite != null && plate.fishSprite != null && plate.condimentSprite != null &&
            cup != null && cup.bottomDrinkSprite != null && cup.middleDrinkSprite != null && cup.topDrinkSprite != null &&
            platePrefab.GetComponent<Food>() != null && cupPrefab.GetComponent<Food>() != null &&
            platePrefab.GetComponent<BoxCollider2D>() != null && cupPrefab.GetComponent<BoxCollider2D>() != null;
    }

    private static bool ValidIngredients(DraggableObjectSO[] ingredients)
    {
        if (ingredients == null || ingredients.Length == 0) return false;
        foreach (var ingredient in ingredients) if (ingredient == null) return false;
        return true;
    }
}
