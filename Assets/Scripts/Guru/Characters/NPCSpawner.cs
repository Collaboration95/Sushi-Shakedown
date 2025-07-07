using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class NPCSpawner : MonoBehaviour
{
    public GameObject npcTemplate;
    public GameObject customerTemplate;
    // Use your scene object as a template. Make sure to disable it in the scene. 
    public float spawnInterval = 2.0f;

    public float spawnMargin = 1.0f;

    private float timer;

    public SpawnZoneData spawnZone;


    public bool spawnCustomerDebug = false;

    public OrderAreaGroup orderAreaGroup;
    void Start()
    {
        // Initialize the timer so that a spawn happens after the first interval.
        timer = spawnInterval;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnNPC();
            timer = spawnInterval;

        }
        if (spawnCustomerDebug)
        {
            SpawnCustomer();
            spawnCustomerDebug = false; // Reset the debug flag after spawning.
        }
    }
    void SpawnNPC()
    {
        bool fromLeft = Random.value > 0.5f;
        Camera cam = Camera.main;
        float height = 2f * cam.orthographicSize;
        float width = height * cam.aspect;

        float x = cam.transform.position.x + (fromLeft ? -1 : 1) * (width / 2 + spawnMargin);
        float y = Random.Range(spawnZone.MinY, spawnZone.MaxY);

        var npcObj = Instantiate(npcTemplate, new Vector3(x, y, 0), Quaternion.identity);
        npcObj.SetActive(true);

        var ctrl = npcObj.GetComponent<NPCController>();
        ctrl.Initialize(
            direction: fromLeft ? 1f : -1f,
            spawnY: y,
            minY: spawnZone.MinY,
            maxY: spawnZone.MaxY
        );
    }

    public bool SpawnCustomer()
    {
        // orderAreaGroup.PrintAllOrderArea(); // Print all order areas for debugging.
        OrderArea orderArea = orderAreaGroup.GetFreeOrderArea();
        // orderAreaGroup.PrintAllOrderArea(); // Print all order areas for debugging.
        // Debug.Log("Spawn Customer is being called.");
        if (orderArea == null)
        {
            Debug.Log("All order areas are occupied. Customer Not Spawned !!");
            return false;
        }
        bool spawnFromLeft = Random.value > 0.5f;
        Camera cam = Camera.main;
        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        float spawnX;
        if (spawnFromLeft)
        {
            spawnX = cam.transform.position.x - (camWidth / 2) - spawnMargin;
        }
        else
        {
            spawnX = cam.transform.position.x + (camWidth / 2) + spawnMargin;
        }

        // Use the spawn zone's y limits for vertical spawn.
        float spawnY = Random.Range(spawnZone.MinY, spawnZone.MaxY);
        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);
        GameObject customer = Instantiate(customerTemplate, spawnPosition, Quaternion.identity);
        customer.SetActive(true);
        if (customer.TryGetComponent<CustomerController>(out var customerController))
        {
            customerController.SetOrderArea(orderArea);
        }

        return true;
    }

}