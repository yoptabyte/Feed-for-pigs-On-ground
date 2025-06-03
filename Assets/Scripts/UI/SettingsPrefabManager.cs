using UnityEngine;
using UnityEngine.UI;

public class SettingsPrefabManager : MonoBehaviour
{
    [Header("Settings Prefab")]
    public GameObject settingsPrefab;
    
    [Header("UI References")]
    public Canvas targetCanvas;
    
    private GameObject currentSettingsInstance;
    
    private void Start()
    {
        // Find canvas if not assigned
        if (targetCanvas == null)
            targetCanvas = FindObjectOfType<Canvas>();
    }
    
    public void ShowSettings(bool isInGameSettings = false)
    {
        if (settingsPrefab == null)
        {
            Debug.LogError("Settings prefab is not assigned!");
            return;
        }
        
        if (targetCanvas == null)
        {
            Debug.LogError("No canvas found!");
            return;
        }
        
        // Destroy existing instance if any
        if (currentSettingsInstance != null)
        {
            DestroyImmediate(currentSettingsInstance);
        }
        
        // Create new instance
        currentSettingsInstance = Instantiate(settingsPrefab, targetCanvas.transform);
        
        // Configure settings manager
        SettingsManager settingsManager = currentSettingsInstance.GetComponent<SettingsManager>();
        if (settingsManager != null)
        {
            settingsManager.isInGameSettings = isInGameSettings;
        }
        
        Debug.Log("Settings panel created and shown");
    }
    
    public void HideSettings()
    {
        if (currentSettingsInstance != null)
        {
            currentSettingsInstance.SetActive(false);
        }
    }
    
    public void DestroySettings()
    {
        if (currentSettingsInstance != null)
        {
            DestroyImmediate(currentSettingsInstance);
            currentSettingsInstance = null;
        }
    }
    
    private void OnDestroy()
    {
        // Clean up when this manager is destroyed
        DestroySettings();
    }
} 