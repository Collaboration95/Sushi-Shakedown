using UnityEngine;

[CreateAssetMenu(fileName = "CupIngredientSO", menuName = "Scriptable Objects/CupIngredientSO")]
public class CupIngredientSO : ScriptableObject
{
    public DraggableObjectSO draggableObjectSO;
    public string ingredientType;
}
