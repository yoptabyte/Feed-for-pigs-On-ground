using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button newGameButton;
    public Button optionsButton;
    public Button quitButton;
    public GameObject optionsPanel;
    
    [Header("Scene Names")]
    public string gameSceneName = "New Scene1"; // Main game scene name
    
    [Header("Cursor Settings")]
    [Tooltip("Show cursor in main menu")]
    public bool showCursorInMenu = true;
    
    [Tooltip("Unlock cursor in main menu")]
    public bool unlockCursorInMenu = true;
    
    [Header("Debug")]
    [Tooltip("Show debug information")]
    public bool showDebugInfo = false;
    
    private void Start()
    {
        // Setup cursor for main menu
        SetupMainMenuCursor();
        
        // Setup button listeners
        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(StartNewGame);
            if (showDebugInfo) Debug.Log("New Game button listener added");
        }
        else
        {
            Debug.LogError("New Game Button is not assigned!");
        }
            
        if (optionsButton != null)
        {
            optionsButton.onClick.AddListener(OpenOptions);
            if (showDebugInfo) Debug.Log("Options button listener added");
        }
        else
        {
            Debug.LogError("Options Button is not assigned!");
        }
            
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
            if (showDebugInfo) Debug.Log("Quit button listener added");
        }
        else
        {
            Debug.LogError("Quit Button is not assigned!");
        }
        
        // Make sure options panel is closed at start
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
            
        if (showDebugInfo)
        {
            Debug.Log($"MainMenuManager: Initialization completed. Cursor: Visible={Cursor.visible}, LockState={Cursor.lockState}");
        }
    }
    
    private void SetupMainMenuCursor()
    {
        if (showCursorInMenu)
        {
            Cursor.visible = true;
            if (showDebugInfo) Debug.Log("MainMenuManager: Cursor enabled");
        }
        
        if (unlockCursorInMenu)
        {
            Cursor.lockState = CursorLockMode.None;
            if (showDebugInfo) Debug.Log("MainMenuManager: Cursor unlocked");
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"MainMenuManager: Cursor setup completed - Visible: {Cursor.visible}, LockState: {Cursor.lockState}");
        }
    }

    private void SetMainMenuButtonsActive(bool active)
    {
        if (newGameButton != null)
            newGameButton.gameObject.SetActive(active);
        if (optionsButton != null)
            optionsButton.gameObject.SetActive(active);
        if (quitButton != null)
            quitButton.gameObject.SetActive(active);
            
        if (showDebugInfo)
            Debug.Log($"Main menu buttons set to: {(active ? "visible" : "hidden")}");
    }
    
    public void StartNewGame()
    {
        if (showDebugInfo) Debug.Log("StartNewGame called, loading scene: " + gameSceneName);
        
        // Reset game statistics before starting new game
        if (GameStatsTracker.Instance != null)
        {
            GameStatsTracker.Instance.ResetStats();
            if (showDebugInfo) Debug.Log("Game statistics reset for new game");
        }
        
        // Check if scene exists in build settings
        if (Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            if (showDebugInfo) Debug.Log("Scene found in build settings, loading...");
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("Scene '" + gameSceneName + "' not found in Build Settings! Please add it to File > Build Settings.");
            
            // Try loading by build index as fallback
            if (SceneManager.sceneCountInBuildSettings > 1)
            {
                if (showDebugInfo) Debug.Log("Attempting to load scene by index 1...");
                SceneManager.LoadScene(1);
            }
        }
    }
    
    public void OpenOptions()
    {
        if (showDebugInfo) Debug.Log("Opening options panel");
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
            SetMainMenuButtonsActive(false); // Disable main menu buttons when options are open
        }
    }
    
    public void CloseOptions()
    {
        if (showDebugInfo) Debug.Log("Closing options panel");
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
            SetMainMenuButtonsActive(true); // Enable main menu buttons when options are closed
        }
    }
    
    public void QuitGame()
    {
        if (showDebugInfo) Debug.Log("QuitGame called");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    // Alternative method to load game scene by index (for testing)
    public void StartNewGameByIndex()
    {
        if (showDebugInfo) Debug.Log("Loading game scene by index 1");
        
        // Reset game statistics before starting new game
        if (GameStatsTracker.Instance != null)
        {
            GameStatsTracker.Instance.ResetStats();
            if (showDebugInfo) Debug.Log("Game statistics reset for new game (by index)");
        }
        
        SceneManager.LoadScene(1);
    }
    
    // Context menu methods for testing
    [ContextMenu("Setup Cursor")]
    public void TestSetupCursor()
    {
        SetupMainMenuCursor();
        Debug.Log($"MainMenuManager: Cursor setup test - Visible: {Cursor.visible}, LockState: {Cursor.lockState}");
    }
    
    [ContextMenu("Show Cursor Info")]
    public void ShowCursorInfo()
    {
        Debug.Log($"MainMenuManager: Current cursor state - Visible: {Cursor.visible}, LockState: {Cursor.lockState}");
    }
    
    void OnEnable()
    {
        // Ensure cursor is properly set when this object becomes active
        SetupMainMenuCursor();
    }
} 