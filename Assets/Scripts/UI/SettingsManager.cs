using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    [Header("Graphics Settings")]
    public Dropdown qualityDropdown;
    public Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    
    [Header("Audio Settings")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public AudioMixer audioMixer;
    
    [Header("UI Elements")]
    public Button applyButton;
    public Button resetButton;
    public Button closeButton;
    public Button mainMenuButton; // Main Menu button for settings panel
    
    [Header("Panel Management")]
    public bool isInGameSettings = false; // True if this is in-game settings panel
    
    [Header("Auto-Find Components")]
    public bool autoFindComponents = true;
    
    private Resolution[] resolutions;
    private int currentResolutionIndex = 0;
    
    private void Start()
    {
        if (autoFindComponents)
        {
            AutoFindUIComponents();
        }
        
        ValidateComponents();
        SetupResolutions();
        SetupQualitySettings();
        LoadSettings();
        SetupEventListeners();
    }
    
    private void AutoFindUIComponents()
    {
        Debug.Log("Auto-finding UI components...");
        
        // Find dropdowns by name
        if (qualityDropdown == null)
        {
            qualityDropdown = FindChildComponent<Dropdown>("QualityDropdown");
            if (qualityDropdown != null) Debug.Log("Found QualityDropdown");
        }
        
        if (resolutionDropdown == null)
        {
            resolutionDropdown = FindChildComponent<Dropdown>("ResolutionDropdown");
            if (resolutionDropdown != null) Debug.Log("Found ResolutionDropdown");
        }
        
        // Find toggle
        if (fullscreenToggle == null)
        {
            fullscreenToggle = FindChildComponent<Toggle>("FullscreenToggle");
            if (fullscreenToggle != null) Debug.Log("Found FullscreenToggle");
        }
        
        // Find sliders
        if (masterVolumeSlider == null)
        {
            masterVolumeSlider = FindChildComponent<Slider>("MasterVolumeSlider");
            if (masterVolumeSlider != null) Debug.Log("Found MasterVolumeSlider");
        }
        
        if (musicVolumeSlider == null)
        {
            musicVolumeSlider = FindChildComponent<Slider>("MusicVolumeSlider");
            if (musicVolumeSlider != null) Debug.Log("Found MusicVolumeSlider");
        }
        
        if (sfxVolumeSlider == null)
        {
            sfxVolumeSlider = FindChildComponent<Slider>("SFXVolumeSlider");
            if (sfxVolumeSlider != null) Debug.Log("Found SFXVolumeSlider");
        }
        
        // Find buttons
        if (applyButton == null)
        {
            applyButton = FindChildComponent<Button>("ApplyButton");
            if (applyButton != null) Debug.Log("Found ApplyButton");
        }
        
        if (resetButton == null)
        {
            resetButton = FindChildComponent<Button>("ResetButton");
            if (resetButton != null) Debug.Log("Found ResetButton");
        }
        
        if (closeButton == null)
        {
            closeButton = FindChildComponent<Button>("CloseButton");
            if (closeButton != null) Debug.Log("Found CloseButton");
        }
        
        if (mainMenuButton == null)
        {
            mainMenuButton = FindChildComponent<Button>("MainMenuButton");
            if (mainMenuButton != null) Debug.Log("Found MainMenuButton");
        }
    }
    
    private T FindChildComponent<T>(string childName) where T : Component
    {
        Transform child = transform.Find(childName);
        if (child == null)
        {
            // Try recursive search
            child = FindChildRecursive(transform, childName);
        }
        
        if (child != null)
        {
            T component = child.GetComponent<T>();
            if (component != null)
            {
                return component;
            }
            else
            {
                Debug.LogWarning($"Found GameObject '{childName}' but it doesn't have component {typeof(T).Name}");
            }
        }
        else
        {
            Debug.LogWarning($"Could not find child GameObject named '{childName}'");
        }
        
        return null;
    }
    
    private Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;
            
            Transform found = FindChildRecursive(child, childName);
            if (found != null)
                return found;
        }
        return null;
    }
    
    private void ValidateComponents()
    {
        Debug.Log("=== SETTINGS MANAGER VALIDATION ===");
        Debug.Log($"Quality Dropdown: {(qualityDropdown != null ? "✓" : "✗")}");
        Debug.Log($"Resolution Dropdown: {(resolutionDropdown != null ? "✓" : "✗")}");
        Debug.Log($"Fullscreen Toggle: {(fullscreenToggle != null ? "✓" : "✗")}");
        Debug.Log($"Master Volume Slider: {(masterVolumeSlider != null ? "✓" : "✗")}");
        Debug.Log($"Music Volume Slider: {(musicVolumeSlider != null ? "✓" : "✗")}");
        Debug.Log($"SFX Volume Slider: {(sfxVolumeSlider != null ? "✓" : "✗")}");
        Debug.Log($"Apply Button: {(applyButton != null ? "✓" : "✗")}");
        Debug.Log($"Reset Button: {(resetButton != null ? "✓" : "✗")}");
        Debug.Log($"Close Button: {(closeButton != null ? "✓" : "✗")}");
        Debug.Log($"Main Menu Button: {(mainMenuButton != null ? "✓" : "✗")}");
        Debug.Log($"Audio Mixer: {(audioMixer != null ? "✓" : "✗")}");
        Debug.Log("=== END VALIDATION ===");
    }
    
    private void SetupEventListeners()
    {
        // Setup button listeners
        if (applyButton != null)
            applyButton.onClick.AddListener(ApplySettings);
            
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetToDefaults);
            
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseSettings);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            
        // Setup UI listeners
        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }
    
    private void SetupResolutions()
    {
        resolutions = Screen.resolutions;
        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            
            List<string> options = new List<string>();
            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + " x " + resolutions[i].height;
                options.Add(option);
                
                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }
            
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
            
            Debug.Log($"Setup {options.Count} resolution options");
        }
        else
        {
            Debug.LogError("Resolution Dropdown is null! Cannot setup resolutions.");
        }
    }
    
    private void SetupQualitySettings()
    {
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            
            // Check the number of available quality levels
            if (QualitySettings.names.Length > 1)
            {
                qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
                qualityDropdown.value = QualitySettings.GetQualityLevel();
            }
            else
            {
                Debug.LogWarning("Only 1 quality level found! Consider adding more quality levels in Project Settings > Quality");
                Debug.LogWarning("Go to Edit > Project Settings > Quality to add more quality levels");
                
                // Create basic options if there are none
                List<string> fallbackOptions = new List<string> { "Low", "Medium", "High" };
                qualityDropdown.AddOptions(fallbackOptions);
                qualityDropdown.value = 0;
            }
            
            qualityDropdown.RefreshShownValue();
            
            Debug.Log($"Setup {QualitySettings.names.Length} quality options from Unity Quality Settings");
            
            // Output available quality levels
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                Debug.Log($"  Quality Level {i}: {QualitySettings.names[i]}");
            }
        }
        else
        {
            Debug.LogError("Quality Dropdown is null! Cannot setup quality settings.");
        }
    }
    
    private void LoadSettings()
    {
        // Load graphics settings
        int savedQuality = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        if (qualityDropdown != null)
        {
            qualityDropdown.value = savedQuality;
            QualitySettings.SetQualityLevel(savedQuality); // Apply the saved quality
        }
        
        // Load resolution settings
        int savedResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
        if (resolutionDropdown != null && savedResolutionIndex < resolutions.Length)
        {
            resolutionDropdown.value = savedResolutionIndex;
            currentResolutionIndex = savedResolutionIndex;
        }
            
        // Load fullscreen setting
        bool savedFullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = savedFullscreen;
            Screen.fullScreen = savedFullscreen; // Apply the saved fullscreen mode
        }
            
        // Load audio settings
        float savedMasterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = savedMasterVolume;
            OnMasterVolumeChanged(savedMasterVolume); // Apply the saved volume
        }
            
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = savedMusicVolume;
            OnMusicVolumeChanged(savedMusicVolume); // Apply the saved volume
        }
            
        float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = savedSFXVolume;
            OnSFXVolumeChanged(savedSFXVolume); // Apply the saved volume
        }
        
        Debug.Log($"Settings loaded - Quality: {savedQuality}, Resolution: {savedResolutionIndex}, Fullscreen: {savedFullscreen}");
    }
    
    public void OnQualityChanged(int qualityIndex)
    {
        // If we only have one quality level in Unity, but dropdown has more options
        if (QualitySettings.names.Length == 1 && qualityDropdown.options.Count > 1)
        {
            Debug.LogWarning($"Trying to set quality to index {qualityIndex}, but Unity only has {QualitySettings.names.Length} quality level(s)");
            Debug.LogWarning("Using fallback quality simulation. Add more quality levels in Project Settings > Quality for full functionality");
            
            // Simulate quality change through individual settings changes
            SimulateQualityChange(qualityIndex);
            return;
        }
        
        if (qualityIndex >= 0 && qualityIndex < QualitySettings.names.Length)
        {
            QualitySettings.SetQualityLevel(qualityIndex);
            Debug.Log($"Quality changed to: {QualitySettings.names[qualityIndex]} (Index: {qualityIndex})");
            
            // Optionally auto-save quality changes immediately
            PlayerPrefs.SetInt("QualityLevel", qualityIndex);
        }
        else
        {
            Debug.LogError($"Invalid quality index: {qualityIndex} (Available: {QualitySettings.names.Length})");
        }
    }
    
    // Simulate quality change when we have limited Unity settings
    private void SimulateQualityChange(int qualityIndex)
    {
        Debug.Log($"Simulating quality change to level {qualityIndex}");
        
        // Simulate quality change through individual settings changes
        switch (qualityIndex)
        {
            case 0: // Low Quality
                QualitySettings.shadowResolution = ShadowResolution.Low;
                QualitySettings.shadows = ShadowQuality.HardOnly;
                QualitySettings.antiAliasing = 0;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                QualitySettings.vSyncCount = 0;
                Debug.Log("Applied Low Quality settings");
                break;
                
            case 1: // Medium Quality
                QualitySettings.shadowResolution = ShadowResolution.Medium;
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.antiAliasing = 2;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
                QualitySettings.vSyncCount = 0;
                Debug.Log("Applied Medium Quality settings");
                break;
                
            case 2: // High Quality
                QualitySettings.shadowResolution = ShadowResolution.High;
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.antiAliasing = 4;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
                QualitySettings.vSyncCount = 1;
                Debug.Log("Applied High Quality settings");
                break;
                
            default:
                Debug.LogWarning($"Unknown quality index: {qualityIndex}");
                break;
        }
        
        // Save simulated quality level
        PlayerPrefs.SetInt("SimulatedQualityLevel", qualityIndex);
    }
    
    public void OnResolutionChanged(int resolutionIndex)
    {
        if (resolutions != null && resolutionIndex >= 0 && resolutionIndex < resolutions.Length)
        {
            Resolution resolution = resolutions[resolutionIndex];
            bool isFullscreen = fullscreenToggle != null ? fullscreenToggle.isOn : Screen.fullScreen;
            
            Screen.SetResolution(resolution.width, resolution.height, isFullscreen);
            currentResolutionIndex = resolutionIndex;
            
            Debug.Log($"Resolution changed to: {resolution.width}x{resolution.height} (Fullscreen: {isFullscreen})");
            
            // Optionally auto-save resolution changes immediately
            PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        }
        else
        {
            Debug.LogError($"Invalid resolution index: {resolutionIndex} (Available: {resolutions?.Length ?? 0})");
        }
    }
    
    public void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        Debug.Log($"Fullscreen changed to: {isFullscreen}");
        
        // If we have a current resolution, reapply it with the new fullscreen mode
        if (resolutions != null && currentResolutionIndex < resolutions.Length)
        {
            Resolution currentRes = resolutions[currentResolutionIndex];
            Screen.SetResolution(currentRes.width, currentRes.height, isFullscreen);
            Debug.Log($"Reapplied resolution: {currentRes.width}x{currentRes.height} with fullscreen: {isFullscreen}");
        }
        
        // Optionally auto-save fullscreen changes immediately
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }
    
    public void OnMasterVolumeChanged(float volume)
    {
        if (audioMixer != null)
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
    }
    
    public void OnMusicVolumeChanged(float volume)
    {
        if (audioMixer != null)
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
    }
    
    public void OnSFXVolumeChanged(float volume)
    {
        if (audioMixer != null)
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
    }
    
    public void ApplySettings()
    {
        // Apply and save all settings
        if (qualityDropdown != null)
        {
            int qualityLevel = qualityDropdown.value;
            QualitySettings.SetQualityLevel(qualityLevel);
            PlayerPrefs.SetInt("QualityLevel", qualityLevel);
            Debug.Log($"Applied Quality: {QualitySettings.names[qualityLevel]}");
        }
        
        if (resolutionDropdown != null && resolutions != null)
        {
            int resIndex = resolutionDropdown.value;
            if (resIndex < resolutions.Length)
            {
                Resolution res = resolutions[resIndex];
                bool fullscreen = fullscreenToggle != null ? fullscreenToggle.isOn : Screen.fullScreen;
                Screen.SetResolution(res.width, res.height, fullscreen);
                PlayerPrefs.SetInt("ResolutionIndex", resIndex);
                currentResolutionIndex = resIndex;
                Debug.Log($"Applied Resolution: {res.width}x{res.height}");
            }
        }
        
        if (fullscreenToggle != null)
        {
            bool fullscreen = fullscreenToggle.isOn;
            Screen.fullScreen = fullscreen;
            PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
            Debug.Log($"Applied Fullscreen: {fullscreen}");
        }
        
        // Save audio settings
        if (masterVolumeSlider != null)
        {
            PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
            OnMasterVolumeChanged(masterVolumeSlider.value);
        }
        
        if (musicVolumeSlider != null)
        {
            PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
            OnMusicVolumeChanged(musicVolumeSlider.value);
        }
        
        if (sfxVolumeSlider != null)
        {
            PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
            OnSFXVolumeChanged(sfxVolumeSlider.value);
        }
        
        PlayerPrefs.Save();
        
        Debug.Log("All settings applied and saved successfully!");
    }
    
    public void CloseSettings()
    {
        // Close settings panel
        gameObject.SetActive(false);
        
        // If this is in-game settings, also notify the in-game menu manager
        if (isInGameSettings)
        {
            InGameMenuManager inGameMenu = FindObjectOfType<InGameMenuManager>();
            if (inGameMenu != null)
            {
                inGameMenu.CloseSettings();
            }
        }
        else
        {
            // If this is main menu settings, notify main menu manager
            MainMenuManager mainMenu = FindObjectOfType<MainMenuManager>();
            if (mainMenu != null)
            {
                mainMenu.CloseOptions();
            }
        }
    }
    
    public void ReturnToMainMenu()
    {
        Debug.Log("ReturnToMainMenu called from SettingsManager");
        
        if (isInGameSettings)
        {
            // If we're in-game, use the InGameMenuManager to return to main menu
            InGameMenuManager inGameMenu = FindObjectOfType<InGameMenuManager>();
            if (inGameMenu != null)
            {
                inGameMenu.ReturnToMainMenu();
            }
            else
            {
                Debug.LogError("InGameMenuManager not found! Cannot return to main menu.");
            }
        }
        else
        {
            Debug.LogWarning("ReturnToMainMenu called from main menu settings - this shouldn't happen");
            // Close settings if we're in main menu
            CloseSettings();
        }
    }
    
    public void ResetToDefaults()
    {
        // Reset to default values
        if (qualityDropdown != null)
        {
            qualityDropdown.value = 2; // Medium quality
            OnQualityChanged(2);
        }
        
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = true;
            OnFullscreenChanged(true);
        }
        
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = 0.75f;
            OnMasterVolumeChanged(0.75f);
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = 0.75f;
            OnMusicVolumeChanged(0.75f);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = 0.75f;
            OnSFXVolumeChanged(0.75f);
        }
        
        Debug.Log("Settings reset to defaults");
    }
    
    // Manual component assignment methods for Inspector
    [ContextMenu("Find All Components")]
    public void FindAllComponents()
    {
        AutoFindUIComponents();
        ValidateComponents();
    }
    
    // Additional helper methods for graphics settings
    [ContextMenu("Apply Current Graphics Settings")]
    public void ApplyCurrentGraphicsSettings()
    {
        if (qualityDropdown != null)
            OnQualityChanged(qualityDropdown.value);
            
        if (resolutionDropdown != null)
            OnResolutionChanged(resolutionDropdown.value);
            
        if (fullscreenToggle != null)
            OnFullscreenChanged(fullscreenToggle.isOn);
            
        Debug.Log("Current graphics settings applied");
    }
    
    [ContextMenu("Log Current Settings")]
    public void LogCurrentSettings()
    {
        Debug.Log("=== CURRENT SETTINGS ===");
        Debug.Log($"Quality Level: {QualitySettings.GetQualityLevel()} ({QualitySettings.names[QualitySettings.GetQualityLevel()]})");
        Debug.Log($"Screen Resolution: {Screen.currentResolution.width}x{Screen.currentResolution.height}");
        Debug.Log($"Fullscreen: {Screen.fullScreen}");
        Debug.Log($"Available Resolutions: {Screen.resolutions.Length}");
        
        if (resolutions != null)
        {
            Debug.Log($"Loaded Resolutions: {resolutions.Length}");
            for (int i = 0; i < Mathf.Min(5, resolutions.Length); i++)
            {
                Debug.Log($"  [{i}] {resolutions[i].width}x{resolutions[i].height} @ {resolutions[i].refreshRate}Hz");
            }
        }
        Debug.Log("=== END SETTINGS ===");
    }
    
    // Method to refresh resolution list (useful if display setup changes)
    public void RefreshResolutions()
    {
        SetupResolutions();
        Debug.Log("Resolution list refreshed");
    }
} 