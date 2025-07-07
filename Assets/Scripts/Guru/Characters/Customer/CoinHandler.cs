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
    public ScoreParent ScoreParent; // Reference to the ScoreParent script

    public static CoinHandler Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Start()
    {
        scoreTarget = GameObject.Find("CoinFloating");
        ScoreParent = GameObject.Find("Score").GetComponent<ScoreParent>();

    }

    // --------------------------------------------------------------------
    public void SpawnCoins(int count, Vector3 worldSpawnPos)
    {
        // start the coroutine that handles both spawning and delays
        StartCoroutine(SpawnCoinsRoutine(count, worldSpawnPos, 0.1f));
    }

    private IEnumerator SpawnCoinsRoutine(int count, Vector3 worldSpawnPos, float delayBetweenSpawns)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 offset = Random.insideUnitCircle * randomSpread;
            GameObject coin = Instantiate(
                coinPrefab,
                worldSpawnPos,
                Quaternion.identity,
                null
            );
            StartCoroutine(MoveCoinToTarget(coin));

            // wait before spawning the next one
            yield return new WaitForSeconds(delayBetweenSpawns);
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
            // Debug.Log("Coin distance to target: " +
            //           Vector3.Distance(coin.transform.position, targetWorld));
            coin.transform.position = Vector3.MoveTowards(
                coin.transform.position,
                targetWorld,
                speed * Time.deltaTime
            );

            yield return null;
        }
        ScoreParent.DeleteCoin(coin); // Call the static method to handle coin collection
        // Debug.Log("Coin reached target: " + coin);
        // Destroy(coin); // destroy the coin when it reaches the target
    }


    // Convert the UI element’s screen position to world space (for Overlay / Screen‑space canvases)
    private Vector3 GetTargetWorldPos()
    {
        Canvas canvas = scoreTarget.GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.WorldSpace)
        {
            Vector3 screen = RectTransformUtility.WorldToScreenPoint(null, scoreTarget.transform.position);
            Vector3 world = Camera.main.ScreenToWorldPoint(screen);
            world.z = 0f;                       // keep on 0 plane
            return world;
        }
        // World‑space Canvas or world object
        return scoreTarget.transform.position;
    }
}
