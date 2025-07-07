using UnityEngine;

[CreateAssetMenu(fileName = "GrillingRecipeSO", menuName = "Scriptable Objects/GrillingRecipeSO")]
public class GrillingRecipeSO : ScriptableObject
{
    public DraggableObjectSO inputIngredient;
    public float grillingProgressMax;
    public DraggableObjectSO outputIngredient;
}
