using UnityEngine;

public class GraphicsSettingsDemo : MonoBehaviour
{
    [Header("Demo Settings")]
    public KeyCode toggleFullscreenKey = KeyCode.F11;
    public KeyCode cycleQualityKey = KeyCode.Q;
    public KeyCode cycleResolutionKey = KeyCode.R;
    public KeyCode logSettingsKey = KeyCode.L;
    
    private SettingsManager settingsManager;
    private int currentQualityIndex = 0;
    private int currentResolutionIndex = 0;
    
    private void Start()
    {
        settingsManager = FindObjectOfType<SettingsManager>();
        if (settingsManager == null)
        {
            Debug.LogWarning("SettingsManager not found! Graphics demo won't work.");
        }
        
        currentQualityIndex = QualitySettings.GetQualityLevel();
        
        Debug.Log("=== GRAPHICS SETTINGS DEMO ===");
        Debug.Log($"Press {toggleFullscreenKey} to toggle fullscreen");
        Debug.Log($"Press {cycleQualityKey} to cycle quality settings");
        Debug.Log($"Press {cycleResolutionKey} to cycle resolutions");
        Debug.Log($"Press {logSettingsKey} to log current settings");
        Debug.Log("===============================");
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(toggleFullscreenKey))
        {
            ToggleFullscreen();
        }
        
        if (Input.GetKeyDown(cycleQualityKey))
        {
            CycleQuality();
        }
        
        if (Input.GetKeyDown(cycleResolutionKey))
        {
            CycleResolution();
        }
        
        if (Input.GetKeyDown(logSettingsKey))
        {
            LogCurrentSettings();
        }
    }
    
    public void ToggleFullscreen()
    {
        bool newFullscreen = !Screen.fullScreen;
        Screen.fullScreen = newFullscreen;
        
        if (settingsManager != null && settingsManager.fullscreenToggle != null)
        {
            settingsManager.fullscreenToggle.isOn = newFullscreen;
        }
        
        Debug.Log($"Fullscreen toggled to: {newFullscreen}");
    }
    
    public void CycleQuality()
    {
        currentQualityIndex = (currentQualityIndex + 1) % QualitySettings.names.Length;
        QualitySettings.SetQualityLevel(currentQualityIndex);
        
        if (settingsManager != null && settingsManager.qualityDropdown != null)
        {
            settingsManager.qualityDropdown.value = currentQualityIndex;
        }
        
        Debug.Log($"Quality changed to: {QualitySettings.names[currentQualityIndex]} (Index: {currentQualityIndex})");
    }
    
    public void CycleResolution()
    {
        Resolution[] resolutions = Screen.resolutions;
        if (resolutions.Length > 0)
        {
            currentResolutionIndex = (currentResolutionIndex + 1) % resolutions.Length;
            Resolution newRes = resolutions[currentResolutionIndex];
            
            Screen.SetResolution(newRes.width, newRes.height, Screen.fullScreen);
            
            if (settingsManager != null && settingsManager.resolutionDropdown != null)
            {
                settingsManager.resolutionDropdown.value = currentResolutionIndex;
            }
            
            Debug.Log($"Resolution changed to: {newRes.width}x{newRes.height} @ {newRes.refreshRate}Hz");
        }
    }
    
    public void LogCurrentSettings()
    {
        Debug.Log("=== CURRENT GRAPHICS SETTINGS ===");
        Debug.Log($"Quality: {QualitySettings.names[QualitySettings.GetQualityLevel()]} (Level {QualitySettings.GetQualityLevel()})");
        Debug.Log($"Resolution: {Screen.currentResolution.width}x{Screen.currentResolution.height} @ {Screen.currentResolution.refreshRate}Hz");
        Debug.Log($"Fullscreen: {Screen.fullScreen}");
        Debug.Log($"VSync: {QualitySettings.vSyncCount}");
        Debug.Log($"Anti-Aliasing: {QualitySettings.antiAliasing}");
        Debug.Log($"Anisotropic Filtering: {QualitySettings.anisotropicFiltering}");
        Debug.Log($"Shadow Quality: {QualitySettings.shadows}");
        Debug.Log($"Shadow Resolution: {QualitySettings.shadowResolution}");
        Debug.Log($"Texture Quality: {QualitySettings.globalTextureMipmapLimit}");
        Debug.Log("==================================");
    }
    
    // Method to apply specific quality preset
    public void SetQualityPreset(string presetName)
    {
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            if (QualitySettings.names[i].ToLower().Contains(presetName.ToLower()))
            {
                QualitySettings.SetQualityLevel(i);
                currentQualityIndex = i;
                
                if (settingsManager != null && settingsManager.qualityDropdown != null)
                {
                    settingsManager.qualityDropdown.value = i;
                }
                
                Debug.Log($"Quality preset set to: {QualitySettings.names[i]}");
                return;
            }
        }
        
        Debug.LogWarning($"Quality preset '{presetName}' not found!");
    }
    
    // Method to set specific resolution
    public void SetResolution(int width, int height, bool fullscreen = true)
    {
        Screen.SetResolution(width, height, fullscreen);
        
        // Update dropdown if available
        if (settingsManager != null && settingsManager.resolutionDropdown != null)
        {
            Resolution[] resolutions = Screen.resolutions;
            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i].width == width && resolutions[i].height == height)
                {
                    settingsManager.resolutionDropdown.value = i;
                    currentResolutionIndex = i;
                    break;
                }
            }
        }
        
        Debug.Log($"Resolution set to: {width}x{height} (Fullscreen: {fullscreen})");
    }
} 