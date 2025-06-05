using UnityEngine;

/// <summary>
/// Game Initializer - ensures all managers are available in any scene
/// Add this to any scene where you need audio/graphics settings to work
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [Header("Auto Initialize")]
    [Tooltip("Automatically initialize AudioManager")]
    public bool initializeAudioManager = true;
    
    [Tooltip("Automatically initialize GraphicsSettingsManager")]
    public bool initializeGraphicsManager = true;
    
    [Header("Debug")]
    [Tooltip("Show debug messages")]
    public bool showDebugInfo = true;
    
    private void Awake()
    {
        if (showDebugInfo)
        {
            Debug.Log("🚀 GameInitializer: Starting initialization...");
        }
        
        if (initializeAudioManager)
        {
            AudioManager.Initialize();
            if (showDebugInfo)
            {
                Debug.Log("🎵 GameInitializer: AudioManager initialized");
            }
        }
        
        if (initializeGraphicsManager)
        {
            // Create GraphicsSettingsManager if it doesn't exist
            GraphicsSettingsManager graphics = GraphicsSettingsManager.Instance;
            if (showDebugInfo)
            {
                Debug.Log("🎮 GameInitializer: GraphicsSettingsManager initialized");
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log("🚀 GameInitializer: Initialization complete!");
        }
    }
    
    [ContextMenu("Force Initialize All")]
    public void ForceInitializeAll()
    {
        AudioManager.Initialize();
        GraphicsSettingsManager graphics = GraphicsSettingsManager.Instance;
        Debug.Log("🚀 GameInitializer: Force initialized all managers");
    }
    
    [ContextMenu("Test Audio Settings")]
    public void TestAudioSettings()
    {
        AudioManager audio = AudioManager.Instance;
        audio.LogAudioStats();
    }
} 