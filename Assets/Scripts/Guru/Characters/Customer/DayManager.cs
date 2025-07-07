using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
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
        waveManager.OnWavesCompleted += OnWavesCompleted;
        customerDataSO.OnGameModeChanged += OnModeChanged;
        OM.OnPreDayClosed += HandleStartWaves;
        OM.OnInfoClosed += OnInfoClosed;
        UpdateLogStatus();

    }

    protected override void OnDisable()
    {
        base.OnDisable(); // Call the base class method to clean up logging
        waveManager.OnWavesCompleted -= OnWavesCompleted;
        customerDataSO.OnGameModeChanged -= OnModeChanged;
        OM.OnPreDayClosed -= HandleStartWaves;
        OM.OnInfoClosed -= OnInfoClosed;
    }
    public void Start()
    {
        ScoreParent = GameObject.Find("Score").GetComponent<ScoreParent>();
        Debug.Log("DayManager: Start() called.");
        Debug.Log("Day is " + customerDataSO.Day + " in Start() method.");
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

        Debug.Log($"--- Starting Day {customerDataSO.Day} ---");
        // reset daily stats
        customerDataSO.WaveCount = 0;
        customerDataSO.customersServed = 0;
        customerDataSO.score = customerDataSO.CustomerCoins;
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
            Debug.Log("Max days reached in HandleStartWaves — showing final UI.");
            OM.ShowFinalDayUI();
            return;
        }

        OnModeChanged(GameMode.Waves);
    }


    private void OnModeChanged(GameMode mode)
    {
        if (mode == GameMode.Waves)
        {
            Debug.Log("Day is ." + customerDataSO.Day + " in Waves mode.");

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
            Debug.Log("All days complete! Transition to endgame...");

            // TODO: show final results / return to menu / quit
        }
    }


    protected override void UpdateLogStatus()
    {
        isDebugEnabled = logSettings.DayManagerLogs;
    }


    private void OnWavesCompleted()
    {
        Log("Waves completed. Summarizing the day...");
        PrintDaySummary();

        int EarnedCoins = customerDataSO.CustomerCoins;

        int YakuzaDeduction = customerDataSO.GetRansom(customerDataSO.Day);
        Debug.Log($"Yakuza deduction for day {customerDataSO.Day} is {YakuzaDeduction} coins.");
        if (EarnedCoins < YakuzaDeduction)
        {
            Debug.Log("Not enough coins to pay the Yakuza! Game over.");
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
