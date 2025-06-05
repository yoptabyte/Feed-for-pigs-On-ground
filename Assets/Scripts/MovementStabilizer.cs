using UnityEngine;

[RequireComponent(typeof(MovementData))]
public class MovementStabilizer : MonoBehaviour
{
    [Header("Speed-based Adjustments")]
    public bool enableSpeedBasedDrag = true;
    public float highSpeedDragMultiplier = 2f; // Extra drag at high speeds
    public float speedThreshold = 5f; // Speed at which to start applying extra drag
    
    [Header("Rotation Stability")]
    public bool enableRotationDamping = true;
    public float rotationDampingAtSpeed = 0.5f; // How much to reduce rotation at high speeds
    
    [Header("Auto-adjustment")]
    public bool autoAdjustForSpeed = true;
    
    private MovementData movementData;
    private Rigidbody rb;
    private float originalDrag;
    private float originalRotationSpeed;
    
    void Awake()
    {
        movementData = GetComponent<MovementData>();
        rb = GetComponent<Rigidbody>();
        
        // Store original values
        originalDrag = movementData.drag;
        originalRotationSpeed = movementData.rotationSpeed;
    }
    
    void Update()
    {
        if (autoAdjustForSpeed)
        {
            AdjustForCurrentSpeed();
        }
    }
    
    private void AdjustForCurrentSpeed()
    {
        float currentSpeed = rb.linearVelocity.magnitude;
        
        // Adjust drag based on speed
        if (enableSpeedBasedDrag && currentSpeed > speedThreshold)
        {
            float speedRatio = (currentSpeed - speedThreshold) / speedThreshold;
            float extraDrag = speedRatio * highSpeedDragMultiplier;
            movementData.drag = originalDrag + extraDrag;
        }
        else
        {
            movementData.drag = originalDrag;
        }
        
        // Adjust rotation speed based on speed
        if (enableRotationDamping && currentSpeed > speedThreshold)
        {
            float speedRatio = Mathf.Clamp01((currentSpeed - speedThreshold) / speedThreshold);
            float dampingFactor = 1f - (speedRatio * rotationDampingAtSpeed);
            movementData.rotationSpeed = originalRotationSpeed * dampingFactor;
        }
        else
        {
            movementData.rotationSpeed = originalRotationSpeed;
        }
    }
    
    // Public method to manually set stability parameters
    public void SetStabilityParameters(float dragMultiplier, float rotationDamping)
    {
        highSpeedDragMultiplier = dragMultiplier;
        rotationDampingAtSpeed = rotationDamping;
    }
    
    // Reset to original values
    public void ResetToOriginal()
    {
        movementData.drag = originalDrag;
        movementData.rotationSpeed = originalRotationSpeed;
    }
    
    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && rb != null)
        {
            // Show current speed
            Vector3 position = transform.position + Vector3.up * 3f;
            
            // Speed indicator
            float currentSpeed = rb.linearVelocity.magnitude;
            Gizmos.color = currentSpeed > speedThreshold ? Color.red : Color.green;
            Gizmos.DrawWireSphere(position, 0.5f);
            
            // Velocity vector
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + rb.linearVelocity);
        }
    }
} 