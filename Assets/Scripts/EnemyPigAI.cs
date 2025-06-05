using UnityEngine;

[RequireComponent(typeof(InputData))]
[RequireComponent(typeof(WaypointSystem))]
[RequireComponent(typeof(MovementData))]
[RequireComponent(typeof(GroundCheckData))]
[RequireComponent(typeof(Health))]
public class EnemyPigAI : MonoBehaviour
{
    [Header("AI Settings")]
    public float baseMoveSpeed = 1f; // Base speed, will be modified by surface effects
    public float baseRotationSpeed = 90f; // Base rotation speed, will be modified by surface effects
    public float maxAngleForFullSpeed = 30f; // Angle threshold for full speed movement
    
    [Header("Behavior Settings")]
    public bool canJump = false;
    public float jumpChance = 0.05f; // Chance per second to jump randomly
    
    [Header("Push Recovery")]
    public float pushRecoveryTime = 1f; // Time to pause after being pushed
    
    [Header("Death Settings")]
    public float deathDestroyDelay = 5f; // Time before destroying dead bot
    
    private InputData inputData;
    private WaypointSystem waypointSystem;
    private MovementData movementData;
    private GroundCheckData groundCheckData;
    private Health health;
    private float pushRecoveryTimer = 0f;
    private bool isRecoveringFromPush = false;
    private Vector3 lastKnownDirection = Vector3.forward;
    private bool isDead = false;
    
    // Current effective speeds (modified by surface effects)
    private float currentMoveSpeed;
    private float currentRotationSpeed;
    
    void Awake()
    {
        inputData = GetComponent<InputData>();
        waypointSystem = GetComponent<WaypointSystem>();
        movementData = GetComponent<MovementData>();
        groundCheckData = GetComponent<GroundCheckData>();
        health = GetComponent<Health>();
        
        // Subscribe to death event
        if (health != null)
        {
            health.OnDeath.AddListener(OnBotDeath);
        }
    }
    
    void Start()
    {
        Debug.Log($"Enemy Pig AI started with simplified movement");
    }
    
    void Update()
    {
        // Don't do anything if dead
        if (isDead) return;
        
        UpdateSpeedsBasedOnSurface();
        HandlePushRecovery();
        HandleWaypointMovement();
        HandleRandomJumping();
    }
    
    private void OnBotDeath()
    {
        Debug.Log($"🐷💀 Bot {name} died! Stopping all movement and scheduling destruction.");
        
        isDead = true;
        
        // Stop all movement immediately
        StopAllMovement();
        
        // Schedule destruction after delay
        Invoke(nameof(DestroyBot), deathDestroyDelay);
    }
    
    private void StopAllMovement()
    {
        // Stop input
        if (inputData != null)
        {
            inputData.moveInput = Vector3.zero;
            inputData.jumpInput = false;
        }
        
        // Stop rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true; // Prevent further physics interactions
        }
        
        // Disable waypoint system
        if (waypointSystem != null)
        {
            waypointSystem.enabled = false;
        }
        
        // Disable NavMesh if present
        UnityEngine.AI.NavMeshAgent navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.enabled = false;
        }
        
        Debug.Log($"🐷💀 Stopped all movement for {name}");
    }
    
    private void DestroyBot()
    {
        Debug.Log($"🐷💀 Destroying dead bot {name}");
        Destroy(gameObject);
    }
    
    private void UpdateSpeedsBasedOnSurface()
    {
        // Start with base speeds
        currentMoveSpeed = baseMoveSpeed;
        currentRotationSpeed = baseRotationSpeed;
        
        // Apply surface effects if grounded and on a surface
        if (groundCheckData.isGrounded && groundCheckData.CurrentSurfaceData != null)
        {
            currentMoveSpeed *= groundCheckData.CurrentSurfaceData.SpeedMultiplier;
            // Rotation speed can also be affected by surface (like ice making turning harder)
            currentRotationSpeed *= groundCheckData.CurrentSurfaceData.AccelerationMultiplier;
        }
        
        // Update MovementData for consistency with player movement system
        if (movementData != null)
        {
            movementData.moveSpeed = currentMoveSpeed;
            movementData.rotationSpeed = currentRotationSpeed;
        }
    }
    
    private void HandlePushRecovery()
    {
        if (isRecoveringFromPush)
        {
            pushRecoveryTimer -= Time.deltaTime;
            if (pushRecoveryTimer <= 0f)
            {
                isRecoveringFromPush = false;
            }
            else
            {
                inputData.moveInput = Vector3.zero;
                return;
            }
        }
    }
    
    private void HandleWaypointMovement()
    {
        // Skip movement if recovering from push
        if (isRecoveringFromPush)
            return;
        
        // Check if we've reached the current waypoint
        if (waypointSystem.HasReachedCurrentWaypoint(transform.position))
        {
            waypointSystem.GetNextWaypoint();
        }
        
        // Get direction to current waypoint
        Vector3 targetDirection = waypointSystem.GetDirectionToCurrentWaypoint(transform.position);
        
        if (targetDirection != Vector3.zero)
        {
            lastKnownDirection = targetDirection;
            
            // Calculate angle to target
            float angleToTarget = Vector3.SignedAngle(transform.forward, targetDirection, Vector3.up);
            
            // Smooth rotation towards target
            float maxRotationThisFrame = currentRotationSpeed * Time.deltaTime;
            float rotationThisFrame = Mathf.Clamp(angleToTarget, -maxRotationThisFrame, maxRotationThisFrame);
            
            // Apply rotation directly to transform
            transform.Rotate(0, rotationThisFrame, 0, Space.Self);
            
            // Calculate movement speed based on how aligned we are
            float alignmentFactor = 1f - (Mathf.Abs(angleToTarget) / maxAngleForFullSpeed);
            alignmentFactor = Mathf.Clamp01(alignmentFactor);
            
            // Use simple forward movement instead of complex input system
            float effectiveMoveSpeed = currentMoveSpeed * alignmentFactor;
            
            // Set movement input - only forward movement, no rotation input
            inputData.moveInput = new Vector3(0f, 0f, effectiveMoveSpeed);
        }
        else
        {
            // No valid direction, stop moving
            inputData.moveInput = Vector3.zero;
        }
    }
    
    private void HandleRandomJumping()
    {
        // Only do random jumps if not recovering from push
        if (canJump && !isRecoveringFromPush && Random.value < jumpChance * Time.deltaTime)
        {
            inputData.jumpInput = true;
            Debug.Log("🐷 Random jump!");
        }
    }
    
    // Method to interrupt AI movement (useful when being pushed or jumping)
    public void InterruptMovement(float duration = 2f)
    {
        Debug.Log($"🐷🚀 Interrupting movement for {name} for {duration} seconds");
        
        isRecoveringFromPush = true;
        pushRecoveryTimer = duration;
        
        // Stop input immediately
        if (inputData != null)
        {
            inputData.moveInput = Vector3.zero;
            inputData.jumpInput = false;
        }
        
        // Temporarily disable NavMesh if present
        UnityEngine.AI.NavMeshAgent navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null && navAgent.enabled)
        {
            navAgent.enabled = false;
            Debug.Log($"🐷🚀 Disabled NavMesh Agent for {name}");
            
            // Re-enable NavMesh after duration
            Invoke(nameof(ReEnableNavMesh), duration);
        }
        
        // Temporarily disable waypoint system
        if (waypointSystem != null && waypointSystem.enabled)
        {
            waypointSystem.enabled = false;
            Debug.Log($"🐷🚀 Disabled WaypointSystem for {name}");
            
            // Re-enable waypoint system after duration
            Invoke(nameof(ReEnableWaypointSystem), duration);
        }
    }
    
    private void ReEnableNavMesh()
    {
        if (isDead) return; // Don't re-enable if dead
        
        UnityEngine.AI.NavMeshAgent navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.enabled = true;
            Debug.Log($"🐷🚀 Re-enabled NavMesh Agent for {name}");
        }
    }
    
    private void ReEnableWaypointSystem()
    {
        if (isDead) return; // Don't re-enable if dead
        
        if (waypointSystem != null)
        {
            waypointSystem.enabled = true;
            Debug.Log($"🐷🚀 Re-enabled WaypointSystem for {name}");
        }
    }
    
    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            // Draw direction arrow
            Gizmos.color = isRecoveringFromPush ? Color.red : Color.green;
            Vector3 from = transform.position + Vector3.up * 0.5f;
            Vector3 to = from + lastKnownDirection * 2f;
            Gizmos.DrawLine(from, to);
            
            // Draw arrow head
            if (lastKnownDirection != Vector3.zero)
            {
                Vector3 right = Vector3.Cross(Vector3.up, lastKnownDirection) * 0.3f;
                Vector3 arrowTip = to;
                Gizmos.DrawLine(arrowTip, arrowTip - lastKnownDirection * 0.5f + right);
                Gizmos.DrawLine(arrowTip, arrowTip - lastKnownDirection * 0.5f - right);
            }
            
            // Draw recovery indicator
            if (isRecoveringFromPush)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f);
            }
            
            // Draw current waypoint connection
            if (waypointSystem != null && waypointSystem.GetCurrentWaypoint() != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, waypointSystem.GetCurrentWaypoint().position);
                
                // Draw angle indicator
                Vector3 targetDirection = waypointSystem.GetDirectionToCurrentWaypoint(transform.position);
                float angleToTarget = Vector3.SignedAngle(transform.forward, targetDirection, Vector3.up);
                
                // Color based on alignment
                if (Mathf.Abs(angleToTarget) < 10f)
                    Gizmos.color = Color.green;
                else if (Mathf.Abs(angleToTarget) < 45f)
                    Gizmos.color = Color.yellow;
                else
                    Gizmos.color = Color.red;
                    
                Gizmos.DrawWireSphere(transform.position + Vector3.up * 1.5f, 0.3f);
            }
        }
    }
} 