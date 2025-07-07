using UnityEngine;
using UnityEngine.EventSystems;

public class SettingsClickTester : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("[Settings]  Settings UI received click");
    }
}
