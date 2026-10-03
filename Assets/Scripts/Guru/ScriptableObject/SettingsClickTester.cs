using UnityEngine;
using UnityEngine.EventSystems;

public class SettingsClickTester : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        RuntimeLog.Write("[Settings]  Settings UI received click");
    }
}
