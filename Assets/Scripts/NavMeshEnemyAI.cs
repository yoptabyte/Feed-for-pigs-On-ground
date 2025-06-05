using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(WaypointSystem))]
[RequireComponent(typeof(Animator))]
public class NavMeshEnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float acceleration = 8f;
    public float angularSpeed = 120f;
    
    [Header("Push Settings")]
    public float pushForce = 10f;
    public float pushRecoveryTime = 2f;
    public float resumeNavigationDelay = 1f;
    public string playerTag = "Player";
    
    [Header("Behavior")]
    public bool canJump = false;
    public float jumpForce = 8f;
    public float jumpChance = 0.05f;
    
    [Header("Animation")]
    public float movementThreshold = 0.1f; // Minimum speed to consider as walking
    
    private NavMeshAgent navAgent;
    private Rigidbody rb;
    private WaypointSystem waypointSystem;
    private Animator animator;
    
    private bool isPushed = false;
    private float pushTimer = 0f;
    private Vector3 pushVelocity = Vector3.zero;
    private bool wasNavigating = false;
    private bool isWalking = false;
    
    void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        waypointSystem = GetComponent<WaypointSystem>();
        animator = GetComponent<Animator>();
        
        // Configure NavMeshAgent
        navAgent.speed = moveSpeed;
        navAgent.acceleration = acceleration;
        navAgent.angularSpeed = angularSpeed;
        navAgent.autoBraking = false; // Keep moving to waypoints without slowing down
        
        // Configure Rigidbody for push physics
        rb.isKinematic = false; // Allow physics interactions
        rb.freezeRotation = true; // Prevent tumbling
    }
    
    void Start()
    {
        SetNextWaypoint();
        Debug.Log($"NavMesh Enemy AI started - Speed: {moveSpeed}");
    }
    
    void Update()
    {
        HandlePushRecovery();
        HandleNavigation();
        RecoverFromPush();
        HandleRandomJumping();
        UpdateAnimation();
    }
    
    void FixedUpdate()
    {
        HandlePushPhysics();
    }
    
    private void UpdateAnimation()
    {
        if (animator == null) return;
        
        // Calculate current movement speed
        float currentSpeed = 0f;
        
        if (isPushed)
        {
            // Use rigidbody velocity when pushed
            currentSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
        }
        else if (navAgent.enabled)
        {
            // Use NavMeshAgent velocity when navigating
            currentSpeed = navAgent.velocity.magnitude;
        }
        
        // Determine if walking based on speed threshold
        bool shouldWalk = currentSpeed > movementThreshold;
        
        // Update animation only if state changed
        if (shouldWalk != isWalking)
        {
            isWalking = shouldWalk;
            animator.SetBool("isWalking", isWalking);
            
            Debug.Log($"🐷 Animation state changed: {(isWalking ? "Walking" : "Idle")} (Speed: {currentSpeed:F2})");
        }
    }
    
    private void HandlePushRecovery()
    {
        if (isPushed)
        {
            pushTimer -= Time.deltaTime;
            
            if (pushTimer <= 0f)
            {
                // Recovery completed
                isPushed = false;
                
                // Re-enable NavMeshAgent after a small delay
                Invoke(nameof(ResumeNavigation), resumeNavigationDelay);
            }
        }
    }
    
    private void HandleNavigation()
    {
        if (isPushed || !navAgent.enabled)
            return;
            
        // Check if we've reached the current waypoint or are very close to destination
        bool reachedDestination = !navAgent.pathPending && navAgent.remainingDistance < 1.5f;
        bool reachedWaypoint = waypointSystem.HasReachedCurrentWaypoint(transform.position);
        
        if (reachedDestination || reachedWaypoint)
        {
            // Move to next waypoint
            Transform nextWaypoint = waypointSystem.GetNextWaypoint();
            if (nextWaypoint != null)
            {
                navAgent.SetDestination(nextWaypoint.position);
                Debug.Log($"🐷 Moving to next waypoint: {nextWaypoint.name}");
            }
        }
        
        // Update NavMeshAgent speed
        navAgent.speed = moveSpeed;
        
        // Ensure we always have a destination
        if (!navAgent.hasPath || navAgent.destination == Vector3.zero)
        {
            SetNextWaypoint();
        }
    }
    
    private void SetNextWaypoint()
    {
        Transform targetWaypoint = waypointSystem.GetCurrentWaypoint();
        if (targetWaypoint != null && navAgent.enabled)
        {
            navAgent.SetDestination(targetWaypoint.position);
            Debug.Log($"🐷 Setting destination to: {targetWaypoint.name}");
        }
    }
    
    private void HandlePushPhysics()
    {
        if (isPushed)
        {
            // Apply push velocity with gradual decay
            rb.linearVelocity = new Vector3(
                pushVelocity.x,
                rb.linearVelocity.y, // Keep Y velocity for gravity/jumps
                pushVelocity.z
            );
            
            // Decay push velocity over time
            pushVelocity = Vector3.Lerp(pushVelocity, Vector3.zero, Time.fixedDeltaTime * 2f);
        }
    }
    
    private void HandleRandomJumping()
    {
        if (canJump && !isPushed && Random.value < jumpChance * Time.deltaTime)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            Debug.Log("🐷 NavMesh jump!");
        }
    }
    
    private void ResumeNavigation()
    {
        if (!isPushed)
        {
            navAgent.enabled = true;
            SetNextWaypoint();
        }
    }
    
    // Handle collision with player for pushing
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            HandlePlayerPush(collision);
        }
    }
    
    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            HandlePlayerPush(collision, true);
        }
    }
    
    private void HandlePlayerPush(Collision collision, bool isContinuous = false)
    {
        Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
        if (playerRb == null) return;
        
        // Calculate push direction
        Vector3 pushDirection = (transform.position - collision.transform.position).normalized;
        pushDirection.y = 0; // Keep on horizontal plane
        
        // Calculate push strength based on player velocity
        float playerSpeed = playerRb.linearVelocity.magnitude;
        float currentPushForce = pushForce;
        
        if (isContinuous)
        {
            currentPushForce *= 0.5f; // Reduce force for continuous pushing
        }
        
        // Apply push effect
        ApplyPush(pushDirection, currentPushForce + playerSpeed);
        
        Debug.Log($"🐷 NavMesh pig pushed! Force: {currentPushForce + playerSpeed:F1}");
    }
    
    public void ApplyPush(Vector3 direction, float force)
    {
        // Disable NavMeshAgent temporarily
        navAgent.enabled = false;
        
        // Set push state
        isPushed = true;
        pushTimer = pushRecoveryTime;
        
        // Set push velocity
        pushVelocity = direction.normalized * force;
        
        // Apply immediate impulse
        rb.AddForce(direction.normalized * force, ForceMode.Impulse);
    }
    
    // Public method for jump triggers
    public void TriggerJump(float force, Vector3 direction)
    {
        rb.AddForce(direction.normalized * force, ForceMode.Impulse);
        
        // Brief pause in navigation
        if (navAgent.enabled)
        {
            navAgent.enabled = false;
            Invoke(nameof(ResumeNavigation), 0.5f);
        }
    }
    
    // Public method to interrupt movement
    public void InterruptMovement(float duration = 2f)
    {
        ApplyPush(Vector3.zero, 0f); // Just pause without push
        pushTimer = duration;
    }
    
    private void RecoverFromPush()
    {
        if (isPushed && rb.linearVelocity.magnitude < 1f)
        {
            isPushed = false;
            
            // Re-enable NavMeshAgent
            if (!navAgent.enabled)
            {
                navAgent.enabled = true;
                navAgent.Warp(transform.position); // Warp to current position
            }
            
            // Continue to current or next waypoint (not previous one)
            Transform currentWaypoint = waypointSystem.GetCurrentWaypoint();
            if (currentWaypoint != null)
            {
                // Check if we're already close to current waypoint
                if (waypointSystem.HasReachedCurrentWaypoint(transform.position))
                {
                    // Move to next waypoint since we're close to current one
                    Transform nextWaypoint = waypointSystem.GetNextWaypoint();
                    if (nextWaypoint != null)
                    {
                        navAgent.SetDestination(nextWaypoint.position);
                        Debug.Log($"🐷 Recovered from push, moving to next waypoint: {nextWaypoint.name}");
                    }
                }
                else
                {
                    // Continue to current waypoint
                    navAgent.SetDestination(currentWaypoint.position);
                    Debug.Log($"🐷 Recovered from push, continuing to current waypoint: {currentWaypoint.name}");
                }
            }
        }
    }
    
    void OnDrawGizmos()
    {
        // Draw current destination
        if (navAgent != null && navAgent.hasPath)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(navAgent.destination, 1f);
            Gizmos.DrawLine(transform.position, navAgent.destination);
        }
        
        // Draw current waypoint system info
        if (waypointSystem != null)
        {
            Transform currentWaypoint = waypointSystem.GetCurrentWaypoint();
            if (currentWaypoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(currentWaypoint.position, 0.8f);
                
#if UNITY_EDITOR
                // Draw distance info
                float distance = Vector3.Distance(transform.position, currentWaypoint.position);
                Handles.Label(transform.position + Vector3.up * 2f, 
                    $"Dist: {distance:F1}\nPushed: {isPushed}\nSpeed: {navAgent.speed:F1}");
#endif
            }
        }
    }
} 