using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomerData", menuName = "ScriptableObjects/CustomerData")]
public class CustomerData : ScriptableObject
{
    public GameMode gameMode;

    // Existing
    public int customersServed;
    public event Action<int> OnCustomerServed;
    public Difficulty difficulty = Difficulty.Easy;
    public event Action<Difficulty> OnDifficultyChanged;

    public int[] Ransom = { 10, 26, 34, 4, 5, 6, 7 };

    public int Day = 1;
    public int WaveCount = 0;


    public int maxDays = 7;

    // NEW: score tracking
    public int score = 0;
    public int normalCustomersCount = 0;
    public int HappyCustomerCount = 0;
    public int angryCustomersCount = 0;
    public event Action<int> OnScoreChanged;
    public event Action<GameMode> OnGameModeChanged;

    public int CustomerCoins = 0;
    public int getCustomerCoins() => CustomerCoins;
    public void setCustomerCoins(int amount) => CustomerCoins = amount;
    public void IncrementCustomerCoins(int amount) => CustomerCoins += amount;
    public void DecrementCustomerCoins(int amount) => CustomerCoins -= amount;

    public void ResetCustomerCoins() => CustomerCoins = 10;
    public void ResetCustomerCount() => customersServed = 0;
    public void ResetDay() => Day = 1;
    public void ResetWaveCount() => WaveCount = 0;



    public event Action<int> OnFAA_Increased;
    public event Action<int> OnGrillArea_Increased;
    public event Action<int> OnGrillSpeed_Increased;
    public event Action<int> OnPatienceLevel_Increased;

    public event Action<int> OnCuttingSpeed_Increased;
    public int CuttingSpeed = 1;


    public int GrillAreaCount = 2;
    public int FoodAssemblyAreaCount = 2;
    public int GrillSpeedCount = 1;

    public int PatienceLevel = 1;

    public void IncrementPatienceLevel()
    {
        PatienceLevel++;
        OnPatienceLevel_Increased?.Invoke(PatienceLevel);
    }

    public void IncrementFAA()
    {
        FoodAssemblyAreaCount++;
        OnFAA_Increased?.Invoke(FoodAssemblyAreaCount);
    }

    public void IncrementCuttingSpeed()
    {
        CuttingSpeed++;
        OnCuttingSpeed_Increased?.Invoke(CuttingSpeed);
    }

    public void IncrementGrillArea()
    {
        GrillAreaCount++;
        OnGrillArea_Increased?.Invoke(GrillAreaCount);
    }

    public void IncrementGrillSpeed()
    {
        GrillSpeedCount++;
        OnGrillSpeed_Increased?.Invoke(GrillAreaCount);
    }

    private void OnEnable()
    {
        // Initialize the data when the scriptable object is enabled.
        OnStartup();
        // gameMode = GameMode.Waves; // Default mode
    }

    public int GetRansom(int day)
    {
        // day is 0 indexed
        return Ransom[(day - 1) % Ransom.Length];
    }

    public void SetGameMode(GameMode mode)
    {
        if (gameMode != mode)
        {
            gameMode = mode;
            Debug.Log($"Game mode set to: {gameMode}");
            OnGameModeChanged?.Invoke(gameMode);
        }
    }

    public void SetDifficulty(Difficulty diff)
    {

        if (difficulty != diff)
        {
            difficulty = diff;
            Debug.Log($"Game difficulty set to  : {difficulty}");
            OnDifficultyChanged?.Invoke(difficulty);
        }
    }

    public void OnStartup()
    {
        maxDays = 7;
        customersServed = 0;
        WaveCount = 0;
        score = 0;
        normalCustomersCount = 0;
        HappyCustomerCount = 0;
        angryCustomersCount = 0;
        if (Day == 1)
        {
            CustomerCoins = 10; // Start at day 1
        }
        Debug.Log("CustomerData initialized.");
    }

    public void Increment()
    {
        customersServed++;
        OnCustomerServed?.Invoke(customersServed);
    }

    public void AddScore(int amount = 1)
    {

        score += amount;
        IncrementCustomerCoins(amount);
        // Debug.Log("Score is " + score);
        if (amount > 5)
            HappyCustomerCount++;
        else
            normalCustomersCount++;
        // Debug.Log($"Score +{amount}. Total: {score}. Normal served: {normalCustomersCount}");
        OnScoreChanged?.Invoke(score);
    }

    public void DeductScore(int amount)
    {
        score -= amount;
        DecrementCustomerCoins(amount);
        angryCustomersCount++;
        // Debug.Log($"Score –{amount}. Total: {score}. Angry served: {angryCustomersCount}");
        OnScoreChanged?.Invoke(score);
    }

    public void ResetScore()
    {
        score = 0;
        normalCustomersCount = 0;
        angryCustomersCount = 0;
        HappyCustomerCount = 0;
        Debug.Log("Score data reset.");
        OnScoreChanged?.Invoke(score);
    }

    public void ResetEverything()
    {
        ResetCustomerCount();
        ResetDay();
        ResetWaveCount();
        ResetScore();
        ResetCustomerCoins();
        Debug.Log("All data reset.");
        GrillAreaCount = 2;
        FoodAssemblyAreaCount = 2;
        GrillSpeedCount = 1;
        PatienceLevel = 1;
        CuttingSpeed = 1;
    }
}

public enum GameMode
{
    FreePlay,
    Waves
}

public enum Difficulty
{
    Easy,
    Hard
}