using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoadDiagnostics : MonoBehaviour
{
    [Header("Debug Settings")]
    [Tooltip("Show detailed diagnostics")]
    public bool showDetailedDiagnostics = true;
    
    [Header("Scene Names")]
    [Tooltip("Main menu scene name")]
    public string mainMenuSceneName = "MainMenu";
    
    [Tooltip("Game scene name")]
    public string gameSceneName = "GameScene";
    
    void Start()
    {
        if (showDetailedDiagnostics)
        {
            PerformSceneDiagnostic();
        }
    }
    
    [ContextMenu("Full Scene Diagnostic")]
    public void PerformSceneDiagnostic()
    {
        Debug.Log("=== SCENE LOADING DIAGNOSTICS ===");
        
        // Check current scene
        Scene currentScene = SceneManager.GetActiveScene();
        Debug.Log($"[SCENE] Current scene: {currentScene.name}");
        Debug.Log($"[SCENE] Scene path: {currentScene.path}");
        Debug.Log($"[SCENE] Build Index: {currentScene.buildIndex}");
        
        // Check build settings
        Debug.Log($"[BUILD] Total scenes in Build Settings: {SceneManager.sceneCountInBuildSettings}");
        
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            Debug.Log($"[BUILD] Index {i}: {sceneName} (Path: {scenePath})");
        }
        
        // Test scene loading capabilities
        TestSceneLoadCapability(mainMenuSceneName);
        TestSceneLoadCapability(gameSceneName);
        
        // Check for UI conflicts
        CheckForUIConflicts();
        
        // Check AdvancedDeathController settings
        CheckDeathControllerSettings();
        
        Debug.Log("=== SCENE LOADING DIAGNOSTICS ===");
    }
    
    private void TestSceneLoadCapability(string sceneName)
    {
        Debug.Log($"[TEST] Checking scene loading capability: {sceneName}");
        
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError($"[TEST] Scene name is empty!");
            return;
        }
        
        bool canLoad = Application.CanStreamedLevelBeLoaded(sceneName);
        Debug.Log($"[TEST] - CanStreamedLevelBeLoaded: {canLoad}");
        
        // Try to find scene by build index
        bool foundInBuild = false;
        int buildIndex = -1;
        
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            
            if (sceneNameInBuild == sceneName)
            {
                foundInBuild = true;
                buildIndex = i;
                break;
            }
        }
        
        Debug.Log($"[TEST] - Found in Build Settings: {foundInBuild}");
        if (foundInBuild)
        {
            Debug.Log($"[TEST] - Build Index: {buildIndex}");
        }
        
        if (!canLoad && !foundInBuild)
        {
            Debug.LogError($"[TEST] PROBLEM: Scene '{sceneName}' cannot be loaded!");
            Debug.LogError($"[TEST] Please check if the scene is added to File > Build Settings");
        }
    }
    
    private void CheckForUIConflicts()
    {
        Debug.Log("[UI] Checking UI conflicts...");
        
        // Check for multiple Canvas objects
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        Debug.Log($"[UI] Found Canvas objects: {allCanvases.Length}");
        
        foreach (Canvas canvas in allCanvases)
        {
            Debug.Log($"[UI] Canvas: {canvas.name}, active: {canvas.gameObject.activeInHierarchy}, render mode: {canvas.renderMode}");
        }
        
        // Check for main menu buttons in current scene
        Button[] allButtons = FindObjectsOfType<Button>(true);
        Debug.Log($"[UI] Found buttons: {allButtons.Length}");
        
        foreach (Button button in allButtons)
        {
            string name = button.name.ToLower();
            if (name.Contains("mainmenu") || name.Contains("main_menu") || name.Contains("menu") || 
                name.Contains("main") || name.Contains("exit"))
            {
                Debug.Log($"[UI] Main menu button found: {button.name}, active: {button.gameObject.activeInHierarchy}");
                
                // Check button listeners
                int listenerCount = button.onClick.GetPersistentEventCount();
                Debug.Log($"[UI] - Listener count: {listenerCount}");
                
                for (int i = 0; i < listenerCount; i++)
                {
                    Object target = button.onClick.GetPersistentTarget(i);
                    string methodName = button.onClick.GetPersistentMethodName(i);
                    Debug.Log($"[UI] - Listener {i}: {target?.name}.{methodName}");
                }
            }
        }
        
        // Check for panels that might be interfering
        GameObject[] allGameObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allGameObjects)
        {
            string name = obj.name.ToLower();
            if (name.Contains("panel") || name.Contains("menu"))
            {
                Debug.Log($"[UI] Panel/Menu: {obj.name}, active: {obj.activeInHierarchy}");
            }
        }
    }
    
    private void CheckDeathControllerSettings()
    {
        Debug.Log("[DEATH] Checking AdvancedDeathController settings...");
        
        AdvancedDeathController deathController = FindObjectOfType<AdvancedDeathController>();
        if (deathController == null)
        {
            Debug.LogWarning("[DEATH] AdvancedDeathController not found!");
            return;
        }
        
        Debug.Log($"[DEATH] AdvancedDeathController found: {deathController.name}");
        Debug.Log($"[DEATH] - Main menu scene name: '{deathController.mainMenuSceneName}'");
        Debug.Log($"[DEATH] - Show main menu button: {deathController.showMainMenuButton}");
        Debug.Log($"[DEATH] - Create main menu button: {deathController.createMainMenuButton}");
        Debug.Log($"[DEATH] - Main menu button assigned: {deathController.mainMenuButton != null}");
        
        if (deathController.mainMenuButton != null)
        {
            Debug.Log($"[DEATH] - Button: {deathController.mainMenuButton.name}, active: {deathController.mainMenuButton.gameObject.activeInHierarchy}");
            
            // Check if button has correct listener
            int listenerCount = deathController.mainMenuButton.onClick.GetPersistentEventCount();
            Debug.Log($"[DEATH] - Button listener count: {listenerCount}");
            
            for (int i = 0; i < listenerCount; i++)
            {
                Object target = deathController.mainMenuButton.onClick.GetPersistentTarget(i);
                string methodName = deathController.mainMenuButton.onClick.GetPersistentMethodName(i);
                Debug.Log($"[DEATH] - Listener {i}: {target?.name}.{methodName}");
            }
        }
        
        // Test scene name validity
        TestSceneLoadCapability(deathController.mainMenuSceneName);
    }
    
    [ContextMenu("Test Load Main Menu")]
    public void TestLoadMainMenu()
    {
        Debug.Log($"[TEST] Trying to load main menu: {mainMenuSceneName}");
        
        try
        {
            SceneManager.LoadScene(mainMenuSceneName);
            Debug.Log("[TEST] Scene loading initiated successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TEST] Scene loading error: {e.Message}");
        }
    }
    
    [ContextMenu("Test Load Game Scene")]
    public void TestLoadGameScene()
    {
        Debug.Log($"[TEST] Trying to load game scene: {gameSceneName}");
        
        try
        {
            SceneManager.LoadScene(gameSceneName);
            Debug.Log("[TEST] Scene loading initiated successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TEST] Scene loading error: {e.Message}");
        }
    }
    
    [ContextMenu("Force Load Main Menu By Index")]
    public void ForceLoadMainMenuByIndex()
    {
        Debug.Log("[TEST] Force loading main menu by index 0");
        
        try
        {
            SceneManager.LoadScene(0);
            Debug.Log("[TEST] Force loading by index initiated");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TEST] Force loading by index error: {e.Message}");
        }
    }
    
    [ContextMenu("Check Scene Manager State")]
    public void CheckSceneManagerState()
    {
        Debug.Log("=== SCENE MANAGER STATE ===");
        
        Debug.Log($"Loaded scenes: {SceneManager.sceneCount}");
        
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            Debug.Log($"Scene {i}: {scene.name}, loaded: {scene.isLoaded}, active: {scene == SceneManager.GetActiveScene()}");
        }
        
        Debug.Log("=== END OF STATE ===");
    }
    
    [ContextMenu("Simulate Death Controller Button Click")]
    public void SimulateDeathControllerButtonClick()
    {
        Debug.Log("[SIMULATION] Simulating AdvancedDeathController main menu button click");
        
        AdvancedDeathController deathController = FindObjectOfType<AdvancedDeathController>();
        if (deathController != null)
        {
            deathController.LoadMainMenu();
            Debug.Log("[SIMULATION] LoadMainMenu() method called");
        }
        else
        {
            Debug.LogError("[SIMULATION] AdvancedDeathController not found!");
        }
    }
} 