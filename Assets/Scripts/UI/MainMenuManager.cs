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
    [Tooltip("Показывать курсор в главном меню")]
    public bool showCursorInMenu = true;
    
    [Tooltip("Разблокировать курсор в главном меню")]
    public bool unlockCursorInMenu = true;
    
    [Header("Debug")]
    [Tooltip("Показывать отладочную информацию")]
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
            Debug.Log($"MainMenuManager: Инициализация завершена. Курсор: Visible={Cursor.visible}, LockState={Cursor.lockState}");
        }
    }
    
    private void SetupMainMenuCursor()
    {
        if (showCursorInMenu)
        {
            Cursor.visible = true;
            if (showDebugInfo) Debug.Log("MainMenuManager: Курсор включен");
        }
        
        if (unlockCursorInMenu)
        {
            Cursor.lockState = CursorLockMode.None;
            if (showDebugInfo) Debug.Log("MainMenuManager: Курсор разблокирован");
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"MainMenuManager: Настройка курсора завершена - Visible: {Cursor.visible}, LockState: {Cursor.lockState}");
        }
    }
    
    public void StartNewGame()
    {
        if (showDebugInfo) Debug.Log("StartNewGame called, loading scene: " + gameSceneName);
        
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
            optionsPanel.SetActive(true);
    }
    
    public void CloseOptions()
    {
        if (showDebugInfo) Debug.Log("Closing options panel");
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
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
        SceneManager.LoadScene(1);
    }
    
    // Context menu methods for testing
    [ContextMenu("Setup Cursor")]
    public void TestSetupCursor()
    {
        SetupMainMenuCursor();
        Debug.Log($"MainMenuManager: Тест настройки курсора - Visible: {Cursor.visible}, LockState: {Cursor.lockState}");
    }
    
    [ContextMenu("Show Cursor Info")]
    public void ShowCursorInfo()
    {
        Debug.Log($"MainMenuManager: Текущее состояние курсора - Visible: {Cursor.visible}, LockState: {Cursor.lockState}");
    }
    
    void OnEnable()
    {
        // Ensure cursor is properly set when this object becomes active
        SetupMainMenuCursor();
    }
} 