using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    [SerializeField] protected bool isDragging = false;
    [SerializeField] protected Vector3 offset;
    [SerializeField] protected bool isColliding = false;

    [SerializeField] protected DraggableObjectSO draggableObjectSO;

    [SerializeField] protected BaseContainer parentContainer;
    [SerializeField] protected GameDataSO gameDataSO;

    private void Start()
    {

    }

    protected void OnTriggerEnter2D(Collider2D other)
    {
        //check if other GameObject is BaseContainer or TrashBin
        BaseContainer container = other.GetComponent<BaseContainer>();
        TrashBin trashBin = other.GetComponent<TrashBin>();
        if (container != null)
        {
            //set isColliding to true
            Debug.Log($"[DraggableObject] {gameObject.name} entered {container.name}.");

            isColliding = true;
        }
        else if (trashBin != null)
        {
            Debug.Log($"[DraggableObject] {gameObject.name} entered {trashBin.name}.");
            isColliding = true;
            Debug.Log(isColliding);
        }
    }

    protected void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log($"[DraggableObject] {gameObject.name} exited collision zone .");

        isColliding = false;
    }

    protected void Update()
    {
        if (isDragging)
        {
            //continuous position updating
            UpdatePosition();

        }
    }

    protected void UpdatePosition() //draggableobject should update their own positions
    {
        Vector3 newPosition = gameDataSO.mousePosition + offset;
        transform.position = newPosition; // Follow the mouse smoothly
    }

    public void TryPickUpThis() //draggableobject should handle pickup by themselves, called to do so by other objects (basecontainers)
    {
        if (GameManager.Instance.currentlyDragging != null) //!!SHOULD NOT HAPPEN
        {
            Debug.LogWarning($"[{gameObject.name}] Game Manager invoked TryPickUpThis but Game Manager already dragging something");
        }
        else
        {

            Debug.Log($"[DraggableObject] {gameObject.name} picked up.");
            isDragging = true;

            offset = transform.position - gameDataSO.mousePosition;
            GameManager.Instance.currentlyDragging = this;
            transform.SetParent(null);
        }
    }

    public void ReturnToParentContainer() //draggableobjects should handle returning to parent container themselves
    {
        if (parentContainer != null)
        {
            transform.SetParent(parentContainer.transform); // Safe to call even if already the parent, just to be sure
            transform.localPosition = Vector3.zero;
            parentContainer.SetOwnedDraggable(this);
            Debug.Log($"[DraggableObject] {gameObject.name} returned to {parentContainer.name} and centered.");
            isDragging = false;
        }
        else
        {
            Debug.LogWarning($"[DraggableObject] {gameObject.name} has no parentContainer assigned!");
            Destroy(gameObject);
        }
    }


    public BaseContainer GetParentContainer() //getter for parent container
    {
        return parentContainer;
    }

    public void SetParentContainer(BaseContainer container) //setter for parent container
    {   
        
        parentContainer = container;
        transform.SetParent(container.transform);
        transform.localPosition = Vector3.zero;
    }

    public bool IsBeingDragged() //getter to check bool status of isDragging
    {
        return isDragging;
    }

    public void HandleRelease() //method called by game manager when left click is released and game manager currently dragging this object
    {
        Debug.Log($"[DraggableObject] {gameObject.name} released. isColliding = {isColliding}");
        //set isDragging to false
        isDragging=false;
        GameManager.Instance.currentlyDragging=null;

        if (!isColliding) //if no valid collision

        {
            Debug.Log($"[DraggableObject] {gameObject.name} is not colliding with any valid container.");

            if (parentContainer) //check if parent container exist, if does return this to parent
            {
                Debug.Log($"[DraggableObject] Returning {gameObject.name} to its parent container: {parentContainer.name}");
                ReturnToParentContainer();
            }
            else //else, remove this
            {
                Debug.LogWarning($"[DraggableObject] {gameObject.name} has no parent container and was dropped in an invalid area. Destroying...");
                Destroy(gameObject);
            }
        }
        else //if valid collision, reset values of this, then let the collided gameObject handle the flow
        {
            Debug.Log($"[DraggableObject] {gameObject.name} was released while colliding with a valid container. set isDragging to False.");
        }
    }



    public DraggableObjectSO GetDraggableObjectSO()
    {
        return draggableObjectSO;
    }

}
