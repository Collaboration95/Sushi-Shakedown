using System.Collections.Generic;
using UnityEngine;

public class Hypno : MonoBehaviour
{
    public List<CustomerController> customers;

    public void SpawnDestroyer()
    {
        // 1) Grab all CustomerController instances in the scene
        customers = new List<CustomerController>(FindObjectsOfType<CustomerController>());
        Debug.Log($"Found {customers.Count} customers");

        // 2) For each one, check if they're en route or already waiting
        foreach (var c in customers)
        {
            bool walkingToOrder = !c.hasArrived && !c.isWalkingOffScreen;
            bool waitingToOrder = c.hasArrived && c.OrderBubble.activeSelf;
            if (walkingToOrder || waitingToOrder)
                c.ForceTimeout();

        }
    }
}