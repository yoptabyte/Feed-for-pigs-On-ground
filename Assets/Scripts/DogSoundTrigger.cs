using UnityEngine;

[System.Serializable]
public class DogSoundTrigger : MonoBehaviour
{
    [Header("Dog Sound Settings")]
    [SerializeField]
    private float detectionRadius = 8f;
    [SerializeField]
    private float soundCooldown = 2.5f; // Time between bark sounds
    [SerializeField]
    private float volume = 0.9f;
    [SerializeField]
    private bool aggressiveBarkingMode = true; // Bark more frequently when player is close
    [SerializeField]
    private float aggressiveRadius = 4f; // Closer range for aggressive barking
    [SerializeField]
    private Vector2 barkCooldownRange = new Vector2(1f, 4f); // Random bark timing
    
    [Header("Custom Audio")]
    [Tooltip("Leave empty to auto-load dog.mp3")]
    [SerializeField]
    private AudioClip customDogSound;
    
    // Private components
    private AudioSource audioSource;
    private GameObject player;
    private float lastSoundTime = 0f;
    private float currentCooldown;
    private bool playerInRange = false;
    private bool playerInAggressiveRange = false;
    
    void Awake()
    {
        SetupDogAudio();
        ResetCooldown();
    }
    
    void Update()
    {
        CheckPlayerProximity();
    }
    
    private void SetupDogAudio()
    {
        // Get or create AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Load dog sound
        AudioClip dogClip = customDogSound;
        if (dogClip == null)
        {
            dogClip = Resources.Load<AudioClip>("dog");
            if (dogClip == null)
            {
                dogClip = Resources.Load<AudioClip>("Assets/dog");
            }
        }
        
        // Configure AudioSource for dog
        audioSource.clip = dogClip;
        audioSource.volume = volume;
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = detectionRadius * 1.8f;
        
        if (dogClip != null)
        {
            Debug.Log($"🐶 DogSoundTrigger: Setup complete with dog bark sound");
        }
        else
        {
            Debug.LogWarning($"🐶 DogSoundTrigger: Could not find dog.mp3 sound file");
        }
    }
    
    private void CheckPlayerProximity()
    {
        // Find player if not cached
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("PlayerTag");
            }
        }
        
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        bool isPlayerInRange = distanceToPlayer <= detectionRadius;
        bool isPlayerInAggressiveRange = distanceToPlayer <= aggressiveRadius;
        
        // Update states
        if (isPlayerInRange && !playerInRange)
        {
            playerInRange = true;
            OnPlayerEnterRange();
        }
        else if (!isPlayerInRange && playerInRange)
        {
            playerInRange = false;
            OnPlayerExitRange();
        }
        
        // Update aggressive state
        if (isPlayerInAggressiveRange && !playerInAggressiveRange)
        {
            playerInAggressiveRange = true;
            OnPlayerEnterAggressiveRange();
        }
        else if (!isPlayerInAggressiveRange && playerInAggressiveRange)
        {
            playerInAggressiveRange = false;
            OnPlayerExitAggressiveRange();
        }
        
        // Play dog sound periodically while player is in range
        if (playerInRange && CanPlaySound())
        {
            PlayDogSound();
        }
    }
    
    private void OnPlayerEnterRange()
    {
        Debug.Log($"🐶 Dog: Player detected, starting to bark!");
        PlayDogSound();
    }
    
    private void OnPlayerExitRange()
    {
        Debug.Log($"🐶 Dog: Player left, calming down");
        playerInAggressiveRange = false;
    }
    
    private void OnPlayerEnterAggressiveRange()
    {
        if (aggressiveBarkingMode)
        {
            Debug.Log($"🐶 Dog: Player too close! Aggressive barking mode!");
            PlayDogSound(); // Immediate bark when too close
        }
    }
    
    private void OnPlayerExitAggressiveRange()
    {
        if (aggressiveBarkingMode)
        {
            Debug.Log($"🐶 Dog: Player moved away, less aggressive barking");
        }
    }
    
    private void PlayDogSound()
    {
        if (audioSource == null || audioSource.clip == null) return;
        
        audioSource.PlayOneShot(audioSource.clip);
        lastSoundTime = Time.time;
        ResetCooldown();
        
        string barkType = playerInAggressiveRange ? "AGGRESSIVE BARK!" : "bark";
        Debug.Log($"🐶 Dog: {barkType} (Next bark in {currentCooldown:F1}s)");
    }
    
    private void ResetCooldown()
    {
        if (playerInAggressiveRange && aggressiveBarkingMode)
        {
            // Faster barking when in aggressive mode
            currentCooldown = Random.Range(barkCooldownRange.x * 0.5f, barkCooldownRange.y * 0.5f);
        }
        else
        {
            // Normal barking timing
            currentCooldown = Random.Range(barkCooldownRange.x, barkCooldownRange.y);
        }
    }
    
    private bool CanPlaySound()
    {
        return Time.time - lastSoundTime >= currentCooldown;
    }
    
    // Public methods
    public void BarkNow()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
            Debug.Log($"🐶 Dog: Manual bark!");
        }
    }
    
    public void SetDetectionRadius(float newRadius)
    {
        detectionRadius = newRadius;
        if (audioSource != null)
        {
            audioSource.maxDistance = detectionRadius * 1.8f;
        }
    }
    
    public void SetAggressiveMode(bool enabled)
    {
        aggressiveBarkingMode = enabled;
        Debug.Log($"🐶 Dog: Aggressive mode {(enabled ? "enabled" : "disabled")}");
    }
    
    private void OnDrawGizmosSelected()
    {
        // Define custom brown color
        Color brownColor = new Color(0.6f, 0.4f, 0.2f, 1f); // Custom brown color
        
        // Draw dog detection radius
        Gizmos.color = brownColor;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Draw aggressive barking radius
        if (aggressiveBarkingMode)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, aggressiveRadius);
        }
        
        // Draw optimal hearing range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1f);
        
        // Draw a small dog icon (diamond shape)
        Gizmos.color = brownColor;
        Vector3 dogPos = transform.position + Vector3.up * 1.5f;
        Gizmos.DrawLine(dogPos + Vector3.forward * 0.3f, dogPos + Vector3.right * 0.3f);
        Gizmos.DrawLine(dogPos + Vector3.right * 0.3f, dogPos + Vector3.back * 0.3f);
        Gizmos.DrawLine(dogPos + Vector3.back * 0.3f, dogPos + Vector3.left * 0.3f);
        Gizmos.DrawLine(dogPos + Vector3.left * 0.3f, dogPos + Vector3.forward * 0.3f);
    }
    
    // Context menu for testing
    [ContextMenu("Test Dog Bark")]
    private void TestDogBark()
    {
        BarkNow();
    }
    
    [ContextMenu("Toggle Aggressive Mode")]
    private void ToggleAggressiveMode()
    {
        SetAggressiveMode(!aggressiveBarkingMode);
    }
    
    [ContextMenu("Reload Dog Audio")]
    private void ReloadDogAudio()
    {
        SetupDogAudio();
    }
} 