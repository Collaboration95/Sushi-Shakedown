using UnityEngine;
using UnityEngine.EventSystems;

public class GameManagerClickTester : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Ignore clicks that are currently over a UI element
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            Debug.Log("[GameManager]  World click detected");
        }
    }
}
