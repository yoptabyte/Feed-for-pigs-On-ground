using UnityEngine;

[System.Serializable]
public class CowSoundTrigger : MonoBehaviour
{
    [Header("Cow Sound Settings")]
    [SerializeField]
    private float detectionRadius = 12f;
    [SerializeField]
    private float soundCooldown = 4f; // Time between moo sounds
    [SerializeField]
    private float volume = 0.8f;
    [SerializeField]
    private bool randomizeSoundTiming = true;
    [SerializeField]
    private Vector2 randomCooldownRange = new Vector2(2f, 6f);
    
    [Header("Custom Audio")]
    [Tooltip("Leave empty to auto-load cow.mp3")]
    [SerializeField]
    private AudioClip customCowSound;
    
    // Private components
    private AudioSource audioSource;
    private GameObject player;
    private float lastSoundTime = 0f;
    private float currentCooldown;
    private bool playerInRange = false;
    
    void Awake()
    {
        SetupCowAudio();
        ResetCooldown();
    }
    
    void Update()
    {
        CheckPlayerProximity();
    }
    
    private void SetupCowAudio()
    {
        // Get or create AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Load cow sound
        AudioClip cowClip = customCowSound;
        if (cowClip == null)
        {
            cowClip = Resources.Load<AudioClip>("cow");
            if (cowClip == null)
            {
                cowClip = Resources.Load<AudioClip>("Assets/cow");
            }
        }
        
        // Configure AudioSource for cow
        audioSource.clip = cowClip;
        audioSource.volume = volume;
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 2f;
        audioSource.maxDistance = detectionRadius * 1.5f;
        
        if (cowClip != null)
        {
            Debug.Log($"🐄 CowSoundTrigger: Setup complete with cow sound");
        }
        else
        {
            Debug.LogWarning($"🐄 CowSoundTrigger: Could not find cow.mp3 sound file");
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
        
        // Player entered range
        if (isPlayerInRange && !playerInRange)
        {
            playerInRange = true;
            OnPlayerEnterRange();
        }
        // Player left range
        else if (!isPlayerInRange && playerInRange)
        {
            playerInRange = false;
            OnPlayerExitRange();
        }
        
        // Play cow sound periodically while player is in range
        if (playerInRange && CanPlaySound())
        {
            PlayCowSound();
        }
    }
    
    private void OnPlayerEnterRange()
    {
        Debug.Log($"🐄 Cow: Player entered range, starting to moo!");
        PlayCowSound();
    }
    
    private void OnPlayerExitRange()
    {
        Debug.Log($"🐄 Cow: Player left range, stopping moo sounds");
    }
    
    private void PlayCowSound()
    {
        if (audioSource == null || audioSource.clip == null) return;
        
        audioSource.PlayOneShot(audioSource.clip);
        lastSoundTime = Time.time;
        ResetCooldown();
        
        Debug.Log($"🐄 Cow: Moooo! (Next sound in {currentCooldown:F1}s)");
    }
    
    private void ResetCooldown()
    {
        if (randomizeSoundTiming)
        {
            currentCooldown = Random.Range(randomCooldownRange.x, randomCooldownRange.y);
        }
        else
        {
            currentCooldown = soundCooldown;
        }
    }
    
    private bool CanPlaySound()
    {
        return Time.time - lastSoundTime >= currentCooldown;
    }
    
    // Public methods
    public void MooNow()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
            Debug.Log($"🐄 Cow: Manual moo!");
        }
    }
    
    public void SetDetectionRadius(float newRadius)
    {
        detectionRadius = newRadius;
        if (audioSource != null)
        {
            audioSource.maxDistance = detectionRadius * 1.5f;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw cow detection radius
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Draw optimal hearing range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 2f);
        
        // Draw a small cow icon (square)
        Gizmos.color = Color.black;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 2, Vector3.one * 0.5f);
    }
    
    // Context menu for testing
    [ContextMenu("Test Cow Moo")]
    private void TestCowMoo()
    {
        MooNow();
    }
    
    [ContextMenu("Reload Cow Audio")]
    private void ReloadCowAudio()
    {
        SetupCowAudio();
    }
} 