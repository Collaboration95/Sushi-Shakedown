using System.Collections;
using UnityEngine;
using System.Runtime.CompilerServices;

// public class WaveManager : DebuggableMonoBehaviour
// {

//     public NPCSpawner npcSpawner;
//     public float waveCountdownDuration = 1f;
//     public CustomerData customerData;
//     public int[] waveSizes = { 1 };
//     private int tempWaveLimit = 1;
//     private Coroutine _endlessRoutine;
//     private bool _wavesRunning;

//     private Coroutine currentWaveRoutine = null;
//     public event System.Action OnWavesCompleted;
//     public event System.Action<string> OnWaveStatusChanged;

//     public OrderAreaGroup orderAreaGroup; // assign via the Inspector 

//     private bool endlessModeActive = false;
//     protected override void OnEnable()
//     {
//         base.OnEnable(); // Call the base class method to set up logging
//     }

//     protected override void OnDisable()
//     {
//         base.OnDisable(); // Call the base class method to clean up logging
//     }

//     public void StartWaves()
//     {
//         if (_wavesRunning) return;
//         StopWaves();
//         _wavesRunning = true;
//         Debug.Log("WaveManager: Starting waves.");
//         currentWaveRoutine = StartCoroutine(RunWaves());
//         OnWaveStatusChanged?.Invoke("Waves are starting!");
//     }

//     public void StopWaves()
//     {
//         if (currentWaveRoutine != null)
//             StopCoroutine(currentWaveRoutine);
//         currentWaveRoutine = null;
//         _wavesRunning = false;
//     }
//     protected override void UpdateLogStatus()
//     {
//         isDebugEnabled = logSettings.WaveManagerLogs;
//     }

//     protected override void Awake()
//     {
//         base.Awake();
//     }
//     public class WaveStats
//     {
//         public int waveNumber;
//         public int customersServed;
//         public int score;
//         public int normalCustomersCount;
//         public int angryCustomersCount;
//     }

//     private WaveStats GetWaveStats()
//     {
//         return new WaveStats
//         {
//             waveNumber = customerData.WaveCount,
//             customersServed = customerData.customersServed,
//             score = customerData.score,
//             normalCustomersCount = customerData.normalCustomersCount,
//             angryCustomersCount = customerData.angryCustomersCount
//         };
//     }

//     /// <summary>
//     /// Starts the waves.
//     /// </summary>

//     IEnumerator RunWaves()
//     {
//         for (int i = 0; i < waveSizes.Length; i++)
//         {
//             int waveNumber = i + 1;
//             string msg = $"--- Wave {waveNumber}: Preparing to start ---";

//             Log(msg);

//             // Capture stats before the wave begins.
//             WaveStats startStats = GetWaveStats();

//             // Countdown.
//             yield return StartCoroutine(WaveCountdown(waveCountdownDuration));

//             // Update the global number.
//             customerData.WaveCount = waveNumber;

//             msg = $"Wave {waveNumber} started!";
//             Log(msg);
//             OnWaveStatusChanged?.Invoke(msg);

//             // Spawn the wave.
//             yield return StartCoroutine(SpawnWave(waveSizes[i], 1f, waveNumber));

//             // Wait a moment between waves.
//             yield return new WaitForSeconds(2f);

//             // Print a summary using the difference between start and current stats.
//             PrintWaveSummary(startStats, GetWaveStats());
//         }

//         // after the for‐loop
//         // Wait until all order areas are free, but only check every 0.5 seconds
//         while (!orderAreaGroup.AreAllOrderAreasFree())
//         {
//             yield return new WaitForSeconds(0.5f); // check every half-second
//         }

//         string msg2 = "All waves for the day are complete!";
//         Log(msg2);
//         OnWaveStatusChanged?.Invoke("All waves are over!");
//         StopWaves();
//         OnWavesCompleted?.Invoke();

//     }
//     public void StartEndlessCustomers()
//     {
//         StopWaves();
//         StopEndlessCustomers();            // just in case
//         endlessModeActive = true;
//         _endlessRoutine = StartCoroutine(EndlessCustomersRoutine());
//     }

//     public void StopEndlessCustomers()
//     {
//         endlessModeActive = false;
//         if (_endlessRoutine != null)
//         {
//             StopCoroutine(_endlessRoutine);
//             _endlessRoutine = null;
//         }
//     }
//     private IEnumerator EndlessCustomersRoutine()
//     {
//         int temp = 0;
//         while (endlessModeActive)
//         {
//             // Debug.Log("Endless Customer Round : " + temp);

//             float waitTime = Random.Range(1f, 2f);
//             yield return new WaitForSeconds(waitTime);

//             bool didSpawn = npcSpawner.SpawnCustomer();
//             if (didSpawn)
//             {
//                 // Log($"Spawned a customer: " + temp);
//                 // Debug.Log("Endless Customer Finish Round : " + temp);
//                 OnWaveStatusChanged?.Invoke($"served: {customerData.customersServed}");
//             }
//             else
//             {
//                 Log("All order areas are full—will retry later.");
//             }
//             temp += 1;
//         }

//     }

//     /// <summary>
//     /// If you ever want to stop the endless spawning.
//     /// </summary>

//     IEnumerator WaveCountdown(float seconds)
//     {
//         float count = seconds;
//         while (count > 0)
//         {
//             yield return new WaitForSeconds(1f);
//             count -= 1f;
//         }
//     }

//     // Modified SpawnWave which tracks and displays the current count as "Wave X: A/B"
//     public IEnumerator SpawnWave(int customerCount, float interval, int waveNumber)
//     {
//         int remaining = customerCount;
//         int spawnedCount = 0; // number successfully spawned
//         Debug.Log($"Spawning {customerCount} customers in wave {waveNumber}.");
//         while (remaining > 0)
//         {
//             bool didSpawn = npcSpawner.SpawnCustomer();
//             if (didSpawn)
//             {
//                 spawnedCount++;
//                 remaining--;
//                 string spawnMsg = $"Wave {waveNumber}: {spawnedCount}/{customerCount}";
//                 OnWaveStatusChanged?.Invoke(spawnMsg);
//                 Log(spawnMsg);
//                 yield return new WaitForSeconds(interval);
//             }
//             else
//             {
//                 Log("Spawn failed — will retry next tick.");
//                 yield return null;
//             }
//         }
//         Log("Wave complete!");
//     }

//     void PrintWaveSummary(WaveStats startStats, WaveStats currentStats)
//     {
//         Log(
//             $"--- Wave {currentStats.waveNumber} Summary ---\n" +
//             $"Total Score: {currentStats.score - startStats.score}\n" +
//             $"Customers Served: {currentStats.customersServed - startStats.customersServed}\n" +
//             $"  • Happy/Normal: {currentStats.normalCustomersCount - startStats.normalCustomersCount}\n" +
//             $"  • Angry/Failed: {currentStats.angryCustomersCount - startStats.angryCustomersCount}\n" +
//             $"------------------------------"
//         );
//     }
// }

public class WaveManager : DebuggableMonoBehaviour
{
    // ──────────────────────────── existing fields ────────────────────────────
    public NPCSpawner npcSpawner;
    public float waveCountdownDuration = 1f;
    public CustomerData customerData;
    /*  OLD: int[] waveSizes = { 1 };       */
    /*  OLD: private int tempWaveLimit = 1; */
    // ^― no longer needed for time-based waves

    private Coroutine _endlessRoutine;
    private bool _wavesRunning;
    private Coroutine currentWaveRoutine = null;

    public event System.Action OnWavesCompleted;
    public event System.Action<string> OnWaveStatusChanged;
    public OrderAreaGroup orderAreaGroup;  // assign via Inspector
    private bool endlessModeActive = false;

    public Hypno hypno;

    // ─────────────────────────── NEW time-wave settings ──────────────────────
    [Header("Time-based wave settings")]
    [SerializeField] private float waveDurationSeconds = 120f;  // N seconds
    [SerializeField] private float spawnInterval = 1.0f; // seconds between spawns
    [SerializeField] private Vector2 spawnJitter = new Vector2(0f, 0.3f); // optional random extra delay

    // ─────────────────────────── life-cycle boilerplate (unchanged) ──────────
    protected override void OnEnable() { base.OnEnable(); }
    protected override void OnDisable() { base.OnDisable(); }
    protected override void Awake() { base.Awake(); }

    protected override void UpdateLogStatus()
    {
        isDebugEnabled = logSettings.WaveManagerLogs;
    }

    // ─────────────────────────── PUBLIC API ──────────────────────────────────
    public void StartWaves()
    {
        if (_wavesRunning) return;

        StopWaves();
        _wavesRunning = true;

        Debug.Log("WaveManager: Starting timed wave.");
        currentWaveRoutine = StartCoroutine(RunTimedWave());   // MODIFIED
        OnWaveStatusChanged?.Invoke("Wave is starting!");
    }

    public void StopWaves()
    {
        if (currentWaveRoutine != null)
            StopCoroutine(currentWaveRoutine);

        currentWaveRoutine = null;
        _wavesRunning = false;
    }

    // ─────────────────────────── WAVE LOGIC (REWRITTEN) ──────────────────────
    private IEnumerator RunTimedWave()                                  // NEW
    {
        int waveNumber = customerData.WaveCount + 1;  // increment logically
        WaveStats startStats = GetWaveStats();        // capture pre-wave stats

        // Countdown before the wave starts.
        yield return StartCoroutine(WaveCountdown(waveCountdownDuration));

        customerData.WaveCount = waveNumber;
        string msg = $"Wave {waveNumber} started (time-based, {waveDurationSeconds} s)!";
        Log(msg);
        OnWaveStatusChanged?.Invoke(msg);

        // Spawn continuously for the configured duration
        yield return StartCoroutine(SpawnForDuration(waveDurationSeconds, spawnInterval, waveNumber));

        //    while (!orderAreaGroup.AreAllOrderAreasFree())
        //         yield return new WaitForSeconds(0.5f);     // Wait until all order areas are free before finishing
        //orderAreaGroup.BootAllCustomers();
        hypno.SpawnDestroyer();
        PrintWaveSummary(startStats, GetWaveStats());

        Log("Wave complete!");
        OnWaveStatusChanged?.Invoke("Wave over!");
        StopWaves();
        OnWavesCompleted?.Invoke();
    }


    // Spawn customers for <duration> seconds; UI shows "XX s left" only.
    private IEnumerator SpawnForDuration(float duration, float interval, int waveNumber)
    {
        float endTime = Time.time + duration;

        while (Time.time < endTime)
        {
            npcSpawner.SpawnCustomer();   // ignore return; we just keep trying

            float timeLeft = Mathf.Max(0f, endTime - Time.time);
            OnWaveStatusChanged?.Invoke($"{timeLeft:0}s left");
            Log($"Wave timer: {timeLeft:0}s remaining");

            float wait = interval + Random.Range(spawnJitter.x, spawnJitter.y);
            yield return new WaitForSeconds(wait);
        }
    }

    // ─────────────────────────── ENDLESS / FREE-PLAY (unchanged) ─────────────
    public void StartEndlessCustomers()
    {
        StopWaves();
        StopEndlessCustomers();            // just in case
        endlessModeActive = true;
        _endlessRoutine = StartCoroutine(EndlessCustomersRoutine());
    }

    public void StopEndlessCustomers()
    {
        endlessModeActive = false;
        if (_endlessRoutine != null)
        {
            StopCoroutine(_endlessRoutine);
            _endlessRoutine = null;
        }
    }
    private IEnumerator EndlessCustomersRoutine()
    {
        int temp = 0;
        while (endlessModeActive)
        {
            // Debug.Log("Endless Customer Round : " + temp);

            float waitTime = Random.Range(1f, 2f);
            yield return new WaitForSeconds(waitTime);

            bool didSpawn = npcSpawner.SpawnCustomer();
            if (didSpawn)
            {
                // Log($"Spawned a customer: " + temp);
                // Debug.Log("Endless Customer Finish Round : " + temp);
                OnWaveStatusChanged?.Invoke($"served: {customerData.customersServed}");
            }
            else
            {
                Log("All order areas are full—will retry later.");
            }
            temp += 1;
        }

    }


    // ─────────────────────────── HELPERS (mostly unchanged) ──────────────────
    private IEnumerator WaveCountdown(float seconds)
    {
        float count = seconds;
        while (count > 0)
        {
            yield return new WaitForSeconds(1f);
            count -= 1f;
        }
    }


    // OLD SpawnWave(int count…) remains for reference but is unused
    /* public IEnumerator SpawnWave(int customerCount, float interval, int waveNumber) { … } pri*/

    private WaveStats GetWaveStats()
    {
        return new WaveStats
        {
            waveNumber = customerData.WaveCount,
            customersServed = customerData.customersServed,
            score = customerData.score,
            normalCustomersCount = customerData.normalCustomersCount,
            angryCustomersCount = customerData.angryCustomersCount
        };
    }


    void PrintWaveSummary(WaveStats startStats, WaveStats currentStats)
    {
        Log(
            $"--- Wave {currentStats.waveNumber} Summary ---\n" +
            $"Total Score: {currentStats.score - startStats.score}\n" +
            $"Customers Served: {currentStats.customersServed - startStats.customersServed}\n" +
            $"  • Happy/Normal: {currentStats.normalCustomersCount - startStats.normalCustomersCount}\n" +
            $"  • Angry/Failed: {currentStats.angryCustomersCount - startStats.angryCustomersCount}\n" +
            $"------------------------------"
        );
    }
    // ─────────────────────────── NESTED TYPE ─────────────────────────────────
    public class WaveStats
    {
        public int waveNumber;
        public int customersServed;
        public int score;
        public int normalCustomersCount;
        public int angryCustomersCount;
    }
}
