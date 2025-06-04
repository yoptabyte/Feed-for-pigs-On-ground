using UnityEngine;

public class InGameMenuSetupHelper : MonoBehaviour
{
    [Header("Setup Helper")]
    [SerializeField] private bool setupInGameMenu = false;
    
    [ContextMenu("Setup In-Game Menu with OptionsPanel")]
    public void SetupInGameMenu()
    {
        Debug.Log("Setting up In-Game Menu with OptionsPanel support...");
        
        // Find or create InGameMenuManager
        InGameMenuManager menuManager = FindObjectOfType<InGameMenuManager>();
        if (menuManager == null)
        {
            GameObject managerObj = new GameObject("InGameMenuManager");
            menuManager = managerObj.AddComponent<InGameMenuManager>();
            Debug.Log("Created InGameMenuManager");
        }
        
        // Find OptionsPanel in scene
        GameObject optionsPanel = FindOptionsPanel();
        if (optionsPanel != null)
        {
            menuManager.optionsPanel = optionsPanel;
            Debug.Log($"Assigned OptionsPanel: {optionsPanel.name}");
            
            // Make sure OptionsPanel is initially inactive
            optionsPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("OptionsPanel not found in scene! Make sure it exists in Canvas.");
        }
        
        // Enable auto-find components
        menuManager.autoFindComponents = true;
        
        // Call validation
        menuManager.FindAllComponents();
        
        Debug.Log("In-Game Menu setup complete! Press ESC in play mode to test.");
    }
    
    private GameObject FindOptionsPanel()
    {
        // Search for OptionsPanel in the entire scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "OptionsPanel")
            {
                return obj;
            }
        }
        return null;
    }
    
    [ContextMenu("Test ESC Key Functionality")]
    public void TestESCKey()
    {
        InGameMenuManager menuManager = FindObjectOfType<InGameMenuManager>();
        if (menuManager != null)
        {
            if (menuManager.IsPaused())
            {
                menuManager.ResumeGame();
                Debug.Log("Test: Game resumed");
            }
            else
            {
                menuManager.PauseGame();
                Debug.Log("Test: Game paused, OptionsPanel should be active");
            }
        }
        else
        {
            Debug.LogError("InGameMenuManager not found! Run 'Setup In-Game Menu with OptionsPanel' first.");
        }
    }
} 