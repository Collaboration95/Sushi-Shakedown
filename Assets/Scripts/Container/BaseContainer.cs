using UnityEngine;

public class BaseContainer : MonoBehaviour
{
    //base container defined as something that can store draggables as a child (not including plate/cups)
    //condiments and drinks ingredient container will inherit basecontainer directly
    //assembler will inherit base container, then add some checking functionality for foodBase (plate, cup) objects
    //cutting board will inherit base container, add the cutting functionality
    //stove will inherit base container, add cooking functionllity


    //commonality of all basecontainers
    //1.)track the draggableobject hovering in their colliders (draggableInZone)
    //2.)keep a record of what child draggables they own (ownedDraggable)
    [SerializeField] protected DraggableObject hoveringDraggableInCollider = null;
    [SerializeField] protected DraggableObject ownedDraggable= null;

    //TODO UI
    //[SerializeField] protected SpriteRenderer containerVisual;
    //protected Color defaultColor;
    private void Start()
    {
        ////TODO UI
        //if (containerVisual != null)
        //{

        //    defaultColor = containerVisual.color;
        //}
    }
    private void OnTriggerEnter2D(Collider2D other) //if collision detected
    {
        if (other.GetComponent<DraggableObject>() != null && other.GetComponent<DraggableObject>().IsBeingDragged()) //check if other object is a draggableobject and its being dragged
        {
            Debug.Log($"[{gameObject.name}] {other.name} entered {gameObject.name}.");
            //if other is draggableObject, track the hovering draggableObject by invoking SetHoveringDraggableObject 
            SetHoveringDraggableObjectTracking(other.GetComponent<DraggableObject>());
 
        }
    }

    private void OnTriggerExit2D(Collider2D other) //if collider exits
    {
        if (GetHoveringDraggableObjectTracking() != null && other.gameObject == GetHoveringDraggableObjectTracking().gameObject)
        {
            //if draggableInZone exist, reset to null to show no more draggable hovering in collider
            Debug.Log($"[{gameObject.name}] {other.name} left {gameObject.name}.");
            ClearHoveringDraggableObjectTracking();
        }
    }
    public virtual void TryGetDraggableToCursor(Vector3 mousePosition)
    {
        //first check if there is draggables within itself
        //if no draggables, ignore
        //if draggables, trigger draggable trytopickup
        if (GetOwnedDraggable())
        {
            GetOwnedDraggable().TryPickUpThis();
            SetHoveringDraggableObjectTracking(GetOwnedDraggable());
            ClearOwnedDraggable();
        }
        else
        {
            Debug.Log($"No Owned Draggables in {gameObject.name}");
        }
    }

    public DraggableObject GetHoveringDraggableObjectTracking()
    {
        return hoveringDraggableInCollider;
    }

    public void SetHoveringDraggableObjectTracking(DraggableObject draggableObject)
    {
        hoveringDraggableInCollider = draggableObject;
    }

    public void ClearHoveringDraggableObjectTracking()
    {
        hoveringDraggableInCollider = null;
    }

    public DraggableObject GetOwnedDraggable()
    {
        return ownedDraggable;
    }
    
    public void SetOwnedDraggable(DraggableObject draggableObject)
    {
        ownedDraggable = draggableObject; 
    }

    public void ClearOwnedDraggable()
    {
        ownedDraggable = null;
    }

    public void TriggerBaseContainerSelectedVisualEvent(BaseContainer baseContainer)
    {
        EventManager.Instance.Trigger<BaseContainer>("baseContainerSelectedVisual", baseContainer);
    }

    public void TriggerBaseContainerDeselectedSelectedVisualEvent(BaseContainer baseContainer)
    {
        EventManager.Instance.Trigger<BaseContainer>("baseContainerDeselectedVisual", baseContainer);
    }
}
