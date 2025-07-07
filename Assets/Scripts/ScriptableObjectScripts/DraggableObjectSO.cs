using UnityEngine;

[CreateAssetMenu(fileName = "DraggableObjectSO", menuName = "Kitchen/Draggable Object")]
public class DraggableObjectSO : ScriptableObject
{
    public Transform prefab;
    public Sprite sprite;
    public string objectName;
}
