using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameUIController : MonoBehaviour
{
    [Header("UI Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button continueButton;
    
    [Header("Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private bool autoFindButtons = true;
    
    void Start()
    {
        if (autoFindButtons)
        {
            AutoFindButtons();
        }
        
        SetupButtonListeners();
    }
    
    private void AutoFindButtons()
    {
        // Find restart button
        if (restartButton == null)
        {
            Button[] allButtons = FindObjectsOfType<Button>();
            foreach (Button button in allButtons)
            {
                string name = button.name.ToLower();
                if (name.Contains("restart") || name.Contains("restart") || name.Contains("retry"))
                {
                    restartButton = button;
                    Debug.Log($"GameUIController: Found Restart Button: {button.name}");
                    break;
                }
            }
        }
        
        // Find main menu button
        if (mainMenuButton == null)
        {
            Button[] allButtons = FindObjectsOfType<Button>();
            foreach (Button button in allButtons)
            {
                string name = button.name.ToLower();
                if (name.Contains("menu") || name.Contains("menu") || name.Contains("main"))
                {
                    mainMenuButton = button;
                    Debug.Log($"GameUIController: Found Main Menu Button: {button.name}");
                    break;
                }
            }
        }
        
        // Find continue button
        if (continueButton == null)
        {
            Button[] allButtons = FindObjectsOfType<Button>();
            foreach (Button button in allButtons)
            {
                string name = button.name.ToLower();
                if (name.Contains("continue") || name.Contains("continue") || name.Contains("next"))
                {
                    continueButton = button;
                    Debug.Log($"GameUIController: Found Continue Button: {button.name}");
                    break;
                }
            }
        }
    }
    
    private void SetupButtonListeners()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
        
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(ContinueGame);
        }
    }
    
    public void RestartGame()
    {
        Debug.Log("GameUIController: Restarting game...");
        
        // Reset game statistics before restarting
        if (GameStatsTracker.Instance != null)
        {
            GameStatsTracker.Instance.ResetStats();
            Debug.Log("GameUIController: Game statistics reset for restart");
        }
        
        // Reset time scale
        Time.timeScale = 1f;
        
        // Restart current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void GoToMainMenu()
    {
        Debug.Log("GameUIController: Going to main menu...");
        
        // Reset time scale
        Time.timeScale = 1f;
        
        // Load main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }
    
    public void ContinueGame()
    {
        Debug.Log("GameUIController: Continuing game...");
        
        // Reset time scale and continue
        Time.timeScale = 1f;
        
        // If finish line controller exists, restart it
        if (FinishLineController.Instance != null)
        {
            FinishLineController.Instance.RestartGame();
        }
        
        // Find and reset all finish line triggers
        FinishLineTrigger[] triggers = FindObjectsOfType<FinishLineTrigger>();
        foreach (FinishLineTrigger trigger in triggers)
        {
            trigger.ResetTrigger();
        }
    }
    
    // Method to show/hide specific buttons based on game state
    public void SetButtonsState(bool showRestart, bool showMainMenu, bool showContinue)
    {
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(showRestart);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.gameObject.SetActive(showMainMenu);
        }
        
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(showContinue);
        }
    }
    
    // Method to enable/disable buttons
    public void SetButtonsInteractable(bool interactable)
    {
        if (restartButton != null)
        {
            restartButton.interactable = interactable;
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.interactable = interactable;
        }
        
        if (continueButton != null)
        {
            continueButton.interactable = interactable;
        }
    }
} 