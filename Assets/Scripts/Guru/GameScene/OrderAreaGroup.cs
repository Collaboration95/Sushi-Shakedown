using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class OrderAreaGroup : MonoBehaviour
{
    public List<OrderArea> orderAreas;

    void Awake()
    {
        // Automatically get all OrderArea components from child objects.
        orderAreas = new List<OrderArea>(GetComponentsInChildren<OrderArea>());
    }

    // Sequential version that marks the area as occupied if needed.
    public OrderArea GetFreeOrderArea()
    {
        foreach (var area in orderAreas)
        {
            if (area != null && area.IsFree())
            {
                // Mark the area as occupied.
                area.UpdateState(true);
                return area;
            }

        }

        return null; // No free area available.
    }

    public void BootAllCustomers()
    {
        // Include customers still walking to their reserved slot, not just colliders at the counter.
        foreach (var customer in FindObjectsByType<CustomerController>(FindObjectsSortMode.None))
            if (customer.assignedOrderArea != null && orderAreas.Contains(customer.assignedOrderArea)) customer.ForceTimeout();
        foreach (var area in orderAreas) if (area != null) area.UpdateState(false);
    }

    public void ReleaseOrderArea(OrderArea area)
    {
        if (orderAreas.Contains(area))
        {
            area.UpdateState(false);
        }
    }

    public void PrintAllOrderArea()
    {

        RuntimeLog.Write(string.Join(" | ", orderAreas.Select(area => $"Order Area: {area.name}, Free: {(area.IsFree() ? 1 : 0)}")));

    }

    void Start()
    {
        RuntimeLog.Write(string.Join(" | ", orderAreas.Select(area => $"Order Area: {area.name}, Free: {(area.IsFree() ? 1 : 0)}")));

    }
    public bool AreAllOrderAreasFree()
    {
        // PrintAllOrderArea(); // Print all order areas for debugging.
        return orderAreas.All(area => area == null || area.IsFree());

    }
}
