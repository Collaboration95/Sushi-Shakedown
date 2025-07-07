using UnityEngine;

public class IngredientDispenserContainer : BaseContainer
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //if (containerVisual != null)
        //{
        //    defaultColor = containerVisual.color;
        //}
       
        ClearHoveringDraggableObjectTracking();
    }

    // Update is called once per frame
    private void Update()
    {

        // On mouse release, check if draggable is inside and was just dropped
        if (GetHoveringDraggableObjectTracking() != null) //
        {
            DraggableObject trackingHoveringDraggableObject = GetHoveringDraggableObjectTracking();
            TriggerBaseContainerSelectedVisualEvent(this);
            //if (containerVisual != null)
            //{
            //    Color faded = containerVisual.color;
            //    faded.a = 0.5f; // semi-transparent
            //    containerVisual.color = faded;
            //}
            if (!trackingHoveringDraggableObject.IsBeingDragged())
            {
                Debug.Log($"{trackingHoveringDraggableObject.name} released in {gameObject.name}");

                if (GetOwnedDraggable() == null) //if container does not contain draggables
                {
                    if (trackingHoveringDraggableObject.GetComponent<DraggableObject>() != null) //TODO NEED TO UPDATE
                    {
                        Debug.Log($"{trackingHoveringDraggableObject.name} is a valid IngredientDraggable");

                        //set draggable object parent container to this
                        //set ownedDraggable to this draggableObject
                        //clear tracking of hovering object
                        trackingHoveringDraggableObject.SetParentContainer(this);
                        SetOwnedDraggable(trackingHoveringDraggableObject);
                        ClearHoveringDraggableObjectTracking();
                    }
                }
                else //container contains draggables
                {
                    trackingHoveringDraggableObject.ReturnToParentContainer();
                }
            }
        }
        else
        {
            TriggerBaseContainerDeselectedSelectedVisualEvent(this);
            //if (containerVisual != null)
            //{
            //    containerVisual.color = defaultColor;
            //}
        }
    }
}
