using UnityEngine;

public class AssemblerContainer : BaseContainer
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
    private void Update()
    {

        // On mouse release, check if draggable is inside and was just dropped
        if (GetHoveringDraggableObjectTracking() != null) //
        {
            DraggableObject trackingHoveringDraggableObject = GetHoveringDraggableObjectTracking();
            //TODO add event for ui
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
                    if (trackingHoveringDraggableObject.GetComponent<IngredientDraggable>() != null || trackingHoveringDraggableObject.GetComponent<PlateDraggable>() != null || trackingHoveringDraggableObject.GetComponent<CupDraggable>() != null) 
                    {
                        // It is an IngredientDraggable or a ServingDraggable
                        Debug.Log($"{trackingHoveringDraggableObject.name} is a valid DraggableObject");
                        
                       
                        //Setting up object being placed on assemblycontainer
                        //logic
                        trackingHoveringDraggableObject.SetParentContainer(this); 
                        SetOwnedDraggable(trackingHoveringDraggableObject);
                        ClearHoveringDraggableObjectTracking();
                        //audio
                        EventManager.Instance.Trigger<object>("PlaceItemAudio", this);
                    }
                    else if (trackingHoveringDraggableObject.GetComponent<IngredientDispenserDraggable>() != null)
                    {
                        //if its a ingredient dispenser, return it back to parent container
                        trackingHoveringDraggableObject.ReturnToParentContainer();
                    }
                } 
                else //container contains draggables
                {
                    //check if its a plate or cup
                    if(GetOwnedDraggable().GetComponent<PlateDraggable>() != null)
                    {
                        //if plate, trigger plate handle function
                        PlateDraggable plate = GetOwnedDraggable().GetComponent<PlateDraggable>();  
                        if (plate.TryHandleIngredient(trackingHoveringDraggableObject)) //if plate can handle ingredient
                        {
                            //if its condiment (roe, soysauce, wasabi), return to parent container
                            if(trackingHoveringDraggableObject.GetDraggableObjectSO().objectName == "roe" || trackingHoveringDraggableObject.GetDraggableObjectSO().objectName == "soysauce" || trackingHoveringDraggableObject.GetDraggableObjectSO().objectName == "wasabi")
                            {
                                trackingHoveringDraggableObject.ReturnToParentContainer();
                                ClearHoveringDraggableObjectTracking();
                            }
                            //else, destroy object
                            else
                            {
                                Destroy(trackingHoveringDraggableObject.gameObject);
                                ClearHoveringDraggableObjectTracking();
                            }
                        } 
                        else
                        {
                            //return it to parent
                            trackingHoveringDraggableObject.ReturnToParentContainer();
                            ClearHoveringDraggableObjectTracking();
                        }

                    } 
                    else if(GetOwnedDraggable().GetComponent<CupDraggable>() != null)
                    {
                        CupDraggable cup = GetOwnedDraggable().GetComponent<CupDraggable>();
                        if (cup.TryHandleIngredient(trackingHoveringDraggableObject))
                        {
                            // then return it (or destroy it) as you already do
                            trackingHoveringDraggableObject.ReturnToParentContainer();
                            // immediately clear hover so we don't re-enter this block next frame
                            ClearHoveringDraggableObjectTracking();
                        }
                        else
                        {
                            // now covers invalid cup drops too
                            trackingHoveringDraggableObject.ReturnToParentContainer();
                            ClearHoveringDraggableObjectTracking();
                        }
                    }
                    else
                    {
                        //if it is, trigger function in plate or cup that verifies if possble to add
                        trackingHoveringDraggableObject.ReturnToParentContainer();
                        ClearHoveringDraggableObjectTracking();

                    }

                }
                //!TODO check if assembler owns any draggables
                //if empty, check if draggable is valid for assembler

                //if not empty, check if its a plate or cup
                //if not, means area is occupied, return the currentlydragging to parent container

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
