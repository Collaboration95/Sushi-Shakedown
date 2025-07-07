using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting; // for loading scenes

public class OverLayManager : DebuggableMonoBehaviour, IPointerClickHandler
{
    public CustomerData customerData;
    private CustomerData Pause_CustomerData_Local;
    public static event Action<bool> OnUIBlockToggle;

    private void BlockUI(bool isBlocked)
    {
        OnUIBlockToggle?.Invoke(isBlocked);
    }


    // Screens
    public GameObject Game_Screen;
    public GameObject Pause_Screen;
    public GameObject Info_Screen;
    public GameObject PreDay_Screen;

    public GameObject UpgradeScreen;
    public GameObject Failure_Screen;
    public CustomerAudioManager cm;

    public GameObject Final_Day_Screen;

    // UI Toggles (Settings)
    [SerializeField] private ToggleSwitch GameMode_Toggle;
    [SerializeField] private ToggleSwitch DifficultyMode_Toggle;

    // InfoUI elements
    public TextMeshProUGUI Info_DayText;
    public TextMeshProUGUI Info_ScoreText;

    public TextMeshProUGUI Info_StashText;
    public TextMeshProUGUI Info_RansomText;
    public TextMeshProUGUI Info_NewTotalStashText;
    public TextMeshProUGUI Info_TotalServedText;
    public TextMeshProUGUI Info_HappyText;


    public event System.Action OnInfoClosed;



    // PreDayUI elements
    public TextMeshProUGUI PreDay_DayText;
    public TextMeshProUGUI PreDay_ScoreText;

    public TextMeshProUGUI PreDay_HiddenStashText;
    public event System.Action OnPreDayClosed;


    public TextMeshProUGUI Failure_Title;
    public TextMeshProUGUI Failure_Description;

    public TextMeshProUGUI Upgrade_CointCount;

    public TextMeshProUGUI Upgrade_FoodAssemblyCount;


    public TextMeshProUGUI Upgrade_RequiredCoinsCount;

    [SerializeField] private TextMeshProUGUI Upgrade_GrillCount;
    [SerializeField] private TextMeshProUGUI Upgrade_GrillCost;

    private int[] FAA_Cost = { 20, 30 };
    private int[] GrillAreaCost = { 20, 30 };
    private int[] GrillSpeedCost = { 20, 30 };
    private int[] PatienceLevelCost = { 20, 30 };

    private int[] CuttingSpeedCost = { 20, 30 };
    public int FoodAssemblyAreaMaxCount = 4;
    public int GrillAreaMaxCount = 4;
    public int GrillMaxSpeedCount = 3;

    public int FoodAssemblyLevelMaxCount = 3;


    public int CuttingSpeedMaxLevel = 3;
    public int PatienceLevelMaxCount = 3;

    [SerializeField] private TextMeshProUGUI Upgrade_GrillSpeed;
    [SerializeField] private TextMeshProUGUI Upgrade_GrillSpeedCost;


    [SerializeField] private TextMeshProUGUI Upgrade_CustomerPatience;
    [SerializeField] private TextMeshProUGUI Upgrade_CustomerPatienceCost;

    [SerializeField] private TextMeshProUGUI Upgrade_CuttingSpeed;
    [SerializeField] private TextMeshProUGUI Upgrade_CuttingSpeedCost;
    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        cm = GameObject.Find("CustomerAudioManager").GetComponent<CustomerAudioManager>();
        // Initialize the UI to show the game screen by default
        DefaultView();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        // customerData.OnGameModeChanged += HandleModeChanged;
    }

    protected override void UpdateLogStatus()
    {
        isDebugEnabled = logSettings.OverLayManagerLogs;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        // customerData.OnGameModeChanged -= HandleModeChanged;
    }

    public void PauseButtonClick()
    {

        CreateLocalCopy();
        RefreshUI();
        HideAllScreens();

        cm.PlayButtonClickSound();
        Pause_Screen.SetActive(true);
        Time.timeScale = 0f; // Pause the game

        cm.TransitionToDimmedSnapshot();  // ADD HERE
        BlockUI(true); // Block UI interactions

    }
    private void CreateLocalCopy()
    {
        // Destroy any previous local copy
        if (Pause_CustomerData_Local != null)
        {
            Destroy(Pause_CustomerData_Local);
        }
        Pause_CustomerData_Local = Instantiate(customerData);
        Log($"Local copy created: {Pause_CustomerData_Local.gameMode}");

    }
    private void RefreshUI()
    {

        bool isWaves = Pause_CustomerData_Local.gameMode == GameMode.Waves;

        GameMode_Toggle.CurrentValue = isWaves;

        bool isEasy = Pause_CustomerData_Local.difficulty == Difficulty.Easy;
        DifficultyMode_Toggle.CurrentValue = isEasy;
    }

    public void Accept()
    {
        // Copy selected
        cm.PlayButtonClickSound();
        customerData.SetGameMode(Pause_CustomerData_Local.gameMode);
        customerData.SetDifficulty(Pause_CustomerData_Local.difficulty);
        DefaultView();
        // CloseSettings();
        Destroy(Pause_CustomerData_Local);
        Pause_CustomerData_Local = null;
        Time.timeScale = 1f; // Resume the game speed
        cm.TransitionToGameplaySnapshot();

    }

    /// <summary>
    /// Called by the UI "Decline" button.
    /// Discards any changes from the local copy.
    /// </summary>
    public void Decline()
    {
        cm.PlayButtonClickSound();
        Log("Settings Declined. Changes discarded.");
        DefaultView();
        // Resume game speed when returning to game
        Destroy(Pause_CustomerData_Local);
        Pause_CustomerData_Local = null;

        Time.timeScale = 1f; // Resume the game speed
        cm.TransitionToGameplaySnapshot();

    }

    public void ToggleModeButton(int value)
    {
        cm.PlayButtonClickSound();
        Pause_CustomerData_Local.SetGameMode((GameMode)value);
        RefreshUI();
    }

    public void ToggleDifficultyButton(int value)
    {
        cm.PlayButtonClickSound();
        Log("Tn called with value: " + (Difficulty)value);
        Pause_CustomerData_Local.SetDifficulty((Difficulty)value);
    }

    private void DefaultView()

    {
        HideAllScreens();
        Game_Screen.SetActive(true);
        BlockUI(false); // Unblock UI interactions
    }
    public void ShowInfoUI(int day, int score, int totalServed, int happy, int angry, int TotalCoins, int Ransom)
    {
        BlockUI(true); // Block UI interactions
        cm.TransitionToDimmedSnapshot();
        HideAllScreens();
        Info_Screen.SetActive(true);
        Debug.Log($"[Overlay] ShowInfoUI: day={day}, score={score}, served={totalServed}, happy={happy}, angry={angry} , TotalCoins={TotalCoins}");
        Info_DayText.text = $"Day: {day} Completed!";
        Info_ScoreText.text = $"{TotalCoins + Ransom}";
        Info_RansomText.text = $"{Ransom}";
        Info_NewTotalStashText.text = $"{TotalCoins}";

        Info_TotalServedText.text = $"Customers Served: {totalServed}";
        Info_HappyText.text = $"Happy Customers: {happy}";
        // Info_CustomerCoinsText.text = $"Total Coins: {TotalCoins}";
    }

    public void CloseInfoUI()
    {

        cm.TransitionToGameplaySnapshot();
        Debug.Log("[Settings]  CloseInfoUI called");
        DefaultView();
        OnInfoClosed?.Invoke();
    }


    public void Info_UpgradeMenu_ButtonClick()
    {
        cm.PlayButtonClickSound();
        Debug.Log("[Settings]  Upgrade Menu Button Clicked");
        HideAllScreens();
        ShowUpgradeScreen();
    }

    public void Info_ContinueButtonClick()
    {
        cm.TransitionToGameplaySnapshot();
        DefaultView();
        OnInfoClosed?.Invoke();
    }
    public void ShowPreDayUI(int day)
    {
        BlockUI(true); // Block UI interactions
        cm.TransitionToDimmedSnapshot();
        // Debug.Log($"[Overlay] ShowPreDayUI: starting day {day}");
        PreDay_DayText.text = $"Day {day} Starting";
        PreDay_ScoreText.text = $"Make {customerData.GetRansom(day)} COINS OR ELSE!";
        PreDay_HiddenStashText.text = $"Hidden Stash: {customerData.getCustomerCoins()} COINS";
        HideAllScreens();
        PreDay_Screen.SetActive(true);
    }
    public void ClosePreDayUI()
    {

        cm.TransitionToGameplaySnapshot();
        Debug.Log("[Settings]  ClosePreDayUI called");
        DefaultView();
        OnPreDayClosed?.Invoke();
    }
    public void ShowFinalDayUI()
    {
        BlockUI(true); // Block UI interactions
        cm.TransitionToDimmedSnapshot();
        HideAllScreens();
        Final_Day_Screen.SetActive(true);
        // Debug.Log($"[Overlay] ShowFinalDayUI: starting day {customerData.Day}");
    }

    public void FinalDayScreen_ExitButtonClick()
    {
        cm.PlayButtonClickSound();
        Debug.Log("[Overlay] Exit Game");
        Application.Quit();
        // (in the Editor this won’t do anything, but in a build it will quit)
    }


    // called by close in Pause menu causes the massive fuckinng error 
    public void FinalDayScreen_RestartButtonClick()
    {
        cm.TransitionToGameplaySnapshot();
        cm.PlayButtonClickSound();
        Debug.Log("[Overlay] Restart Game");
        customerData.ResetEverything();
        GameSceneManager.instance.BackToMainMenu();
    }


    public void ShowFailureUI(int day, int EarnedCoins, int YakuzaDeduction)
    {
        BlockUI(true); // Block UI interactions
        cm.TransitionToDimmedSnapshot();
        HideAllScreens();
        Failure_Screen.SetActive(true);
        Failure_Title.text = $"Day {day} Failed!";
        Failure_Description.text = $"You were short {YakuzaDeduction - EarnedCoins} COINS TODAY !";
    }

    public void Failure_QuitButtonClick()
    {
        cm.PlayButtonClickSound();
        Debug.Log("[Overlay] Exit Game");
        Application.Quit();
        // (in the Editor this won’t do anything, but in a build it will quit)
    }


    // Success button to exit to main menu 
    public void Failure_RestartButtonClick()
    {
        cm.TransitionToGameplaySnapshot();
        cm.PlayButtonClickSound();
        Debug.Log("[Overlay] Restart Game");
        customerData.ResetEverything();
        GameSceneManager.instance.BackToMainMenu();
    }

    // Need to find out if scripts are refrenceing this 
    public void CloseFailureUI()
    {

        Debug.Log("[Settings]  CloseFailureUI called");
        DefaultView();
    }


    public void OnPointerClick(PointerEventData eventData)

    {
        Debug.Log("[Settings]  Settings UI received click");
    }

    public void HideAllScreens()
    {
        Game_Screen.SetActive(false);
        Pause_Screen.SetActive(false);
        Info_Screen.SetActive(false);
        PreDay_Screen.SetActive(false);
        Final_Day_Screen.SetActive(false);
        Failure_Screen.SetActive(false);
        UpgradeScreen.SetActive(false);
    }

    public void ShowUpgradeScreen()
    {
        BlockUI(true); // Block UI interactions
        HideAllScreens();
        Upgrade_RefreshUI();
        cm.TransitionToDimmedSnapshot();
        UpgradeScreen.SetActive(true);
        Upgrade_CointCount.text = $"{customerData.CustomerCoins}";

    }

    public void CloseUpgradeScreen()
    {
        cm.TransitionToGameplaySnapshot();
        Debug.Log("[Settings]  CloseUpgradeScreen called");
        DefaultView();
        OnInfoClosed?.Invoke();
    }

    public void Upgrade_IncremenetFAA_ButtonClick()
    {
        int currentValue = customerData.FoodAssemblyAreaCount;
        if (currentValue < FoodAssemblyAreaMaxCount)
        {
            int requiredCoins = GetFAAUpgradeCost();
            if (customerData.CustomerCoins >= requiredCoins)
            {
                customerData.CustomerCoins -= requiredCoins;
                customerData.IncrementFAA();
                Upgrade_RefreshUI();
            }
            else
            {
                Debug.Log("Not enough coins to upgrade Food Assembly Area Count!");
            }
        }
        else
        {
            Debug.Log("Max Food Assembly Area Count reached!");
        }
    }

    public void Upgrade_CuttingSpeed_ButtonClick()
    {
        int currentValue = customerData.CuttingSpeed;
        if (currentValue < CuttingSpeedMaxLevel)
        {
            int requiredCoins = GetCuttingSpeedUpgradeCost();
            if (customerData.CustomerCoins >= requiredCoins)
            {
                customerData.CustomerCoins -= requiredCoins;
                customerData.IncrementCuttingSpeed();
                Upgrade_RefreshUI();
            }
            else
            {
                Debug.Log("Not enough coins to upgrade Cutting Speed!");
            }
        }
        else
        {
            Debug.Log("Max Cutting Speed reached!");
        }
    }


    public void Upgrade_IncremenetGrill_ButtonClick()
    {
        int currentValue = customerData.GrillAreaCount;
        if (currentValue < GrillAreaMaxCount)
        {
            int requiredCoins = GetGrillAreaUpgradeCost();

            if (customerData.CustomerCoins >= requiredCoins)
            {
                customerData.CustomerCoins -= requiredCoins;
                customerData.IncrementGrillArea();
                Upgrade_RefreshUI();
            }
            else
            {
                Debug.Log("Not enough coins to upgrade Grill Assembly Area Count!");
            }
        }
        else
        {
            Debug.Log("Max Grill Assembly Count reached!");
        }
    }


    public void Upgrade_RefreshUI()
    {
        Upgrade_CointCount.text = $"{customerData.CustomerCoins}";
        Upgrade_FoodAssemblyCount.text = $"{customerData.FoodAssemblyAreaCount}";
        if (customerData.FoodAssemblyAreaCount < FoodAssemblyAreaMaxCount)
        {
            Upgrade_RequiredCoinsCount.text = $"{GetFAAUpgradeCost()}";
        }
        else
        {
            Upgrade_RequiredCoinsCount.text = $"Max";
        }

        Upgrade_GrillCount.text = $"{customerData.GrillAreaCount}";
        if (customerData.GrillAreaCount < GrillAreaMaxCount)
        {
            Upgrade_GrillCost.text = $"{GetGrillAreaUpgradeCost()}";
        }
        else
        {
            Upgrade_GrillCost.text = $"Max";
        }

        Upgrade_GrillSpeed.text = $"{customerData.GrillSpeedCount}";
        if (customerData.GrillSpeedCount < FoodAssemblyLevelMaxCount)
        {
            Upgrade_GrillSpeedCost.text = $"{GetGrillSpeedUpgradeCost()}";
        }
        else
        {
            Upgrade_GrillSpeedCost.text = $"Max";
        }
        Upgrade_CustomerPatience.text = $"{customerData.PatienceLevel}";
        if (customerData.PatienceLevel < PatienceLevelMaxCount)
        {
            Upgrade_CustomerPatienceCost.text = $"{GetPatienceLevelUpgradeCost()}";
        }
        else
        {
            Upgrade_CustomerPatienceCost.text = $"Max";
        }

        Upgrade_CuttingSpeed.text = $"{customerData.CuttingSpeed}";
        if (customerData.CuttingSpeed < CuttingSpeedMaxLevel)
        {
            Upgrade_CuttingSpeedCost.text = $"{GetCuttingSpeedUpgradeCost()}";
        }
        else
        {
            Upgrade_CuttingSpeedCost.text = $"Max";
        }
    }


    public void Upgrade_GrillSpeed_ButtonClick()
    {
        int currentValue = customerData.GrillSpeedCount;
        if (currentValue < GrillMaxSpeedCount)
        {
            int requiredCoins = GetGrillSpeedUpgradeCost();

            if (customerData.CustomerCoins >= requiredCoins)
            {
                customerData.CustomerCoins -= requiredCoins;
                customerData.IncrementGrillSpeed();
                Upgrade_RefreshUI();
            }
            else
            {
                Debug.Log("Not enough coins to upgrade GrillSpeed Area Count!");
            }
        }
        else
        {
            Debug.Log("Max GrillSpeed Count reached!");
        }
    }

    public void Upgrade_CustomerPatience_ButtonClick()
    {
        int currentValue = customerData.PatienceLevel;
        if (currentValue < PatienceLevelMaxCount)
        {
            int requiredCoins = GetPatienceLevelUpgradeCost();

            if (customerData.CustomerCoins >= requiredCoins)
            {
                customerData.CustomerCoins -= requiredCoins;
                customerData.IncrementPatienceLevel();
                Upgrade_RefreshUI();
            }
            else
            {
                Debug.Log("Not enough coins to upgrade  Customer Patience Count!");
            }
        }
        else
        {
            Debug.Log("Max Customer Patience!");
        }
    }

    public int GetFAAUpgradeCost()
    {
        return FAA_Cost[customerData.FoodAssemblyAreaCount - 2];
    }

    public int GetGrillAreaUpgradeCost()
    {
        return GrillAreaCost[customerData.GrillAreaCount - 2];
    }

    public int GetGrillSpeedUpgradeCost()
    {
        return GrillSpeedCost[customerData.GrillSpeedCount - 1];
    }

    public int GetPatienceLevelUpgradeCost()
    {
        return PatienceLevelCost[customerData.PatienceLevel - 1];
    }

    public int GetCuttingSpeedUpgradeCost()
    {
        return CuttingSpeedCost[customerData.CuttingSpeed - 1];
    }
}