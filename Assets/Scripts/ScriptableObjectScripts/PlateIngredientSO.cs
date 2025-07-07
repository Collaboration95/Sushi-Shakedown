using UnityEngine;

[CreateAssetMenu(fileName = "PlateIngredientSO", menuName = "Scriptable Objects/PlateIngredientSO")]
public class PlateIngredientSO : ScriptableObject
{
    public DraggableObjectSO draggableObjectSO;
    public string ingredientType;
}
