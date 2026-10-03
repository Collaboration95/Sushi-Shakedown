using System.Collections;
using UnityEngine;

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
    protected override void OnDisable()
    {
        StopWaves();
        StopEndlessCustomers();
        base.OnDisable();
    }
    protected override void Awake() { base.Awake(); }

    protected override void UpdateLogStatus()
    {
        isDebugEnabled = logSettings != null && logSettings.WaveManagerLogs;
    }

    // ─────────────────────────── PUBLIC API ──────────────────────────────────
    public void StartWaves()
    {
        if (_wavesRunning || !isActiveAndEnabled || !ValidateDependencies()) return;
        StopEndlessCustomers();
        StopWaves();
        _wavesRunning = true;

        RuntimeLog.Write("WaveManager: Starting timed wave.");
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
        yield return WaveCountdown(waveCountdownDuration);

        customerData.WaveCount = waveNumber;
        string msg = $"Wave {waveNumber} started (time-based, {waveDurationSeconds} s)!";
        Log(msg);
        OnWaveStatusChanged?.Invoke(msg);

        // Spawn continuously for the configured duration
        yield return SpawnForDuration(waveDurationSeconds, spawnInterval, waveNumber);

        //    while (!orderAreaGroup.AreAllOrderAreasFree())
        //         yield return new WaitForSeconds(0.5f);     // Wait until all order areas are free before finishing
        //orderAreaGroup.BootAllCustomers();
        if (hypno != null) hypno.SpawnDestroyer();
        else orderAreaGroup.BootAllCustomers();
        PrintWaveSummary(startStats, GetWaveStats());

        Log("Wave complete!");
        OnWaveStatusChanged?.Invoke("Wave over!");
        currentWaveRoutine = null;
        _wavesRunning = false;
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

            float wait = Mathf.Max(0.02f, interval + Random.Range(spawnJitter.x, spawnJitter.y));
            yield return new WaitForSeconds(Mathf.Min(wait, timeLeft));
        }
    }

    // ─────────────────────────── ENDLESS / FREE-PLAY (unchanged) ─────────────
    public void StartEndlessCustomers()
    {
        if (!isActiveAndEnabled || !ValidateDependencies()) return;
        StopWaves();
        StopEndlessCustomers();
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
            // RuntimeLog.Write("Endless Customer Round : " + temp);

            float waitTime = Random.Range(1f, 2f);
            yield return new WaitForSeconds(waitTime);

            bool didSpawn = npcSpawner.SpawnCustomer();
            if (didSpawn)
            {
                // Log($"Spawned a customer: " + temp);
                // RuntimeLog.Write("Endless Customer Finish Round : " + temp);
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
        if (seconds > 0) yield return new WaitForSeconds(seconds);
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
    public bool IsRunning => _wavesRunning || endlessModeActive;

    private bool ValidateDependencies()
    {
        if (customerData != null && npcSpawner != null && orderAreaGroup != null) return true;
        Debug.LogError("WaveManager requires CustomerData, NPCSpawner and OrderAreaGroup.", this);
        return false;
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
