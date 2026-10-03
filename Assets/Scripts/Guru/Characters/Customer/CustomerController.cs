using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Kept for Text component on textBubble

public class CustomerController : DebuggableMonoBehaviour
{
    // === Movement ===
    public GameObject redexclaim;

    public int OrderMultiplier = 1; // 1 to 3 orders
    public GameObject Meh;
    public GameObject Wow;
    public float speed = 3f;
    public Vector2 targetPosition; // Will be assigned from OrderArea
    public bool hasArrived = false;
    public bool isWalkingOffScreen = false;
    private Vector2 offScreenTarget;

    public Animator customerAnimator; // Animator for the customer

    public AnimatorOverrideController overrideController;
    public AnimatorOverrideController overrideController2;
    public AnimatorOverrideController overrideController3;

    public System.Collections.Generic.List<AnimatorOverrideController> overrideControllers;
    private RuntimeAnimatorController originalController;

    // === UI Elements ===
    public GameObject textBubble;
    public PatienceBar patienceBar;
    public GameObject OrderBubble;
    public OrderBubble orderBubble;
    private Text bubbleText;

    // === Customer Data ===
    public CustomerData customerData;
    public CustomerData CustomerData;

    // === Audio ===
    public AudioClip orderCompletedSound;
    public AudioClip orderFailedSound;
    public AudioSource audioSource;

    // === Sprite / Visuals ===
    private SpriteRenderer spriteRenderer;
    public Sprite happySprite;
    public Sprite frustratedSprite;
    public Sprite angrySprite;

    // === Patience System ===
    public int maxPatience = 100;
    public int patienceBoostOnCorrect = 10;
    private int currentPatience;
    private Coroutine progressRoutine;

    // === Order Assignment ===
    public OrderArea assignedOrderArea; // Exposed if you still want to check later

    // === Difficulty ===
    private int difficultyMultiplier = 1; // Default to Easy

    private int patienceLevelMultiplier = 1;

    public CustomerData cs; // assign via the Inspector
    protected override void Awake()
    {
        base.Awake(); // Call the base class method to set up logging
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Log("NPCController: No SpriteRenderer found!");
        }
        if (textBubble != null)
        {
            bubbleText = textBubble.GetComponentInChildren<Text>();
            if (bubbleText != null)
            {
                bubbleText.text = "..."; // Display initial three dots.
            }
        }

        if (OrderBubble != null)
        {
            OrderBubble.SetActive(false); // Hide the order bubble initially
        }
        else
        {
            Log("OrderBubble is not active.");
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (customerAnimator == null)
            customerAnimator = GetComponent<Animator>();

        if (customerAnimator == null || spriteRenderer == null || customerData == null ||
            CustomerData == null || orderBubble == null || OrderBubble == null || patienceBar == null)
        {
            Debug.LogError("CustomerController has missing required animation, order, patience or data references.", this);
            enabled = false;
            return;
        }
        // Keep the existing controller if no visual variants are configured.
        // 2) stash the base controller that's currently assigned
        originalController = customerAnimator.runtimeAnimatorController;

        // 3) randomly pick one of the two
        if (overrideControllers != null && overrideControllers.Count > 0)
        {
            int t = Random.Range(0, overrideControllers.Count);
            if (overrideControllers[t] != null) customerAnimator.runtimeAnimatorController = overrideControllers[t];
        }

    }

    public void ForceTimeout()
    {
        // stop the existing patience coroutine (if any)
        if (progressRoutine != null)
            StopCoroutine(progressRoutine);

        // call your existing failure logic
        OrderFailed(1);
    }

    private void Start()
    {
        currentPatience = maxPatience;
        customerAnimator.SetBool("isWalking", true);
    }

    protected override void OnEnable()
    {
        base.OnEnable(); // Call the base class method to set up logging
        if (customerData == null || !enabled) return;
        OnDifficultyChanged(customerData.difficulty);
        OnPatienceLevelIncreased(customerData.PatienceLevel);
        customerData.OnDifficultyChanged += OnDifficultyChanged;
        customerData.OnPatienceLevel_Increased += OnPatienceLevelIncreased;
    }
    protected override void OnDisable()
    {
        base.OnDisable(); // Call the base class method to clean up logging
        if (customerData != null)
        {
            customerData.OnDifficultyChanged -= OnDifficultyChanged;
            customerData.OnPatienceLevel_Increased -= OnPatienceLevelIncreased;
        }
        StopAllCoroutines();
        progressRoutine = null;
        if (!isWalkingOffScreen && assignedOrderArea != null) assignedOrderArea.UpdateState(false);
    }
    protected override void UpdateLogStatus()
    {
        isDebugEnabled = logSettings != null && logSettings.CustomerControllerLogs;
    }
    private void OnDifficultyChanged(Difficulty difficulty)
    {
        // Update the speed based on the difficulty level
        Log($"CustomerController: Difficulty changed to {difficulty}");

        difficultyMultiplier = difficulty == Difficulty.Hard ? 2 : 1;

    }

    private void OnPatienceLevelIncreased(int newValue)
    {
        patienceLevelMultiplier = newValue;
    }

    public void SetOrderArea(OrderArea area)
    {
        assignedOrderArea = area;
        targetPosition = area.GetCoordinates();
    }



    void Update()
    {
        // — are we walking at all? —
        if ((!hasArrived && !isWalkingOffScreen) || isWalkingOffScreen)
        {
            Vector2 currentPosition = transform.position;
            Vector2 dest = !hasArrived && !isWalkingOffScreen
                ? targetPosition
                : offScreenTarget;
            Vector2 direction = (dest - currentPosition).normalized;

            // ① flip sprite based on x‐direction
            //    true  = face left,  false = face right
            spriteRenderer.flipX = (direction.x < 0);

            // ② actually move
            transform.position = Vector2.MoveTowards(currentPosition, dest, speed * Time.deltaTime);

            // ③ arrival checks
            if (!hasArrived && !isWalkingOffScreen &&
                Vector2.Distance(currentPosition, targetPosition) < 0.1f)
            {
                ArrivedAtCounter();
                hasArrived = true;
            }
            else if (isWalkingOffScreen &&
                     Vector2.Distance(currentPosition, offScreenTarget) < 0.1f)
            {
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator PatienceCountdown()
    {
        float totalSeconds = 50f + (patienceLevelMultiplier - 1) * 10f;

        float waitPerPoint = totalSeconds / maxPatience / difficultyMultiplier;

        var tick = new WaitForSeconds(Mathf.Max(0.01f, waitPerPoint));
        while (currentPatience > 0 && !isWalkingOffScreen)
        {
            // update the UI
            patienceBar.SetHealth(currentPatience);

            yield return tick;
            currentPatience--;
        }

        OrderFailed(1);
    }



    void SetOffScreenTarget()
    {
        Camera cam = Camera.main;
        if (cam == null) { offScreenTarget = (Vector2)transform.position + Vector2.right * 20; return; }
        Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);
        Vector3 targetViewportPos;

        // If the customer is in the left half, target a point off-screen to the left.
        if (viewportPos.x < 0.5f)
        {
            targetViewportPos = new Vector3(-0.1f, viewportPos.y, viewportPos.z);
        }
        // Otherwise, target a point off-screen to the right.
        else
        {
            targetViewportPos = new Vector3(1.1f, viewportPos.y, viewportPos.z);
        }

        // Convert the target viewport position back to world space.
        Vector3 worldTarget = cam.ViewportToWorldPoint(targetViewportPos);
        // Preserve current Z position.
        worldTarget.z = transform.position.z;
        offScreenTarget = worldTarget;
    }

    void ArrivedAtCounter()
    {
        if (textBubble != null) textBubble.SetActive(false);
        OrderBubble.SetActive(true);
        customerAnimator.SetBool("isWalking", false);
        // orderBubble.StartOrder(Random.Range(1, 4));
        OrderMultiplier = Random.Range(1, 3);
        orderBubble.StartOrder(OrderMultiplier);
        // orderBubble.StartOrder(1);
        currentPatience = maxPatience;
        // Start the fake progress count (0 to 100) over 10 seconds.\
        if (progressRoutine != null) StopCoroutine(progressRoutine);
        progressRoutine = StartCoroutine(PatienceCountdown());
    }
    public void OnCorrectDelivery()
    {
        if (isWalkingOffScreen || !isActiveAndEnabled) return;
        Log("CustomerController: Correct delivery! boosted patience.");


        float totalSeconds = 30f + (patienceLevelMultiplier - 1) * 10f;
        float pointsPerSecond = maxPatience / totalSeconds;

        float boostSeconds = 20f;
        int boostPoints = Mathf.RoundToInt(pointsPerSecond * boostSeconds);
        currentPatience = Mathf.Min(maxPatience, currentPatience + boostPoints);
        patienceBar.SetHealth(currentPatience);
    }


    public void OnWrongDelivery(string foodName)
    {
        Log($"CustomerController: Wrong delivery of {foodName}!");
        OrderFailed(2);
    }

    /// <summary>
    /// Called by OrderBubble when the player delivers the wrong item.
    /// Shows feedback, deducts a bit of patience, then reverts.
    /// </summary>

    public void OnAllOrdersFulfilled()
    {
        if (isWalkingOffScreen || !isActiveAndEnabled) return;
        isWalkingOffScreen = true;
        // 1) stop patience timer
        if (progressRoutine != null)
            StopCoroutine(progressRoutine);

        OrderCompleted();
    }

    void OrderFailed(int reason)
    {
        if (isWalkingOffScreen || !isActiveAndEnabled) return;
        isWalkingOffScreen = true;
        if (progressRoutine != null) StopCoroutine(progressRoutine);
        progressRoutine = null;
        if (reason == 1)
        {
            // customer ran out of patience 
            CustomerData.DeductScore(1);

            Log("Times up! Order failed.");
        }
        else if (reason == 2)
        {
            CustomerData.DeductScore(5);
            Log("Customer Received Wrong Order !!");
            // on wrong delivery 
        }
        // spriteRenderer.sprite = angrySprite;
        if (redexclaim != null) redexclaim.SetActive(true);
        PlayOrderFailed();

        OrderBubble.SetActive(false);
        SetOffScreenTarget();
        isWalkingOffScreen = true;
        if (assignedOrderArea != null) assignedOrderArea.UpdateState(false);
    }

    // Updated method to set the off-screen target based on viewport bounds.
    // debugging thing , remove this 

    void OrderCompleted()
    {
        // Determine customer mood based on patience level
        int patiencePercent = patienceBar.GetHealth();

        // Update sprite based on patience level
        if (patiencePercent > 75)
        {
            if (Wow != null) Wow.SetActive(true);
        }
        else if (patiencePercent < 35)
        {

        }
        else
        {
            // Middle range            
            if (Meh != null) Meh.SetActive(true);
            if (redexclaim != null) redexclaim.SetActive(false);
            if (Wow != null) Wow.SetActive(false);
        }

        PlayOrderSuccess(patiencePercent);

        CustomerData.Increment();

        Log("Order Completed!");
        // Calculate score based on patience percentage (1-10)
        int score = Mathf.Clamp(1 + Mathf.FloorToInt(patiencePercent * 9f / 100f), 1, 10);
        score *= OrderMultiplier;
        CustomerData.AddScore(score);

        if (CoinHandler.Instance != null) CoinHandler.Instance.SpawnCoins(score, transform.position);

        if (progressRoutine != null)
        {
            StopCoroutine(progressRoutine);
            progressRoutine = null;
        }

        OrderBubble.SetActive(false);

        SetOffScreenTarget();

        isWalkingOffScreen = true;



        if (assignedOrderArea != null)
        {
            assignedOrderArea.UpdateState(false);
        }
    }

    // helper functions
    public void PlayOrderSuccess(int patiencePercent)
    {
        if (orderCompletedSound != null)
        {
            audioSource.PlayOneShot(orderCompletedSound);
        }
    }

    public void PlayOrderFailed()
    {
        if (orderFailedSound != null)
        {
            audioSource.PlayOneShot(orderFailedSound);
        }
    }



    // debugging thing , remove this
    //void OnMouseDown()
    //{
    //    if (hasArrived)
    //    {
    //        OrderCompleted();
    //    }
    //}

}
