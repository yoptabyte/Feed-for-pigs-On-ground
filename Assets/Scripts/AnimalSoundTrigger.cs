using UnityEngine;

public class AnimalSoundTrigger : MonoBehaviour
{
    [Header("Animal Sound Settings")]
    [SerializeField]
    private AnimalType animalType = AnimalType.Cow;
    [SerializeField]
    private float detectionRadius = 10f;
    [SerializeField]
    private float soundCooldown = 3f; // Cooldown between sound plays
    [SerializeField]
    private float volume = 0.8f;
    [SerializeField]
    private bool loopSound = false;
    
    [Header("Custom Audio Clip")]
    [Tooltip("Leave empty to auto-load based on animal type")]
    [SerializeField]
    private AudioClip customAudioClip;
    
    // Private components
    private AudioSource audioSource;
    private GameObject player;
    private float lastSoundTime = 0f;
    private bool playerInRange = false;
    
    public enum AnimalType
    {
        Cow,
        Dog
    }
    
    void Awake()
    {
        SetupAudioSystem();
    }
    
    void Update()
    {
        CheckPlayerProximity();
    }
    
    private void SetupAudioSystem()
    {
        // Get or create AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Load appropriate sound clip
        AudioClip soundClip = customAudioClip;
        if (soundClip == null)
        {
            soundClip = LoadAnimalSound();
        }
        
        // Configure AudioSource
        audioSource.clip = soundClip;
        audioSource.volume = volume;
        audioSource.loop = loopSound;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = detectionRadius * 2f;
        
        Debug.Log($"🐄🐶 AnimalSoundTrigger: Setup complete for {animalType} with clip: {(soundClip != null ? soundClip.name : "None")}");
    }
    
    private AudioClip LoadAnimalSound()
    {
        string soundName = animalType.ToString().ToLower();
        AudioClip clip = Resources.Load<AudioClip>(soundName);
        
        if (clip == null)
        {
            clip = Resources.Load<AudioClip>($"Assets/{soundName}");
        }
        
        if (clip != null)
        {
            Debug.Log($"🐄🐶 AnimalSoundTrigger: Auto-loaded {soundName}.mp3");
        }
        else
        {
            Debug.LogWarning($"🐄🐶 AnimalSoundTrigger: Could not find {soundName}.mp3 in Resources folder");
        }
        
        return clip;
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
        
        // Continue playing sound while player is in range (if not looping)
        if (playerInRange && !loopSound && CanPlaySound())
        {
            PlayAnimalSound();
        }
    }
    
    private void OnPlayerEnterRange()
    {
        Debug.Log($"🐄🐶 {animalType}: Player entered range, playing sound");
        PlayAnimalSound();
    }
    
    private void OnPlayerExitRange()
    {
        Debug.Log($"🐄🐶 {animalType}: Player left range");
        
        if (loopSound && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    
    private void PlayAnimalSound()
    {
        if (audioSource == null || audioSource.clip == null) return;
        
        if (loopSound)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
                Debug.Log($"🐄🐶 {animalType}: Started looping sound");
            }
        }
        else
        {
            if (CanPlaySound())
            {
                audioSource.PlayOneShot(audioSource.clip);
                lastSoundTime = Time.time;
                Debug.Log($"🐄🐶 {animalType}: Played one-shot sound");
            }
        }
    }
    
    private bool CanPlaySound()
    {
        return Time.time - lastSoundTime >= soundCooldown;
    }
    
    // Public methods for external control
    public void SetAnimalType(AnimalType newType)
    {
        animalType = newType;
        SetupAudioSystem();
    }
    
    public void PlaySoundManually()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
            Debug.Log($"🐄🐶 {animalType}: Manual sound play");
        }
    }
    
    public void StopSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log($"🐄🐶 {animalType}: Sound stopped");
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Define custom brown color for dogs
        Color brownColor = new Color(0.6f, 0.4f, 0.2f, 1f); // Custom brown color
        
        // Draw detection radius in scene view
        Gizmos.color = animalType == AnimalType.Cow ? Color.white : brownColor;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Draw inner area where sound is loudest
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
    
    private void OnDestroy()
    {
        // Stop sound when object is destroyed
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    
    // Context menu for testing
    [ContextMenu("Test Animal Sound")]
    private void TestAnimalSound()
    {
        PlaySoundManually();
    }
    
    [ContextMenu("Reload Audio Clip")]
    private void ReloadAudioClip()
    {
        SetupAudioSystem();
    }
} 