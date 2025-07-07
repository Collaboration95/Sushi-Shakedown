using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "CuttingRecipeSO", menuName = "Scriptable Objects/CuttingRecipeSO")]
public class CuttingRecipeSO : ScriptableObject
{
    public DraggableObjectSO inputIngredient;     // Original ingredient
    public int cuttingProgressMax;              // Cuts needed
    public DraggableObjectSO outputIngredient; // Resulting prefab
}