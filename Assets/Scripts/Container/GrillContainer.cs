using System;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.UI;

public class GrillContainer : BaseContainer
{
    [SerializeField] private GrillingRecipeSO[] grillingRecipeSOArray;

    [Header("Progress UI")]
    [SerializeField] private GameObject grillingProgressUI;  // assign your ProgressBarUI root here

    private float grillingProgress = 0f;
    private bool isGrilling = false;
    private GrillingRecipeSO activeRecipe;
    public CustomerData cs; // assign via the Inspector

    private int grillingMultiplier = 1; // default multiplier
    private void OnEnable()
    {
        cs.OnGrillSpeed_Increased += HandleGrillSpeedIncreased;
    }
    private void OnDisable()
    {
        cs.OnGrillSpeed_Increased -= HandleGrillSpeedIncreased;
    }

    private void Start()
    {
        // set the initial visibility based on your SO's starting counts
        grillingMultiplier = cs.GrillSpeedCount; // default multiplier
    }

    private void HandleGrillSpeedIncreased(int newValue)
    {
        grillingMultiplier = newValue;
    }

    private void Update()
    {
        HandleHoverAndDrop();
        //NEW: if we have an owned draggable, but aren�t cooking, kick off a new cycle
        TryResumeCooking();

        HandleGrillingProgress();
    }
    private void TryResumeCooking()
    {
        // only when:
        // 1) something is back in the grill
        // 2) we�re not already grilling
        // 3) we don�t have an activeRecipe yet
        var held = GetOwnedDraggable();
        if (held != null && !isGrilling && activeRecipe == null && HasRecipeWithInput(held.GetDraggableObjectSO()))
        {
            // find a recipe for it
            var recipe = GetRecipeWithInput(held.GetDraggableObjectSO());
            if (recipe != null)
            {
                activeRecipe = recipe;
                grillingProgress = 0f;
                isGrilling = true;
                EventManager.Instance.Trigger<object>("GrillStartAudioLoop", this);


                EventManager.Instance.Trigger("showProgressUI", grillingProgressUI);
                EventManager.Instance.Trigger("updateProgressUI",
                    new ProgressBarUpdateData(grillingProgressUI, 0f));
            }
        }
    }
    private void HandleHoverAndDrop()
    {
        var hovering = GetHoveringDraggableObjectTracking();
        if (hovering != null)
        {
            TriggerBaseContainerSelectedVisualEvent(this);

            if (!hovering.IsBeingDragged())
            {
                // only accept if empty and we have a recipe for this input
                if (GetOwnedDraggable() == null && HasRecipeWithInput(hovering.GetDraggableObjectSO()))
                {
                    // grab it
                    hovering.SetParentContainer(this);
                    SetOwnedDraggable(hovering);
                    ClearHoveringDraggableObjectTracking();
                    //audio
                    EventManager.Instance.Trigger<object>("PlaceItemAudio", this);

                    // start grilling
                    activeRecipe = GetRecipeWithInput(hovering.GetDraggableObjectSO());
                    grillingProgress = 0f;
                    isGrilling = true;
                    EventManager.Instance.Trigger<object>("GrillStartAudioLoop", this);

                    // show & zero the bar
                    EventManager.Instance.Trigger("showProgressUI", grillingProgressUI);
                    EventManager.Instance.Trigger("updateProgressUI",
                        new ProgressBarUpdateData(grillingProgressUI, 0f));
                }
                else
                {
                    // invalid: return it
                    hovering.ReturnToParentContainer();
                }
            }
        }
        else
        {
            TriggerBaseContainerDeselectedSelectedVisualEvent(this);
        }
    }

    private void HandleGrillingProgress()
    {
        if (!isGrilling || GetOwnedDraggable() == null) return;

        // advance
        float adjustedMultiplier = 1f + (grillingMultiplier - 1) * 1.0f;
        grillingProgress += Time.deltaTime * adjustedMultiplier;


        float normalized = grillingProgress / activeRecipe.grillingProgressMax;

        // update bar every frame
        EventManager.Instance.Trigger("updateProgressUI", new ProgressBarUpdateData(grillingProgressUI, normalized));

        // finished?
        if (grillingProgress >= activeRecipe.grillingProgressMax)
            CompleteGrillingCycle();
    }

    private void CompleteGrillingCycle()
    {
        // destroy raw
        Destroy(GetOwnedDraggable().gameObject);
        ClearOwnedDraggable();

        // spawn grilled
        var go = Instantiate(activeRecipe.outputIngredient.prefab);
        var grilled = go.GetComponent<DraggableObject>();
        SetOwnedDraggable(grilled);
        grilled.SetParentContainer(this);

        // chain to next recipe? (e.g. grilled -> burnt)
        var next = GetRecipeWithInput(activeRecipe.outputIngredient);
        if (next != null)
        {
            activeRecipe = next;
            grillingProgress = 0f;
            isGrilling = true;

            // reset bar to zero (we�re still showing)
            EventManager.Instance.Trigger("updateProgressUI",
                new ProgressBarUpdateData(grillingProgressUI, 0f));
        }
        else
        {
            // done cooking
            isGrilling = false;
            activeRecipe = null;
            EventManager.Instance.Trigger<object>("GrillStopAudioLoop", this);

            // hide the bar
            EventManager.Instance.Trigger("hideProgressUI", grillingProgressUI);
        }
    }

    public override void TryGetDraggableToCursor(Vector3 mousePosition)
    {
        if (GetOwnedDraggable())
        {
            // pick it back up
            GetOwnedDraggable().TryPickUpThis();
            SetHoveringDraggableObjectTracking(GetOwnedDraggable());
            ClearOwnedDraggable();

            // cancel grilling
            grillingProgress = 0f;
            isGrilling = false;
            activeRecipe = null;
            EventManager.Instance.Trigger<object>("GrillStopAudioLoop", this);

            // hide the bar
            EventManager.Instance.Trigger("hideProgressUI", grillingProgressUI);
        }
        else
        {
            Debug.Log($"No Owned Draggables in {gameObject.name}");
        }
    }

    private GrillingRecipeSO GetRecipeWithInput(DraggableObjectSO input)
    {
        foreach (var recipe in grillingRecipeSOArray)
        {
            if (recipe.inputIngredient == input)
                return recipe;
        }
        return null;
    }

    private bool HasRecipeWithInput(DraggableObjectSO input)
    {
        return GetRecipeWithInput(input) != null;
    }

}
