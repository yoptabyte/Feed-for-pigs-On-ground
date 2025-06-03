using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InGameMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;
    
    [Header("Pause Menu Buttons")]
    public Button resumeButton;
    public Button settingsButton;
    public Button mainMenuButton;
    public Button quitButton;
    
    [Header("Settings")]
    public SettingsManager settingsManager;
    
    [Header("Scene Management")]
    public string mainMenuSceneName = "MainMenu";
    
    [Header("Auto-Find Components")]
    public bool autoFindComponents = true;
    
    [Header("Cursor Settings")]
    public bool hideCursorInGame = true;
    public CursorLockMode gameCursorLockMode = CursorLockMode.Locked;
    
    private bool isPaused = false;
    private bool settingsOpen = false;
    
    private void Start()
    {
        if (autoFindComponents)
        {
            AutoFindComponents();
        }
        
        ValidateComponents();
        SetupEventListeners();
        
        // Initialize game state
        ResumeGame();
    }
    
    private void Update()
    {
        // Check for Esc key press
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsOpen)
            {
                CloseSettings();
            }
            else if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
    
    private void AutoFindComponents()
    {
        Debug.Log("Auto-finding InGameMenu components...");
        
        // Find panels
        if (pauseMenuPanel == null)
        {
            pauseMenuPanel = FindChildByName("PauseMenuPanel");
            if (pauseMenuPanel != null) Debug.Log("Found PauseMenuPanel");
        }
        
        if (settingsPanel == null)
        {
            settingsPanel = FindChildByName("SettingsPanel");
            if (settingsPanel != null) Debug.Log("Found SettingsPanel");
        }
        
        // Find buttons
        if (resumeButton == null)
        {
            resumeButton = FindChildComponent<Button>("ResumeButton");
            if (resumeButton != null) Debug.Log("Found ResumeButton");
        }
        
        if (settingsButton == null)
        {
            settingsButton = FindChildComponent<Button>("SettingsButton");
            if (settingsButton != null) Debug.Log("Found SettingsButton");
        }
        
        if (mainMenuButton == null)
        {
            mainMenuButton = FindChildComponent<Button>("MainMenuButton");
            if (mainMenuButton != null) Debug.Log("Found MainMenuButton");
        }
        
        if (quitButton == null)
        {
            quitButton = FindChildComponent<Button>("QuitButton");
            if (quitButton != null) Debug.Log("Found QuitButton");
        }
        
        // Find settings manager
        if (settingsManager == null)
        {
            if (settingsPanel != null)
            {
                settingsManager = settingsPanel.GetComponent<SettingsManager>();
                if (settingsManager != null) Debug.Log("Found SettingsManager on SettingsPanel");
            }
            
            if (settingsManager == null)
            {
                settingsManager = FindObjectOfType<SettingsManager>();
                if (settingsManager != null) Debug.Log("Found SettingsManager in scene");
            }
        }
    }
    
    private GameObject FindChildByName(string childName)
    {
        Transform child = transform.Find(childName);
        if (child == null)
        {
            child = FindChildRecursive(transform, childName);
        }
        
        return child != null ? child.gameObject : null;
    }
    
    private T FindChildComponent<T>(string childName) where T : Component
    {
        Transform child = transform.Find(childName);
        if (child == null)
        {
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
        Debug.Log("=== IN-GAME MENU VALIDATION ===");
        Debug.Log($"Pause Menu Panel: {(pauseMenuPanel != null ? "✓" : "✗")}");
        Debug.Log($"Settings Panel: {(settingsPanel != null ? "✓" : "✗")}");
        Debug.Log($"Resume Button: {(resumeButton != null ? "✓" : "✗")}");
        Debug.Log($"Settings Button: {(settingsButton != null ? "✓" : "✗")}");
        Debug.Log($"Main Menu Button: {(mainMenuButton != null ? "✓" : "✗")}");
        Debug.Log($"Quit Button: {(quitButton != null ? "✓" : "✗")}");
        Debug.Log($"Settings Manager: {(settingsManager != null ? "✓" : "✗")}");
        Debug.Log("=== END VALIDATION ===");
    }
    
    private void SetupEventListeners()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
            
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }
    
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
        
        // Show cursor when paused
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        Debug.Log("Game paused");
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        settingsOpen = false;
        Time.timeScale = 1f;
        
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
            
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        
        // Hide cursor when resumed (if configured)
        if (hideCursorInGame)
        {
            Cursor.visible = false;
            Cursor.lockState = gameCursorLockMode;
        }
        
        Debug.Log("Game resumed");
    }
    
    public void OpenSettings()
    {
        settingsOpen = true;
        
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
            
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            
            // Configure settings manager for in-game use
            if (settingsManager != null)
            {
                settingsManager.isInGameSettings = true;
            }
        }
        
        Debug.Log("Settings opened");
    }
    
    public void CloseSettings()
    {
        settingsOpen = false;
        
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
            
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
        
        Debug.Log("Settings closed");
    }
    
    public void ReturnToMainMenu()
    {
        Debug.Log("Returning to main menu...");
        
        // Resume time before changing scene
        Time.timeScale = 1f;
        
        // Load main menu scene
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError("Main menu scene name is not set!");
        }
    }
    
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    // Public methods for external access
    public bool IsPaused()
    {
        return isPaused;
    }
    
    public bool AreSettingsOpen()
    {
        return settingsOpen;
    }
    
    // Context menu methods for testing
    [ContextMenu("Test Pause")]
    public void TestPause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }
    
    [ContextMenu("Test Settings")]
    public void TestSettings()
    {
        if (settingsOpen)
            CloseSettings();
        else
            OpenSettings();
    }
    
    [ContextMenu("Find All Components")]
    public void FindAllComponents()
    {
        AutoFindComponents();
        ValidateComponents();
    }
} 