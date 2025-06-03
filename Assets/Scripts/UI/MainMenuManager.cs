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
    
    private void Start()
    {
        // Setup button listeners
        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(StartNewGame);
            Debug.Log("New Game button listener added");
        }
        else
        {
            Debug.LogError("New Game Button is not assigned!");
        }
            
        if (optionsButton != null)
        {
            optionsButton.onClick.AddListener(OpenOptions);
            Debug.Log("Options button listener added");
        }
        else
        {
            Debug.LogError("Options Button is not assigned!");
        }
            
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
            Debug.Log("Quit button listener added");
        }
        else
        {
            Debug.LogError("Quit Button is not assigned!");
        }
        
        // Make sure options panel is closed at start
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }
    
    public void StartNewGame()
    {
        Debug.Log("StartNewGame called, loading scene: " + gameSceneName);
        
        // Check if scene exists in build settings
        if (Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            Debug.Log("Scene found in build settings, loading...");
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("Scene '" + gameSceneName + "' not found in Build Settings! Please add it to File > Build Settings.");
            
            // Try loading by build index as fallback
            if (SceneManager.sceneCountInBuildSettings > 1)
            {
                Debug.Log("Attempting to load scene by index 1...");
                SceneManager.LoadScene(1);
            }
        }
    }
    
    public void OpenOptions()
    {
        Debug.Log("Opening options panel");
        if (optionsPanel != null)
            optionsPanel.SetActive(true);
    }
    
    public void CloseOptions()
    {
        Debug.Log("Closing options panel");
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }
    
    public void QuitGame()
    {
        Debug.Log("QuitGame called");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    // Alternative method to load game scene by index (for testing)
    public void StartNewGameByIndex()
    {
        Debug.Log("Loading game scene by index 1");
        SceneManager.LoadScene(1);
    }
} 