using System.Collections.Generic;
using UnityEngine;

public class Guru_FoodSpawner : MonoBehaviour
{
    [Header("How many food items to keep alive at once")]
    public int maxFoodCount = 1;

    // --- internal state ---
    private FoodDraggable template;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    void Awake()
    {
        // 1) find the child FoodDraggable
        template = GetComponentInChildren<FoodDraggable>();
        if (template == null)
        {
            Debug.LogError("FoodSpawner: No FoodDraggable found as a child!");
            enabled = false;
            return;
        }

        // 2) record its transform as our spawn point
        spawnPosition = template.transform.position;
        spawnRotation = template.transform.rotation;

        // 3) hide the template so only clones show up
        template.gameObject.SetActive(false);
    }

    void Start()
    {
        // 4) spawn the initial batch
        for (int i = 0; i < maxFoodCount; i++)
            SpawnFood();
    }

    /// <summary>
    /// Instantiates a clone of the hidden template, activates it, and tracks it.
    /// </summary>
    public void SpawnFood()
    {
        // clone under this spawner
        GameObject go = Instantiate(
            template.gameObject,
            spawnPosition,
            spawnRotation,
            transform
        );
        go.SetActive(true);

        // wire up the spawner reference
        var fd = go.GetComponent<FoodDraggable>();
        fd.spawner = this;


    }

    /// <summary>
    /// Called by a FoodDraggable in its OnDestroy().
    /// Removes the reference and spawns a replacement.
    /// </summary>
    public void OnFoodDestroyed(FoodDraggable gone)
    {
        SpawnFood();
    }
}
