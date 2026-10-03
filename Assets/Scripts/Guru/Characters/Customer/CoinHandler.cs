using System.Collections;
using UnityEngine;

public class CoinHandler : MonoBehaviour
{
    [Header("Coin FX")]
    public GameObject coinPrefab;
    public float randomSpread = 0.5f;

    [Header("Target (score icon)")]
    public GameObject scoreTarget;   // drag the UI Image / Text here
    private float travelTime = .7f;
    private Camera sceneCamera;
    private Canvas targetCanvas;
    private readonly System.Collections.Generic.HashSet<GameObject> coins = new System.Collections.Generic.HashSet<GameObject>();
    public ScoreParent ScoreParent; // Reference to the ScoreParent script

    public static CoinHandler Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Start()
    {
        if (scoreTarget == null) scoreTarget = GameObject.Find("CoinFloating");
        if (ScoreParent == null) ScoreParent = FindFirstObjectByType<ScoreParent>();
        sceneCamera = Camera.main;
        if (scoreTarget != null) targetCanvas = scoreTarget.GetComponentInParent<Canvas>();

    }

    // --------------------------------------------------------------------
    public void SpawnCoins(int count, Vector3 worldSpawnPos)
    {
        if (!isActiveAndEnabled || coinPrefab == null || scoreTarget == null || ScoreParent == null || sceneCamera == null) return;
        StartCoroutine(SpawnCoinsRoutine(count, worldSpawnPos, 0.1f));
    }

    private IEnumerator SpawnCoinsRoutine(int count, Vector3 worldSpawnPos, float delayBetweenSpawns)
    {
        var delay = new WaitForSeconds(delayBetweenSpawns);
        for (int i = 0; i < count; i++)
        {

            GameObject coin = Instantiate(
                coinPrefab,
                worldSpawnPos,
                Quaternion.identity,
                null
            );
            coins.Add(coin);
            StartCoroutine(MoveCoinToTarget(coin));

            // wait before spawning the next one
            yield return delay;
        }
    }

    private IEnumerator MoveCoinToTarget(GameObject coin)
    {
        Vector3 targetWorld = GetTargetWorldPos();

        // Compute speed so that it would have taken `travelTime` to cover the full distance
        float totalDistance = Vector3.Distance(coin.transform.position, targetWorld);
        float speed = totalDistance / travelTime;

        // Move until you actually reach (or very nearly reach) the target
        while (coin != null &&
               Vector3.Distance(coin.transform.position, targetWorld) > 0.065f)
        {
            // RuntimeLog.Write("Coin distance to target: " +
            //           Vector3.Distance(coin.transform.position, targetWorld));
            coin.transform.position = Vector3.MoveTowards(
                coin.transform.position,
                targetWorld,
                speed * Time.deltaTime
            );

            yield return null;
        }
        coins.Remove(coin);
        if (coin != null)
        {
            if (ScoreParent != null) ScoreParent.DeleteCoin(coin);
            else Destroy(coin);
        } // Call the static method to handle coin collection
        // RuntimeLog.Write("Coin reached target: " + coin);
        // Destroy(coin); // destroy the coin when it reaches the target
    }


    void OnDisable()
    {
        StopAllCoroutines();
        foreach (var coin in coins) if (coin != null) Destroy(coin);
        coins.Clear();
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    // Convert the UI element’s screen position to world space (for Overlay / Screen‑space canvases)
    private Vector3 GetTargetWorldPos()
    {
        Canvas canvas = targetCanvas;
        if (canvas != null && canvas.renderMode != RenderMode.WorldSpace)
        {
            Vector3 screen = RectTransformUtility.WorldToScreenPoint(null, scoreTarget.transform.position);
            Vector3 world = sceneCamera.ScreenToWorldPoint(screen);
            world.z = 0f;                       // keep on 0 plane
            return world;
        }
        // World‑space Canvas or world object
        return scoreTarget.transform.position;
    }
}
