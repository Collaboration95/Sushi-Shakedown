using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DaySnapshot
{
    public int day;
    public int score;
    public int customersServed;
    public int happyCustomers;
    public int angryCustomers;
}


public class DayManager : DebuggableMonoBehaviour
{
    public CustomerData customerDataSO;
    public NPCSpawner npcSpawner; // assign via the Inspector
    public WaveManager waveManager; // assign via the Inspector
    public OverLayManager OM;
    public ScoreParent ScoreParent; // assign via the Inspector
    public List<DaySnapshot> dayHistory = new List<DaySnapshot>();
    protected override void OnEnable()
    {
        base.OnEnable(); // Call the base class 
        if (waveManager == null || customerDataSO == null || OM == null)
        {
            Debug.LogError("DayManager requires WaveManager, CustomerData and Overlay.", this);
            enabled = false;
            return;
        }
        waveManager.OnWavesCompleted += OnWavesCompleted;
        customerDataSO.OnGameModeChanged += OnModeChanged;
        OM.OnPreDayClosed += HandleStartWaves;
        OM.OnInfoClosed += OnInfoClosed;
        UpdateLogStatus();

    }

    protected override void OnDisable()
    {
        base.OnDisable(); // Call the base class method to clean up logging
        if (waveManager != null) waveManager.OnWavesCompleted -= OnWavesCompleted;
        if (customerDataSO != null) customerDataSO.OnGameModeChanged -= OnModeChanged;
        if (OM != null)
        {
            OM.OnPreDayClosed -= HandleStartWaves;
            OM.OnInfoClosed -= OnInfoClosed;
        }
    }
    public void Start()
    {
        if (ScoreParent == null) ScoreParent = FindFirstObjectByType<ScoreParent>();
        if (ScoreParent == null) { Debug.LogError("DayManager requires ScoreParent.", this); enabled = false; return; }
        RuntimeLog.Write("DayManager: Start() called.");
        RuntimeLog.Write("Day is " + customerDataSO.Day + " in Start() method.");
        StartDay();
        // OnModeChanged(customerDataSO.gameMode);
    }
    void StartDay()
    {

        if (customerDataSO.Day > customerDataSO.maxDays)
        {
            OM.ShowFinalDayUI();
            return;
        }

        RuntimeLog.Write($"--- Starting Day {customerDataSO.Day} ---");
        // reset daily stats
        customerDataSO.WaveCount = 0;
        customerDataSO.customersServed = 0;
        customerDataSO.score = customerDataSO.CustomerCoins;
        customerDataSO.HappyCustomerCount = 0;
        customerDataSO.normalCustomersCount = 0;
        customerDataSO.angryCustomersCount = 0;
        ScoreParent.SetScore(customerDataSO.score);

        if (customerDataSO.gameMode == GameMode.Waves)
            OM.ShowPreDayUI(customerDataSO.Day);
        // OM.ShowUpgradeScreen();
        else
            OnModeChanged(GameMode.FreePlay); // Start FreePlay mode immediately
    }

    public void HandleStartWaves()
    {
        if (customerDataSO.Day > customerDataSO.maxDays)
        {
            RuntimeLog.Write("Max days reached in HandleStartWaves — showing final UI.");
            OM.ShowFinalDayUI();
            return;
        }

        OnModeChanged(GameMode.Waves);
    }


    private void OnModeChanged(GameMode mode)
    {
        if (mode == GameMode.Waves)
        {
            RuntimeLog.Write("Day is ." + customerDataSO.Day + " in Waves mode.");

            waveManager.StopEndlessCustomers();
            waveManager.StartWaves();
        }
        else
        {
            waveManager.StopWaves();
            waveManager.StartEndlessCustomers();
        }
    }

    private void OnInfoClosed()
    {

        dayHistory.Add(new DaySnapshot
        {
            day = customerDataSO.Day,
            score = customerDataSO.score,
            customersServed = customerDataSO.customersServed,
            happyCustomers = customerDataSO.HappyCustomerCount,
            angryCustomers = customerDataSO.angryCustomersCount
        });

        if (customerDataSO.Day < customerDataSO.maxDays)
        {
            customerDataSO.Day++;
            StartDay();
        }
        else
        {
            RuntimeLog.Write("All days complete! Transition to endgame...");

            // TODO: show final results / return to menu / quit
        }
    }


    protected override void UpdateLogStatus()
    {
        isDebugEnabled = logSettings != null && logSettings.DayManagerLogs;
    }


    private void OnWavesCompleted()
    {
        Log("Waves completed. Summarizing the day...");
        PrintDaySummary();

        int EarnedCoins = customerDataSO.CustomerCoins;

        int YakuzaDeduction = customerDataSO.GetRansom(customerDataSO.Day);
        RuntimeLog.Write($"Yakuza deduction for day {customerDataSO.Day} is {YakuzaDeduction} coins.");
        if (EarnedCoins < YakuzaDeduction)
        {
            RuntimeLog.Write("Not enough coins to pay the Yakuza! Game over.");
            OM.ShowFailureUI(customerDataSO.Day, EarnedCoins, YakuzaDeduction);

        }
        else
        {
            // customerDataSO.IncrementCustomerCoins(customerDataSO.score);
            customerDataSO.DecrementCustomerCoins(YakuzaDeduction);

            if (customerDataSO.Day == customerDataSO.maxDays)
            {
                OM.ShowFinalDayUI();
            }
            else
            {

                OM.ShowInfoUI(customerDataSO.Day, customerDataSO.score, customerDataSO.customersServed, customerDataSO.HappyCustomerCount, customerDataSO.angryCustomersCount, customerDataSO.CustomerCoins, YakuzaDeduction);
            }
        }
    }

    void PrintDaySummary()
    {
        string summary = $"Day Summary - Day: {customerDataSO.Day}, Score: {customerDataSO.score}, Total Served: {customerDataSO.customersServed}, Happy: {customerDataSO.HappyCustomerCount}, Angry: {customerDataSO.angryCustomersCount}";
        Log(summary);
    }

}
