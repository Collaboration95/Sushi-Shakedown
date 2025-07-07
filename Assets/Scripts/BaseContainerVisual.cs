using System;
using UnityEngine;

public class BaseContainerVisual : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer containerVisual;
    protected Color defaultColor;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (containerVisual != null)
        {
            defaultColor = containerVisual.color;
        }

        // Subscribe to the trashbinSelected event
        EventManager.Instance.Subscribe<BaseContainer>("baseContainerSelectedVisual", OnBaseContainerSelectedVisual);
        EventManager.Instance.Subscribe<BaseContainer>("baseContainerDeselectedVisual", OnBaseContainerDeselectedVisual);
    }

    private void OnBaseContainerDeselectedVisual(BaseContainer baseContainer)
    {
        if (baseContainer.gameObject == transform.parent.gameObject)
        {
            containerVisual.color = defaultColor;

        }
    }

    private void OnBaseContainerSelectedVisual(BaseContainer baseContainer)
    {
        if (baseContainer.gameObject == transform.parent.gameObject)
        {
            if (containerVisual != null)
            {
                Color faded = containerVisual.color;
                faded.a = 0.5f; // semi-transparent
                containerVisual.color = faded;
            }
        }
    }

    private void OnDestroy()
    {
        // Always unsubscribe on destroy to prevent memory leaks
        EventManager.Instance.Unsubscribe<BaseContainer>("baseContainerSelectedVisual", OnBaseContainerSelectedVisual);
        EventManager.Instance.Unsubscribe<BaseContainer>("baseContainerDeselectedVisual", OnBaseContainerDeselectedVisual);
    }

}
