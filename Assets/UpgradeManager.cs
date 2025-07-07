using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [Header("ScriptableObject with your counts")]
    [SerializeField] private CustomerData customerData;

    [Header("Food-Assembly Area Objects (1→4)")]
    [SerializeField] private GameObject FA;
    [SerializeField] private GameObject FA2;
    [SerializeField] private GameObject FA3;
    [SerializeField] private GameObject FA4;

    [Header("Grill Area Objects (1→4)")]
    [SerializeField] private GameObject GA;
    [SerializeField] private GameObject GA2;
    [SerializeField] private GameObject GA3;
    [SerializeField] private GameObject GA4;

    // internal arrays for easy indexing
    private GameObject[] faAreas;
    private GameObject[] gaAreas;

    private void Awake()
    {
        // build tidy arrays in index order
        faAreas = new[] { FA, FA2, FA3, FA4 };
        gaAreas = new[] { GA, GA2, GA3, GA4 };
    }

    private void OnEnable()
    {
        customerData.OnFAA_Increased += HandleFAAIncreased;
        customerData.OnGrillArea_Increased += HandleGrillAreaIncreased;
    }

    private void OnDisable()
    {
        customerData.OnFAA_Increased -= HandleFAAIncreased;
        customerData.OnGrillArea_Increased -= HandleGrillAreaIncreased;
    }

    private void Start()
    {
        // set the initial visibility based on your SO's starting counts
        RefreshAllAreas();
        Debug.Log("GrillCount is " + customerData.GrillAreaCount);
        Debug.Log("FoodAssemblyCount is " + customerData.FoodAssemblyAreaCount);
    }

    /// <summary>
    /// Shows exactly N of each set, hides the rest.
    /// </summary>
    private void RefreshAllAreas()
    {
        int faCount = customerData.FoodAssemblyAreaCount;
        for (int i = 0; i < faAreas.Length; i++)
            faAreas[i].SetActive(i < faCount);

        int gaCount = customerData.GrillAreaCount;
        for (int i = 0; i < gaAreas.Length; i++)
            gaAreas[i].SetActive(i < gaCount);
    }

    /// <summary>
    /// Fired whenever you increment FoodAssemblyAreaCount in the SO.
    /// Just turn on the next FA object.
    /// </summary>
    private void HandleFAAIncreased(int newCount)
    {
        int idx = newCount - 1;         // counts are 1-based
        if (idx >= 0 && idx < faAreas.Length)
            faAreas[idx].SetActive(true);
    }

    /// <summary>
    /// Fired whenever you increment GrillAreaCount in the SO.
    /// Just turn on the next GA object.
    /// </summary>
    private void HandleGrillAreaIncreased(int newCount)
    {
        int idx = newCount - 1;
        if (idx >= 0 && idx < gaAreas.Length)
            gaAreas[idx].SetActive(true);
    }
}
