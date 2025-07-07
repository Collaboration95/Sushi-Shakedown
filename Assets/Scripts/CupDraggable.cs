using System.Collections.Generic;
using UnityEngine;

public class CupDraggable : DraggableObject
{
    [SerializeField] private CupIngredientSO[] cupIngredientSOArray;
    [SerializeField] private List<DraggableObjectSO> currentIngredients = new List<DraggableObjectSO>(); // actual ingredients

    [SerializeField] public SpriteRenderer bottomDrinkSprite;
    [SerializeField] public SpriteRenderer middleDrinkSprite;
    [SerializeField] public SpriteRenderer topDrinkSprite;

    [SerializeField] public List<SpriteRenderer> drinkSpriteArray;

    private void Awake()
    {
        Debug.Log("CupDraggable Start() called. OOOOOOOOOGA ");
        drinkSpriteArray.Add(bottomDrinkSprite);
        drinkSpriteArray.Add(middleDrinkSprite);
        drinkSpriteArray.Add(topDrinkSprite);
        Debug.Log($"DrinkSpriteArray count: {drinkSpriteArray.Count}");

    }

    public bool TryHandleIngredient(DraggableObject draggableObject)
    {
        DraggableObjectSO incomingSO = draggableObject.GetDraggableObjectSO();
        CupIngredientSO ingredientInfo = GetCupIngredientSO(incomingSO);
        if (ingredientInfo == null)
        {
            Debug.LogWarning("Ingredient not valid for this plate.");
            return false;
        }

        //check the amount of drink ingredient already present, add drink if length < 3


        if (currentIngredients.Count < 3)
        {
            currentIngredients.Add(incomingSO);
            EventManager.Instance.Trigger<object>("AddCupIngredientAudio", this);
            Debug.Log($"Added {ingredientInfo.ingredientType} ({incomingSO.name}) to cup.");
            UpdateDrinkSprites();
            return true;
        }

        Debug.Log("Cup already has maximum ingredients.");
        return false;
    }

    private void UpdateDrinkSprites()
    {
        for (int i = 0; i < drinkSpriteArray.Count; i++)
        {
            if (i < currentIngredients.Count)
            {
                drinkSpriteArray[i].sprite = currentIngredients[i].sprite;
                drinkSpriteArray[i].color = Color.white; // Reset in case it was hidden
            }
            else
            {
                drinkSpriteArray[i].sprite = null;
                drinkSpriteArray[i].color = new Color(1, 1, 1, 0); // Hide sprite
            }
        }
    }
    private CupIngredientSO GetCupIngredientSO(DraggableObjectSO inputSO)
    {
        foreach (var cupIngredientSO in cupIngredientSOArray)
        {
            if (cupIngredientSO.draggableObjectSO == inputSO)
                return cupIngredientSO;
        }
        return null;
    }

    public int GetIngredientCount()
    {
        return currentIngredients.Count;
    }

    public List<DraggableObjectSO> GetCurrentIngredients()
    {
        return new List<DraggableObjectSO>(currentIngredients);
    }
    public List<SpriteRenderer> GetDrinkSpriteArray()
    {
        return drinkSpriteArray;
    }
    public void SetDrinkSprite(Sprite drinkSprite, int index)
    {
        drinkSpriteArray[index].sprite = drinkSprite;
    }
    public List<DraggableObjectSO> GetCurrentIngredientsList()
    {
        return new List<DraggableObjectSO>(currentIngredients);
    }
}
