using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DeathUIController : MonoBehaviour
{
    [Header("Death UI Settings")]
    [Tooltip("AllPigs UI image to show on death")]
    public GameObject allPigsImage;
    
    [Tooltip("Player/pig Health component")]
    public Health playerHealth;
    
    [Header("Audio Settings")]
    [Tooltip("AudioSource for playing death music")]
    public AudioSource deathAudioSource;
    
    [Tooltip("Audio clip to play on death")]
    public AudioClip deathMusicClip;
    
    [Tooltip("Death music volume")]
    [Range(0f, 1f)]
    public float deathMusicVolume = 0.8f;
    
    [Header("Auto Find Settings")]
    [Tooltip("Automatically find components on start")]
    public bool autoFindComponents = true;
    
    [Header("Debug")]
    [Tooltip("Show debug information")]
    public bool showDebugInfo = false;
    
    // Private variables
    private bool hasPlayedDeathMusic = false;
    private Image allPigsImageComponent;
    
    void Start()
    {
        if (autoFindComponents)
        {
            AutoFindComponents();
        }
        
        SetupDeathUI();
        SubscribeToDeathEvent();
    }
    
    private void AutoFindComponents()
    {
        Debug.Log("DeathUIController: Auto-finding components...");
        
        // Find AllPigs image
        if (allPigsImage == null)
        {
            allPigsImage = FindObjectByName("AllPigs");
            if (allPigsImage != null)
            {
                Debug.Log($"DeathUIController: Found AllPigs: {allPigsImage.name}");
            }
            else
            {
                Debug.LogWarning("DeathUIController: AllPigs image not found!");
            }
        }
        
        // Find player health
        if (playerHealth == null)
        {
            // Try to find by Player tag
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
            }
            
            // If not found, search all Health components
            if (playerHealth == null)
            {
                Health[] allHealthComponents = FindObjectsOfType<Health>();
                foreach (Health health in allHealthComponents)
                {
                    string name = health.gameObject.name.ToLower();
                    if (name.Contains("pig") || name.Contains("player"))
                    {
                        playerHealth = health;
                        Debug.Log($"DeathUIController: Found Health component: {health.gameObject.name}");
                        break;
                    }
                }
            }
        }
        
        // Find or create AudioSource
        if (deathAudioSource == null)
        {
            deathAudioSource = GetComponent<AudioSource>();
            if (deathAudioSource == null)
            {
                deathAudioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("DeathUIController: Created new AudioSource");
            }
        }
    }
    
    private GameObject FindObjectByName(string objectName)
    {
        // Search in all GameObjects
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == objectName)
            {
                return obj;
            }
        }
        return null;
    }
    
    private void SetupDeathUI()
    {
        // Setup AllPigs image
        if (allPigsImage != null)
        {
            allPigsImageComponent = allPigsImage.GetComponent<Image>();
            
            // Initially hide the death UI
            allPigsImage.SetActive(false);
            
            if (showDebugInfo)
            {
                Debug.Log("DeathUIController: AllPigs image set up and hidden");
            }
        }
        
        // Setup audio source
        if (deathAudioSource != null)
        {
            deathAudioSource.volume = deathMusicVolume;
            deathAudioSource.playOnAwake = false;
            deathAudioSource.loop = false;
            
            if (deathMusicClip != null)
            {
                deathAudioSource.clip = deathMusicClip;
            }
        }
    }
    
    private void SubscribeToDeathEvent()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath.AddListener(OnPlayerDeath);
            
            if (showDebugInfo)
            {
                Debug.Log($"DeathUIController: Subscribed to death event for {playerHealth.gameObject.name}");
            }
        }
        else
        {
            Debug.LogError("DeathUIController: Health component not found! Please assign it manually or enable autoFindComponents");
        }
    }
    
    private void OnPlayerDeath()
    {
        if (showDebugInfo)
        {
            Debug.Log("DeathUIController: Death event received!");
        }
        
        ShowDeathUI();
        PlayDeathMusic();
    }
    
    private void ShowDeathUI()
    {
        if (allPigsImage != null)
        {
            allPigsImage.SetActive(true);
            
            if (showDebugInfo)
            {
                Debug.Log("DeathUIController: Shown AllPigs image");
            }
        }
        else
        {
            Debug.LogError("DeathUIController: AllPigs image not assigned!");
        }
    }
    
    private void PlayDeathMusic()
    {
        if (deathAudioSource != null && deathMusicClip != null && !hasPlayedDeathMusic)
        {
            deathAudioSource.clip = deathMusicClip;
            deathAudioSource.volume = deathMusicVolume;
            deathAudioSource.Play();
            
            hasPlayedDeathMusic = true;
            
            if (showDebugInfo)
            {
                Debug.Log("DeathUIController: Death music played");
            }
        }
        else if (hasPlayedDeathMusic)
        {
            if (showDebugInfo)
            {
                Debug.Log("DeathUIController: Death music already played");
            }
        }
        else
        {
            Debug.LogWarning("DeathUIController: AudioSource or death music clip not assigned!");
        }
    }
    
    // Public methods for external control
    public void HideDeathUI()
    {
        if (allPigsImage != null)
        {
            allPigsImage.SetActive(false);
            Debug.Log("DeathUIController: AllPigs image hidden");
        }
    }
    
    public void ResetDeathMusic()
    {
        hasPlayedDeathMusic = false;
        Debug.Log("DeathUIController: Death music flag reset");
    }
    
    public void StopDeathMusic()
    {
        if (deathAudioSource != null && deathAudioSource.isPlaying)
        {
            deathAudioSource.Stop();
            Debug.Log("DeathUIController: Death music stopped");
        }
    }
    
    // Context menu methods for testing
    [ContextMenu("Test Death UI")]
    public void TestDeathUI()
    {
        Debug.Log("DeathUIController: Testing death UI...");
        OnPlayerDeath();
    }
    
    [ContextMenu("Reset Death State")]
    public void ResetDeathState()
    {
        HideDeathUI();
        ResetDeathMusic();
        StopDeathMusic();
        Debug.Log("DeathUIController: Death state reset");
    }
    
    [ContextMenu("Find Components")]
    public void ManualFindComponents()
    {
        AutoFindComponents();
        SetupDeathUI();
        SubscribeToDeathEvent();
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (playerHealth != null)
        {
            playerHealth.OnDeath.RemoveListener(OnPlayerDeath);
        }
    }
} 