using UnityEngine;

public class NPCController : MonoBehaviour
{
    public float speed = 3f;
    private Vector2 moveDirection = Vector2.right; // Default movement direction.
    private SpriteRenderer spriteRenderer;

    [Tooltip("Sprite scale at the closest spawn Y")]
    private readonly float minScale = 0.6f;
    private readonly float maxScale = 0.8f;
    private readonly float NormalScale = 3f;
    public Animator customerAnimator; // Animator for the customer

    public System.Collections.Generic.List<AnimatorOverrideController> overrideControllers;

    private RuntimeAnimatorController originalController;



    void Awake()
    {
        // Debug.Log($"[NPCController] {gameObject.name} initialized.");
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.Log("NPCController: No SpriteRenderer found!");
        }
        speed = Random.Range(.7f, 1.0f) * speed;
        if (customerAnimator == null)
            customerAnimator = GetComponent<Animator>();

        // 2) stash the base controller that's currently assigned
        originalController = customerAnimator.runtimeAnimatorController;

        int t = Random.Range(0, overrideControllers.Count);  // 50/50 chance
        customerAnimator.runtimeAnimatorController = overrideControllers[t];

    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
        // Flip sprite based on horizontal direction
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = moveDirection.x < 0;
        }
        else
        {
            Debug.Log("NPCController: No SpriteRenderer found, cannot flip sprite.");
        }
    }

    void Update()
    {
        // Move the NPC.
        transform.Translate(speed * Time.deltaTime * moveDirection);

        // Check if the NPC has moved off screen.
        Camera cam = Camera.main;
        Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);
        // If NPC goes far off-screen (x < -0.1 or > 1.1), destroy it.
        if (viewportPos.x < -0.1f || viewportPos.x > 1.1f)
        {
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// Initializes this NPC: sets movement, scaling & alignment.
    /// </summary>
    /// <param name="direction">+1 = right, -1 = left</param>
    /// <param name="spawnY">world Y coordinate it spawned at</param>
    /// <param name="minY">minimum Y in your spawn zone</param>
    /// <param name="maxY">maximum Y in your spawn zone</param>
    public void Initialize(
        float direction,
        float spawnY,
        float minY,
        float maxY
    )
    {
        // 1) Movement direction & sprite flip
        moveDirection = new Vector2(direction, 0f);
        spriteRenderer.flipX = direction < 0;

        // 2) Depth‐based scale
        float t = Mathf.InverseLerp(minY, maxY, spawnY);
        float s = Mathf.Lerp(maxScale * NormalScale, NormalScale * minScale, t);
        transform.localScale = Vector3.one * s;

        // 3) Align bottom of sprite to spawnY
        AlignBottomToY(spawnY);
        customerAnimator.SetBool("isWalking", true);
    }

    public void AlignBottomToY(float spawnY)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // Calculate the sprite's height in world units.
            float spriteHeight = sr.bounds.size.y;
            // The current bottom is at (transform.position.y - spriteHeight/2)
            float currentBottomY = transform.position.y - spriteHeight * 0.5f;
            // Calculate the offset needed so that current bottom aligns with spawnY.
            float offset = spawnY - currentBottomY;
            // Only modify the Y position
            transform.position = new Vector3(transform.position.x, transform.position.y + offset, transform.position.z);
        }
        else
        {
            Debug.Log("NPCController: No SpriteRenderer found, cannot adjust spawn Y.");
        }
    }


}