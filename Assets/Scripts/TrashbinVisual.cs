using System;
using UnityEngine;

public class TrashbinVisual : MonoBehaviour
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
        EventManager.Instance.Subscribe<TrashBin>("trashbinSelectedVisual", OnTrashBinSelectedVisual);
        EventManager.Instance.Subscribe<TrashBin>("trashbinDeselectedVisual", OnTrashBinDeselectedVisual);
    }



    private void OnDestroy()
    {
        // Always unsubscribe on destroy to prevent memory leaks
        EventManager.Instance.Unsubscribe<TrashBin>("trashbinSelectedVisual", OnTrashBinSelectedVisual);
        EventManager.Instance.Unsubscribe<TrashBin>("trashbinDeselectedVisual", OnTrashBinDeselectedVisual);
    }


    private void OnTrashBinSelectedVisual(TrashBin trashBin)
    {
        // Check if the event is for *this* trashbin
        if (trashBin.gameObject == transform.parent.gameObject)
        {
            if (containerVisual != null)
            {
                Color faded = containerVisual.color;
                faded.a = 0.5f; // semi-transparent
                containerVisual.color = faded;
            }
        }
    }

    private void OnTrashBinDeselectedVisual(TrashBin trashBin)
    {
        if (trashBin.gameObject == transform.parent.gameObject)
        {
            containerVisual.color = defaultColor;

        }
    }
}
