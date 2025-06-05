using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

/// <summary>
/// Global Audio Manager for controlling all audio in the game
/// Automatically finds and manages AudioSources, applies volume settings
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("Master volume (0-1)")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    
    [Tooltip("Music volume (0-1)")]
    [Range(0f, 1f)]
    public float musicVolume = 0.75f;
    
    [Tooltip("SFX volume (0-1)")]
    [Range(0f, 1f)]
    public float sfxVolume = 0.75f;
    
    [Header("Audio Mixer (Optional)")]
    [Tooltip("If assigned, will use AudioMixer for volume control")]
    public AudioMixer audioMixer;
    
    [Header("Auto Management")]
    [Tooltip("Automatically find and manage all AudioSources")]
    public bool autoManageAudioSources = true;
    
    [Tooltip("Update interval for finding new AudioSources (seconds)")]
    public float updateInterval = 1f;
    
    [Header("Audio Classification")]
    [Tooltip("Keywords to identify music clips")]
    public string[] musicKeywords = { "music", "theme", "canon", "three_little", "background" };
    
    [Tooltip("Keywords to identify SFX clips")]
    public string[] sfxKeywords = { "walk", "dog", "cow", "die", "touch", "saw", "car", "shot", "victory", "lose" };
    
    // Private members
    private static AudioManager instance;
    private List<AudioSource> musicSources = new List<AudioSource>();
    private List<AudioSource> sfxSources = new List<AudioSource>();
    private Dictionary<AudioSource, float> originalVolumes = new Dictionary<AudioSource, float>();
    private float lastUpdateTime;
    
    // Properties
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    instance = go.AddComponent<AudioManager>();
                    DontDestroyOnLoad(go);
                    Debug.Log("🎵 AudioManager: Created new instance automatically");
                }
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("🎵 AudioManager: Initialized as singleton");
            LoadSettings();
        }
        else if (instance != this)
        {
            Debug.Log("🎵 AudioManager: Destroying duplicate instance");
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (autoManageAudioSources)
        {
            RefreshAudioSources();
        }
    }
    
    private void Update()
    {
        if (autoManageAudioSources && Time.time - lastUpdateTime > updateInterval)
        {
            RefreshAudioSources();
            lastUpdateTime = Time.time;
        }
    }
    
    /// <summary>
    /// Load audio settings from PlayerPrefs
    /// </summary>
    public void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        
        Debug.Log($"🎵 AudioManager: Loaded settings - Master: {masterVolume}, Music: {musicVolume}, SFX: {sfxVolume}");
        ApplyAllSettings();
    }
    
    /// <summary>
    /// Save audio settings to PlayerPrefs
    /// </summary>
    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
        
        Debug.Log($"🎵 AudioManager: Saved settings");
    }
    
    /// <summary>
    /// Find and categorize all AudioSources in the scene
    /// </summary>
    public void RefreshAudioSources()
    {
        AudioSource[] allSources = FindObjectsOfType<AudioSource>();
        
        musicSources.Clear();
        sfxSources.Clear();
        
        foreach (AudioSource source in allSources)
        {
            if (source == null) continue;
            
            // Store original volume if not already stored
            if (!originalVolumes.ContainsKey(source))
            {
                originalVolumes[source] = source.volume;
            }
            
            // Classify audio source
            if (IsMusic(source))
            {
                if (!musicSources.Contains(source))
                {
                    musicSources.Add(source);
                }
            }
            else
            {
                if (!sfxSources.Contains(source))
                {
                    sfxSources.Add(source);
                }
            }
        }
        
        // Clean up destroyed sources
        musicSources.RemoveAll(source => source == null);
        sfxSources.RemoveAll(source => source == null);
        
        // Remove destroyed sources from original volumes
        List<AudioSource> keysToRemove = new List<AudioSource>();
        foreach (var kvp in originalVolumes)
        {
            if (kvp.Key == null)
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        foreach (var key in keysToRemove)
        {
            originalVolumes.Remove(key);
        }
        
        ApplyAllSettings();
        
        Debug.Log($"🎵 AudioManager: Found {musicSources.Count} music sources, {sfxSources.Count} SFX sources");
    }
    
    /// <summary>
    /// Determine if an AudioSource should be classified as music
    /// </summary>
    private bool IsMusic(AudioSource source)
    {
        if (source.clip == null) return false;
        
        string clipName = source.clip.name.ToLower();
        string objectName = source.gameObject.name.ToLower();
        
        // Check for music keywords in clip name
        foreach (string keyword in musicKeywords)
        {
            if (clipName.Contains(keyword.ToLower()))
            {
                return true;
            }
        }
        
        // Check for music keywords in object name
        foreach (string keyword in musicKeywords)
        {
            if (objectName.Contains(keyword.ToLower()))
            {
                return true;
            }
        }
        
        // Long looping audio is usually music
        if (source.loop && source.clip.length > 10f)
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Set master volume
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        
        if (audioMixer != null)
        {
            float dbValue = masterVolume > 0.0001f ? Mathf.Log10(masterVolume) * 20 : -80f;
            audioMixer.SetFloat("MasterVolume", dbValue);
        }
        else
        {
            AudioListener.volume = masterVolume;
        }
        
        Debug.Log($"🎵 AudioManager: Master volume set to {masterVolume}");
    }
    
    /// <summary>
    /// Set music volume
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        
        if (audioMixer != null)
        {
            float dbValue = musicVolume > 0.0001f ? Mathf.Log10(musicVolume) * 20 : -80f;
            audioMixer.SetFloat("MusicVolume", dbValue);
        }
        else
        {
            foreach (AudioSource source in musicSources)
            {
                if (source != null && originalVolumes.ContainsKey(source))
                {
                    source.volume = originalVolumes[source] * musicVolume;
                }
            }
        }
        
        Debug.Log($"🎵 AudioManager: Music volume set to {musicVolume} for {musicSources.Count} sources");
    }
    
    /// <summary>
    /// Set SFX volume
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        
        if (audioMixer != null)
        {
            float dbValue = sfxVolume > 0.0001f ? Mathf.Log10(sfxVolume) * 20 : -80f;
            audioMixer.SetFloat("SFXVolume", dbValue);
        }
        else
        {
            foreach (AudioSource source in sfxSources)
            {
                if (source != null && originalVolumes.ContainsKey(source))
                {
                    source.volume = originalVolumes[source] * sfxVolume;
                }
            }
        }
        
        Debug.Log($"🎵 AudioManager: SFX volume set to {sfxVolume} for {sfxSources.Count} sources");
    }
    
    /// <summary>
    /// Apply all current volume settings
    /// </summary>
    public void ApplyAllSettings()
    {
        SetMasterVolume(masterVolume);
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
    }
    
    /// <summary>
    /// Register a new AudioSource (useful for dynamically created sources)
    /// </summary>
    public void RegisterAudioSource(AudioSource source, bool isMusic = false)
    {
        if (source == null) return;
        
        // Store original volume
        if (!originalVolumes.ContainsKey(source))
        {
            originalVolumes[source] = source.volume;
        }
        
        // Add to appropriate list
        if (isMusic || IsMusic(source))
        {
            if (!musicSources.Contains(source))
            {
                musicSources.Add(source);
                SetMusicVolume(musicVolume); // Apply current music volume
            }
        }
        else
        {
            if (!sfxSources.Contains(source))
            {
                sfxSources.Add(source);
                SetSFXVolume(sfxVolume); // Apply current SFX volume
            }
        }
        
        Debug.Log($"🎵 AudioManager: Registered {(isMusic ? "music" : "SFX")} source: {source.gameObject.name}");
    }
    
    /// <summary>
    /// Unregister an AudioSource
    /// </summary>
    public void UnregisterAudioSource(AudioSource source)
    {
        if (source == null) return;
        
        musicSources.Remove(source);
        sfxSources.Remove(source);
        originalVolumes.Remove(source);
        
        Debug.Log($"🎵 AudioManager: Unregistered source: {source.gameObject.name}");
    }
    
    /// <summary>
    /// Get current audio statistics
    /// </summary>
    public void LogAudioStats()
    {
        Debug.Log("=== AUDIO MANAGER STATS ===");
        Debug.Log($"Master Volume: {masterVolume}");
        Debug.Log($"Music Volume: {musicVolume}");
        Debug.Log($"SFX Volume: {sfxVolume}");
        Debug.Log($"Music Sources: {musicSources.Count}");
        Debug.Log($"SFX Sources: {sfxSources.Count}");
        Debug.Log($"Audio Mixer: {(audioMixer != null ? "✓" : "✗")}");
        Debug.Log("=== END STATS ===");
    }
    
    /// <summary>
    /// Force initialization of AudioManager (call this from any script that needs audio)
    /// </summary>
    public static void Initialize()
    {
        // Just accessing Instance will create it if needed
        AudioManager manager = Instance;
        manager.RefreshAudioSources();
        Debug.Log("🎵 AudioManager: Force initialized");
    }
} 