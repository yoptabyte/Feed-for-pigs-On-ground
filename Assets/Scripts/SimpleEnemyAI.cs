using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(WaypointSystem))]
public class SimpleEnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float rotationSpeed = 180f; // Degrees per second
    public float acceleration = 10f;
    public float brakingForce = 5f;
    
    [Header("Behavior")]
    public bool canJump = false;
    public float jumpForce = 8f;
    public float jumpChance = 0.05f;
    
    [Header("Push Recovery")]
    public float pushRecoveryTime = 1f;
    
    private Rigidbody rb;
    private WaypointSystem waypointSystem;
    private float pushRecoveryTimer = 0f;
    private bool isRecoveringFromPush = false;
    private Vector3 lastKnownDirection = Vector3.forward;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        waypointSystem = GetComponent<WaypointSystem>();
        rb.freezeRotation = true; // Prevent physics rotation
    }
    
    void Start()
    {
        Debug.Log($"Simple Enemy AI started - Speed: {moveSpeed}");
    }
    
    void Update()
    {
        HandlePushRecovery();
        HandleWaypointMovement();
        HandleRandomJumping();
    }
    
    void FixedUpdate()
    {
        // Apply movement in FixedUpdate for physics
        ApplyMovement();
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
            
            // Smooth rotation towards target
            Vector3 targetForward = targetDirection.normalized;
            Quaternion targetRotation = Quaternion.LookRotation(targetForward);
            
            float rotationStep = rotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationStep);
        }
    }
    
    private void ApplyMovement()
    {
        if (isRecoveringFromPush)
        {
            // Apply braking when recovering
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            Vector3 brakingForceVector = -horizontalVelocity.normalized * brakingForce * rb.mass;
            rb.AddForce(brakingForceVector, ForceMode.Force);
            return;
        }
        
        // Calculate desired velocity
        Vector3 targetVelocity = transform.forward * moveSpeed;
        Vector3 currentVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        
        // Calculate force needed to reach target velocity
        Vector3 velocityDifference = targetVelocity - currentVelocity;
        Vector3 accelerationForce = velocityDifference * acceleration * rb.mass;
        
        // Limit the force to prevent overshooting
        float maxForce = acceleration * rb.mass;
        if (accelerationForce.magnitude > maxForce)
        {
            accelerationForce = accelerationForce.normalized * maxForce;
        }
        
        // Apply the force
        rb.AddForce(accelerationForce, ForceMode.Force);
        
        // Apply some drag to prevent infinite acceleration
        Vector3 dragForce = -currentVelocity.normalized * currentVelocity.sqrMagnitude * 0.1f * rb.mass;
        rb.AddForce(dragForce, ForceMode.Force);
    }
    
    private void HandleRandomJumping()
    {
        if (canJump && !isRecoveringFromPush && Random.value < jumpChance * Time.deltaTime)
        {
            // Simple jump - directly apply upward force
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            Debug.Log("🐷 Simple jump!");
        }
    }
    
    // Method to interrupt AI movement (useful when being pushed)
    public void InterruptMovement(float duration = 2f)
    {
        isRecoveringFromPush = true;
        pushRecoveryTimer = duration;
    }
    
    // Public method for jump triggers
    public void TriggerJump(float force, Vector3 direction)
    {
        rb.AddForce(direction.normalized * force, ForceMode.Impulse);
        InterruptMovement(0.5f);
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
            
            // Draw velocity vector
            Gizmos.color = Color.yellow;
            if (rb != null)
            {
                Vector3 velocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                Gizmos.DrawLine(transform.position, transform.position + velocity);
            }
            
            // Draw current waypoint connection
            if (waypointSystem != null && waypointSystem.GetCurrentWaypoint() != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, waypointSystem.GetCurrentWaypoint().position);
            }
            
            // Speed indicator
            if (rb != null)
            {
                float speed = rb.linearVelocity.magnitude;
                Gizmos.color = speed > moveSpeed * 0.9f ? Color.green : Color.yellow;
                Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.3f);
            }
        }
    }
} 