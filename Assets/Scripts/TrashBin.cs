using UnityEngine;

public class TrashBin : MonoBehaviour
{
    [SerializeField] private DraggableObject draggableInZone = null;

    public CustomerData cd;

    private void Start()
    {
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<DraggableObject>() != null) //might need to change to prevent condiments and drink source from being trashed
        {
            draggableInZone = other.GetComponent<DraggableObject>();
            Debug.Log($"[TrashBin] {other.name} entered trash zone.");
            cd.DeductScore(3); // Deduct score when an object enters the trash bin
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (draggableInZone != null && other.gameObject == draggableInZone.gameObject)
        {
            Debug.Log($"[TrashBin] {other.name} exited trash zone.");
            draggableInZone = null;
        }
    }

    private void Update()
    {
        // On mouse release, check if draggable is inside and was just dropped
        if (draggableInZone != null) //Condition: if a draggable in trashbin collider
        {
            //UI events
            EventManager.Instance.Trigger<TrashBin>("trashbinSelectedVisual", this);
            if (!draggableInZone.IsBeingDragged())
            {
                //dont allow ingredientdispenser to be trashed, return it to parent container

                if (draggableInZone.GetComponent<IngredientDispenserDraggable>() != null)
                {
                    draggableInZone.ReturnToParentContainer();
                }
                else
                {
                    Debug.Log($"[TrashBin] Destroying {draggableInZone.name}");
                    EventManager.Instance.Trigger<object>("ObjectTrashedAudio", this);
                    //!TODO, need to do additional checker to make sure dont destroy condiments and drinks ingredients
                    Destroy(draggableInZone.gameObject);
                    draggableInZone = null;
                }

            }
        }
        else
        {
            EventManager.Instance.Trigger<TrashBin>("trashbinDeselectedVisual", this);

        }
    }
}
