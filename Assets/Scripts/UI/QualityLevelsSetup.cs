#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class QualityLevelsSetup : MonoBehaviour
{
    [ContextMenu("Setup Quality Levels")]
    public void SetupQualityLevels()
    {
        Debug.Log("Setting up quality levels...");
        
        // Get current quality settings
        string[] currentNames = QualitySettings.names;
        Debug.Log($"Current quality levels: {currentNames.Length}");
        
        if (currentNames.Length < 3)
        {
            Debug.LogWarning("Less than 3 quality levels found. You need to manually add them in Project Settings.");
            Debug.LogWarning("Go to Edit > Project Settings > Quality and add more quality levels.");
            ShowQualitySettingsInstructions();
        }
        else
        {
            Debug.Log("Quality levels are already properly configured:");
            for (int i = 0; i < currentNames.Length; i++)
            {
                Debug.Log($"  Level {i}: {currentNames[i]}");
            }
        }
    }
    
    private void ShowQualitySettingsInstructions()
    {
        Debug.Log("=== HOW TO ADD QUALITY LEVELS ===");
        Debug.Log("1. Go to Edit > Project Settings");
        Debug.Log("2. Select 'Quality' in the left panel");
        Debug.Log("3. You should see quality levels like 'Very Low', 'Low', 'Medium', 'High', 'Very High', 'Ultra'");
        Debug.Log("4. If you only see one level, click the '+' button to add more");
        Debug.Log("5. Configure each level with different settings:");
        Debug.Log("   - Very Low: Shadows Off, Anti-Aliasing Off, VSync Off");
        Debug.Log("   - Low: Hard Shadows Only, Anti-Aliasing Off, VSync Off");
        Debug.Log("   - Medium: All Shadows, 2x Anti-Aliasing, VSync Off");
        Debug.Log("   - High: All Shadows, 4x Anti-Aliasing, VSync On");
        Debug.Log("   - Very High: All Shadows, 8x Anti-Aliasing, VSync On");
        Debug.Log("   - Ultra: Maximum settings");
        Debug.Log("================================");
    }
    
    [ContextMenu("Open Quality Settings")]
    public void OpenQualitySettings()
    {
        #if UNITY_EDITOR
        SettingsService.OpenProjectSettings("Project/Quality");
        Debug.Log("Opened Quality Settings window");
        #endif
    }
    
    [ContextMenu("Log Current Quality Info")]
    public void LogCurrentQualityInfo()
    {
        Debug.Log("=== CURRENT QUALITY INFORMATION ===");
        Debug.Log($"Available Quality Levels: {QualitySettings.names.Length}");
        
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            Debug.Log($"Level {i}: {QualitySettings.names[i]}");
        }
        
        Debug.Log($"Current Quality Level: {QualitySettings.GetQualityLevel()} ({QualitySettings.names[QualitySettings.GetQualityLevel()]})");
        Debug.Log($"Shadow Quality: {QualitySettings.shadows}");
        Debug.Log($"Shadow Resolution: {QualitySettings.shadowResolution}");
        Debug.Log($"Anti-Aliasing: {QualitySettings.antiAliasing}");
        Debug.Log($"VSync Count: {QualitySettings.vSyncCount}");
        Debug.Log($"Anisotropic Filtering: {QualitySettings.anisotropicFiltering}");
        Debug.Log("===================================");
    }
    
    [ContextMenu("Test Quality Changes")]
    public void TestQualityChanges()
    {
        Debug.Log("Testing quality level changes...");
        
        int originalLevel = QualitySettings.GetQualityLevel();
        
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            QualitySettings.SetQualityLevel(i);
            Debug.Log($"Set quality to level {i}: {QualitySettings.names[i]}");
            
            // Log some key settings for this level
            Debug.Log($"  - Shadows: {QualitySettings.shadows}");
            Debug.Log($"  - Anti-Aliasing: {QualitySettings.antiAliasing}");
            Debug.Log($"  - VSync: {QualitySettings.vSyncCount}");
        }
        
        // Restore original level
        QualitySettings.SetQualityLevel(originalLevel);
        Debug.Log($"Restored original quality level: {originalLevel}");
    }
}
#endif 