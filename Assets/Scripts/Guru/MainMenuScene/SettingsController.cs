using UnityEngine;
using UnityEngine.UI;         // For Button, Toggle, etc.

public class SettingsController : MonoBehaviour
{
    [Header("Screen Panels")]
    [Tooltip("Settings Panel GameObject")]
    public GameObject settingsPanel;
    [Tooltip("Main Menu Panel GameObject")]
    public GameObject mainMenuPanel;
    public MainMenuManager mainMenuManager;

    [Header("Game Mode Data")]
    public CustomerData customerData;  // Global data
    [SerializeField] private ToggleSwitch gameModeToggle;

    public GuruAudioManager gm;

    [SerializeField] private ToggleSwitch DifficultyModeToggle;
    // Local copy of the CustomerData that is used to modify settings locally.
    private CustomerData localCustomerData;

    private void Start()
    {
        gm = GameObject.Find("GuruAudioManager").GetComponent<GuruAudioManager>();

        CreateLocalCopy();
        CloseSettings();
    }

    private void OnEnable()
    {
        mainMenuManager.OnSettingsOpened += ShowSettings;
    }

    private void OnDisable()
    {
        mainMenuManager.OnSettingsOpened -= ShowSettings;
    }

    /// <summary>
    /// Creates a fresh local copy of the global CustomerData.
    /// </summary>
    private void CreateLocalCopy()
    {
        // Destroy any previous local copy
        if (localCustomerData != null)
        {
            Destroy(localCustomerData);
        }
        localCustomerData = Instantiate(customerData);
        // Debug.Log($"Local copy created: {localCustomerData.gameMode}");
    }

    /// <summary>
    /// Refreshes all UI elements based on the local copy.
    /// </summary>
    private void RefreshUI()
    {
        bool isWaves = localCustomerData.gameMode == GameMode.Waves;
        // Update the toggle state using a helper method from your ToggleSwitch
        gameModeToggle.SetStateSilently(isWaves);
        bool isNotEasy = localCustomerData.difficulty == Difficulty.Hard;
        DifficultyModeToggle.SetStateSilently(isNotEasy);
    }

    /// <summary>
    /// Called to show the settings panel.
    /// Creates a fresh local copy and refreshes the UI.
    /// </summary>
    public void ShowSettings()
    {
        gm.TransitionToDimmedSnapshot();
        CreateLocalCopy();
        RefreshUI();
        mainMenuPanel?.SetActive(false);
        settingsPanel?.SetActive(true);
    }

    /// <summary>
    /// Called by the UI "Apply" button.
    /// Commits all changes from the local copy back to the global CustomerData.
    /// </summary>
    public void Accept()
    {
        // Copy selected settings (here, just gameMode) from local to global.
        gm.PlayButtonClickSound();
        customerData.SetGameMode(localCustomerData.gameMode);
        customerData.SetDifficulty(localCustomerData.difficulty);

        CloseSettings();
        Destroy(localCustomerData);
        localCustomerData = null;

        gm.TransitionToGameplaySnapshot();
    }

    /// <summary>
    /// Called by the UI "Decline" button.
    /// Discards any changes from the local copy.
    /// </summary>
    public void Decline()
    {
        gm.PlayButtonClickSound();
        Debug.Log("Settings Declined. Changes discarded.");
        CloseSettings();
        Destroy(localCustomerData);
        localCustomerData = null;

        gm.TransitionToGameplaySnapshot();
    }

    public void CloseSettings()
    {
        settingsPanel?.SetActive(false);
        mainMenuPanel?.SetActive(true);
    }

    /// <summary>
    /// UI callback for toggling game mode.
    /// Updates the local copy instead of the global object.
    /// </summary>
    public void ToggleModeButton(int value)
    {
        gm.PlayButtonClickSound();
        localCustomerData.SetGameMode((GameMode)value);
    }

    public void ToggleDifficultyButton(int value)
    {
        gm.PlayButtonClickSound();
        localCustomerData.SetDifficulty((Difficulty)value);
    }

    /// <summary>
    /// Refreshes the UI. This can be called externally as needed.
    /// </summary>
    public void Refresh()
    {
        RefreshUI();
    }
}
