using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class PushableEnemy : MonoBehaviour
{
    [Header("Push Settings")]
    public float pushForceMultiplier = 1.5f;
    public float minimumPushForce = 5f;
    public float maximumPushForce = 50f;
    public string playerTag = "Player";
    
    [Header("Recovery Settings")]
    public float stunDuration = 1f; // How long AI is interrupted after being pushed
    public float pushCooldown = 0.5f; // Minimum time between pushes
    
    [Header("Audio Settings")]
    public AudioClip touchSound; // Assign touch.mp3 in inspector
    
    private Rigidbody rb;
    private EnemyPigAI enemyAI;
    private AudioSource audioSource;
    private float lastPushTime = 0f;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        enemyAI = GetComponent<EnemyPigAI>();
        audioSource = GetComponent<AudioSource>();
        
        if (rb == null)
        {
            Debug.LogError("PushableEnemy requires a Rigidbody component!");
        }
        
        if (audioSource == null)
        {
            Debug.LogError("PushableEnemy requires an AudioSource component!");
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        HandlePushCollision(collision);
    }
    
    void OnCollisionStay(Collision collision)
    {
        // Handle continuous pushing for when player holds against the enemy
        HandlePushCollision(collision, true);
    }
    
    private void HandlePushCollision(Collision collision, bool isContinuous = false)
    {
        // Check if we're being hit by the player
        if (!collision.gameObject.CompareTag(playerTag))
            return;
            
        // Check cooldown to prevent spam pushing
        if (Time.time - lastPushTime < pushCooldown)
            return;
        
        // Play touch sound
        PlayTouchSound();
        
        // Get the player's rigidbody to calculate push force
        Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
        if (playerRb == null)
            return;
        
        // Calculate push direction (from player to enemy)
        Vector3 pushDirection = (transform.position - collision.transform.position).normalized;
        pushDirection.y = 0; // Keep push on horizontal plane
        
        // Calculate push force based on player's velocity
        float playerSpeed = playerRb.linearVelocity.magnitude;
        float pushForce = playerSpeed * pushForceMultiplier;
        
        // For continuous collision, reduce the force
        if (isContinuous)
        {
            pushForce *= 0.3f;
        }
        
        // Clamp push force to reasonable limits
        pushForce = Mathf.Clamp(pushForce, minimumPushForce, maximumPushForce);
        
        // Apply the push force
        Vector3 forceVector = pushDirection * pushForce;
        rb.AddForce(forceVector, ForceMode.Impulse);
        
        // Interrupt AI movement
        if (enemyAI != null)
        {
            enemyAI.InterruptMovement(stunDuration);
        }
        
        // Update last push time
        lastPushTime = Time.time;
        
        // Optional: Add some visual/audio feedback here
        OnPushed(pushForce, pushDirection);
    }
    
    // Method to play touch sound
    private void PlayTouchSound()
    {
        if (audioSource != null && touchSound != null)
        {
            audioSource.PlayOneShot(touchSound);
        }
    }
    
    // Virtual method for adding custom effects when pushed
    protected virtual void OnPushed(float pushForce, Vector3 pushDirection)
    {
        // Override this in derived classes for custom push effects
        // For example: particle effects, sounds, etc.
        
        Debug.Log($"Enemy pushed with force: {pushForce:F1}");
    }
    
    // Public method to push the enemy programmatically
    public void PushEnemy(Vector3 direction, float force)
    {
        direction.y = 0;
        direction = direction.normalized;
        
        force = Mathf.Clamp(force, minimumPushForce, maximumPushForce);
        Vector3 forceVector = direction * force;
        
        rb.AddForce(forceVector, ForceMode.Impulse);
        
        if (enemyAI != null)
        {
            enemyAI.InterruptMovement(stunDuration);
        }
        
        // Play sound when programmatically pushed
        PlayTouchSound();
        
        OnPushed(force, direction);
    }
    
    void OnDrawGizmos()
    {
        // Draw push detection area
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);
        
        // Show if recently pushed
        if (Application.isPlaying && Time.time - lastPushTime < stunDuration)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + Vector3.up, 0.5f);
        }
    }
} 