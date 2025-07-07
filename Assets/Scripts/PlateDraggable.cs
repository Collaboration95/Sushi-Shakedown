using System.Collections.Generic;
using UnityEngine;

public class PlateDraggable : DraggableObject
{
    //A SO List of valid items to put onto it
    [SerializeField] private PlateIngredientSO[] plateIngredientSOArray;
    [SerializeField] private List<string> currentIngredientTypes = new List<string>(); // tracks types like "rice", "fish"
    [SerializeField] private List<DraggableObjectSO> currentIngredients = new List<DraggableObjectSO>(); // actual ingredients

    [SerializeField] public SpriteRenderer riceSprite;
    [SerializeField] public SpriteRenderer fishSprite;
    [SerializeField] public SpriteRenderer condimentSprite;

    //An array to track items currently on it (bluff, no actual gameobject, use sprite to simulate)


    //rice, fish, condiment (only one of each type, if already available, cannot handle this)
    //current max recipe types [rice, fish, condiments]
    public bool TryHandleIngredient(DraggableObject draggableObject)
    {
        Debug.Log($"Try handle object{draggableObject}");
        //check if its valid item in plateIngredientSOArray
        Debug.Log($"[PlateDraggable] Try handle object {draggableObject.name}");

        DraggableObjectSO incomingSO = draggableObject.GetDraggableObjectSO();
        PlateIngredientSO ingredientInfo = GetPlateIngredientSO(incomingSO);
        if (ingredientInfo == null)
        {
            Debug.LogWarning("Ingredient not valid for this plate.");
            return false;
        }
        // Check if the type is already on the plate
        string ingredientType = ingredientInfo.ingredientType;
        if (currentIngredientTypes.Contains(ingredientType))
        {
            Debug.LogWarning($"Plate already contains a {ingredientType}.");
            return false;
        }


        // Add to plate
        if (ingredientType == "rice" || ingredientType == "fish" || ingredientType == "condiment")
        {
            currentIngredientTypes.Add(ingredientType);
            currentIngredients.Add(incomingSO);

            EventManager.Instance.Trigger<object>("AddPlateIngredientAudio", this);
            Debug.Log($"Added {ingredientType} ({incomingSO.name}) to plate.");
        }

        // You can trigger a UI update or visual stacking here
        if (ingredientType == "rice")
        {
            riceSprite.sprite = incomingSO.sprite;
        }
        else if (ingredientType == "fish")
        {
            fishSprite.sprite = incomingSO.sprite;
        }
        else if (ingredientType == "condiment")
        {
            condimentSprite.sprite = incomingSO.sprite;
        }

        return true;
    }
    private PlateIngredientSO GetPlateIngredientSO(DraggableObjectSO inputSO)
    {
        foreach (var plateIngredientSO in plateIngredientSOArray)
        {
            if (plateIngredientSO.draggableObjectSO == inputSO)
                return plateIngredientSO;
        }
        return null;
    }
    public List<DraggableObjectSO> GetCurrentIngredients()
    {
        return new List<DraggableObjectSO>(currentIngredients);
    }
    public void ClearPlate()
    {
        currentIngredientTypes.Clear();
        currentIngredients.Clear();
        Debug.Log("Plate cleared.");
    }

    public int GetIngredientCount()
    {
        return currentIngredients.Count;
    }

    public List<DraggableObjectSO> GetCurrentIngredientsList()
    {
        return new List<DraggableObjectSO>(currentIngredients);
    }
    public string GetCurrentIngredientsListString()
    {
        if (currentIngredients.Count == 0)
        {
            return "Empty plate";
        }

        List<string> ingredientNames = new List<string>();
        foreach (var ingredient in currentIngredients)
        {
            ingredientNames.Add(ingredient.name);
        }

        return string.Join(", ", ingredientNames);
    }
    public void SetRiceSprite(Sprite sprite)
    {
        riceSprite.sprite = sprite;
    }
    public void SetFishSprite(Sprite sprite)
    {
        fishSprite.sprite = sprite;
    }
    public void SetCondimentSprite(Sprite sprite)
    {
        condimentSprite.sprite = sprite;
    }
}
