using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using System.Collections.Generic;

/// <summary>
/// Enhanced Graphics Settings Manager with URP support
/// Handles quality levels, resolution, and advanced graphics settings
/// </summary>
public class GraphicsSettingsManager : MonoBehaviour
{
    [Header("Quality Settings")]
    [Tooltip("Custom quality level names")]
    public string[] customQualityNames = { "Low", "Medium", "High", "Ultra" };
    
    [Header("URP Settings")]
    [Tooltip("URP Asset for Low quality")]
    public UniversalRenderPipelineAsset lowQualityURP;
    
    [Tooltip("URP Asset for Medium quality")]
    public UniversalRenderPipelineAsset mediumQualityURP;
    
    [Tooltip("URP Asset for High quality")]
    public UniversalRenderPipelineAsset highQualityURP;
    
    [Tooltip("URP Asset for Ultra quality")]
    public UniversalRenderPipelineAsset ultraQualityURP;
    
    [Header("Resolution Settings")]
    [Tooltip("Automatically detect available resolutions")]
    public bool autoDetectResolutions = true;
    
    [Tooltip("Custom resolution options (only if autoDetectResolutions is false)")]
    public Vector2Int[] customResolutions = {
        new Vector2Int(1280, 720),
        new Vector2Int(1920, 1080),
        new Vector2Int(2560, 1440),
        new Vector2Int(3840, 2160)
    };
    
    [Header("Advanced Graphics")]
    [Tooltip("Enable V-Sync by default")]
    public bool enableVSyncByDefault = true;
    
    [Tooltip("Target frame rate (0 = unlimited)")]
    public int targetFrameRate = 60;
    
    // Private members
    private static GraphicsSettingsManager instance;
    private Resolution[] availableResolutions;
    private UniversalRenderPipelineAsset[] urpAssets;
    
    // Properties
    public static GraphicsSettingsManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GraphicsSettingsManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GraphicsSettingsManager");
                    instance = go.AddComponent<GraphicsSettingsManager>();
                    DontDestroyOnLoad(go);
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
            InitializeGraphicsSettings();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        LoadGraphicsSettings();
    }
    
    /// <summary>
    /// Initialize graphics settings and detect capabilities
    /// </summary>
    private void InitializeGraphicsSettings()
    {
        // Setup URP assets array
        urpAssets = new UniversalRenderPipelineAsset[] {
            lowQualityURP,
            mediumQualityURP,
            highQualityURP,
            ultraQualityURP
        };
        
        // Auto-find URP assets if not assigned
        if (lowQualityURP == null || mediumQualityURP == null || highQualityURP == null)
        {
            AutoFindURPAssets();
        }
        
        // Setup resolutions
        if (autoDetectResolutions)
        {
            availableResolutions = Screen.resolutions;
        }
        else
        {
            // Create Resolution array from custom resolutions
            availableResolutions = new Resolution[customResolutions.Length];
            for (int i = 0; i < customResolutions.Length; i++)
            {
                availableResolutions[i] = new Resolution
                {
                    width = customResolutions[i].x,
                    height = customResolutions[i].y,
                    refreshRate = 60
                };
            }
        }
        
        // Set target frame rate
        Application.targetFrameRate = targetFrameRate;
        
        Debug.Log($"🎮 GraphicsSettingsManager: Initialized with {availableResolutions.Length} resolutions");
    }
    
    /// <summary>
    /// Automatically find URP assets in the project
    /// </summary>
    private void AutoFindURPAssets()
    {
        // Try to load URP assets from Resources
        if (lowQualityURP == null)
        {
            lowQualityURP = Resources.Load<UniversalRenderPipelineAsset>("Mobile_RPAsset");
            if (lowQualityURP == null)
                lowQualityURP = Resources.Load<UniversalRenderPipelineAsset>("Low_Quality_URP");
        }
        
        if (mediumQualityURP == null)
        {
            mediumQualityURP = Resources.Load<UniversalRenderPipelineAsset>("PC_RPAsset");
            if (mediumQualityURP == null)
                mediumQualityURP = Resources.Load<UniversalRenderPipelineAsset>("Medium_Quality_URP");
        }
        
        if (highQualityURP == null)
        {
            highQualityURP = Resources.Load<UniversalRenderPipelineAsset>("High_Quality_URP");
        }
        
        if (ultraQualityURP == null)
        {
            ultraQualityURP = Resources.Load<UniversalRenderPipelineAsset>("Ultra_Quality_URP");
        }
        
        // Update URP assets array
        urpAssets[0] = lowQualityURP;
        urpAssets[1] = mediumQualityURP;
        urpAssets[2] = highQualityURP;
        urpAssets[3] = ultraQualityURP;
        
        Debug.Log($"🎮 GraphicsSettingsManager: Auto-found URP assets - Low: {lowQualityURP != null}, Medium: {mediumQualityURP != null}, High: {highQualityURP != null}, Ultra: {ultraQualityURP != null}");
    }
    
    /// <summary>
    /// Set quality level with enhanced control
    /// </summary>
    public void SetQualityLevel(int qualityIndex)
    {
        qualityIndex = Mathf.Clamp(qualityIndex, 0, customQualityNames.Length - 1);
        
        // Apply Unity quality settings if available
        if (qualityIndex < QualitySettings.names.Length)
        {
            QualitySettings.SetQualityLevel(qualityIndex, true);
        }
        
        // Apply URP asset if available
        if (qualityIndex < urpAssets.Length && urpAssets[qualityIndex] != null)
        {
            GraphicsSettings.defaultRenderPipeline = urpAssets[qualityIndex];
            QualitySettings.renderPipeline = urpAssets[qualityIndex];
            Debug.Log($"🎮 Applied URP asset for quality level: {customQualityNames[qualityIndex]}");
        }
        
        // Apply custom quality settings
        ApplyCustomQualitySettings(qualityIndex);
        
        // Save setting
        PlayerPrefs.SetInt("QualityLevel", qualityIndex);
        
        Debug.Log($"🎮 Quality level set to: {customQualityNames[qualityIndex]} (Index: {qualityIndex})");
    }
    
    /// <summary>
    /// Apply custom quality settings based on level
    /// </summary>
    private void ApplyCustomQualitySettings(int qualityIndex)
    {
        switch (qualityIndex)
        {
            case 0: // Low Quality
                QualitySettings.shadowResolution = UnityEngine.ShadowResolution.Low;
                QualitySettings.shadows = UnityEngine.ShadowQuality.HardOnly;
                QualitySettings.antiAliasing = 0;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                QualitySettings.vSyncCount = 0;
                QualitySettings.lodBias = 0.7f;
                QualitySettings.maximumLODLevel = 2;
                break;
                
            case 1: // Medium Quality
                QualitySettings.shadowResolution = UnityEngine.ShadowResolution.Medium;
                QualitySettings.shadows = UnityEngine.ShadowQuality.All;
                QualitySettings.antiAliasing = 2;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
                QualitySettings.vSyncCount = enableVSyncByDefault ? 1 : 0;
                QualitySettings.lodBias = 1.0f;
                QualitySettings.maximumLODLevel = 1;
                break;
                
            case 2: // High Quality
                QualitySettings.shadowResolution = UnityEngine.ShadowResolution.High;
                QualitySettings.shadows = UnityEngine.ShadowQuality.All;
                QualitySettings.antiAliasing = 4;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
                QualitySettings.vSyncCount = enableVSyncByDefault ? 1 : 0;
                QualitySettings.lodBias = 1.5f;
                QualitySettings.maximumLODLevel = 0;
                break;
                
            case 3: // Ultra Quality
                QualitySettings.shadowResolution = UnityEngine.ShadowResolution.VeryHigh;
                QualitySettings.shadows = UnityEngine.ShadowQuality.All;
                QualitySettings.antiAliasing = 8;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
                QualitySettings.vSyncCount = enableVSyncByDefault ? 1 : 0;
                QualitySettings.lodBias = 2.0f;
                QualitySettings.maximumLODLevel = 0;
                break;
        }
        
        Debug.Log($"🎮 Applied custom settings for quality level: {customQualityNames[qualityIndex]}");
    }
    
    /// <summary>
    /// Set screen resolution
    /// </summary>
    public void SetResolution(int resolutionIndex, bool fullscreen = true)
    {
        if (resolutionIndex >= 0 && resolutionIndex < availableResolutions.Length)
        {
            Resolution resolution = availableResolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, fullscreen);
            
            PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
            PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
            
            Debug.Log($"🎮 Resolution set to: {resolution.width}x{resolution.height} (Fullscreen: {fullscreen})");
        }
        else
        {
            Debug.LogError($"🎮 Invalid resolution index: {resolutionIndex}");
        }
    }
    
    /// <summary>
    /// Set fullscreen mode
    /// </summary>
    public void SetFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
        PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
        
        Debug.Log($"🎮 Fullscreen set to: {fullscreen}");
    }
    
    /// <summary>
    /// Set V-Sync
    /// </summary>
    public void SetVSync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;
        PlayerPrefs.SetInt("VSync", enabled ? 1 : 0);
        
        Debug.Log($"🎮 V-Sync set to: {enabled}");
    }
    
    /// <summary>
    /// Set target frame rate
    /// </summary>
    public void SetTargetFrameRate(int frameRate)
    {
        Application.targetFrameRate = frameRate;
        targetFrameRate = frameRate;
        PlayerPrefs.SetInt("TargetFrameRate", frameRate);
        
        Debug.Log($"🎮 Target frame rate set to: {frameRate}");
    }
    
    /// <summary>
    /// Load graphics settings from PlayerPrefs
    /// </summary>
    public void LoadGraphicsSettings()
    {
        // Load quality level
        int savedQuality = PlayerPrefs.GetInt("QualityLevel", 1); // Default to Medium
        SetQualityLevel(savedQuality);
        
        // Load resolution
        int savedResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", GetCurrentResolutionIndex());
        bool savedFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        SetResolution(savedResolutionIndex, savedFullscreen);
        
        // Load V-Sync
        bool savedVSync = PlayerPrefs.GetInt("VSync", enableVSyncByDefault ? 1 : 0) == 1;
        SetVSync(savedVSync);
        
        // Load target frame rate
        int savedFrameRate = PlayerPrefs.GetInt("TargetFrameRate", targetFrameRate);
        SetTargetFrameRate(savedFrameRate);
        
        Debug.Log($"🎮 Graphics settings loaded");
    }
    
    /// <summary>
    /// Get current resolution index
    /// </summary>
    private int GetCurrentResolutionIndex()
    {
        for (int i = 0; i < availableResolutions.Length; i++)
        {
            if (availableResolutions[i].width == Screen.currentResolution.width &&
                availableResolutions[i].height == Screen.currentResolution.height)
            {
                return i;
            }
        }
        return 0; // Default to first resolution
    }
    
    /// <summary>
    /// Get available resolutions
    /// </summary>
    public Resolution[] GetAvailableResolutions()
    {
        return availableResolutions;
    }
    
    /// <summary>
    /// Get quality level names
    /// </summary>
    public string[] GetQualityNames()
    {
        return customQualityNames;
    }
    
    /// <summary>
    /// Get current quality level
    /// </summary>
    public int GetCurrentQualityLevel()
    {
        return PlayerPrefs.GetInt("QualityLevel", 1);
    }
    
    /// <summary>
    /// Reset to default settings
    /// </summary>
    public void ResetToDefaults()
    {
        SetQualityLevel(1); // Medium quality
        SetResolution(GetCurrentResolutionIndex(), true);
        SetVSync(enableVSyncByDefault);
        SetTargetFrameRate(60);
        
        Debug.Log($"🎮 Graphics settings reset to defaults");
    }
    
    /// <summary>
    /// Log current graphics settings
    /// </summary>
    public void LogCurrentSettings()
    {
        Debug.Log("=== GRAPHICS SETTINGS ===");
        Debug.Log($"Quality Level: {GetCurrentQualityLevel()} ({customQualityNames[GetCurrentQualityLevel()]})");
        Debug.Log($"Resolution: {Screen.currentResolution.width}x{Screen.currentResolution.height}");
        Debug.Log($"Fullscreen: {Screen.fullScreen}");
        Debug.Log($"V-Sync: {QualitySettings.vSyncCount > 0}");
        Debug.Log($"Target Frame Rate: {Application.targetFrameRate}");
        Debug.Log($"Current FPS: {1f / Time.deltaTime:F1}");
        Debug.Log("=== END GRAPHICS SETTINGS ===");
    }
} 