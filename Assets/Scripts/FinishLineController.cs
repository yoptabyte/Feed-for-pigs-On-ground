using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FinishLineController : MonoBehaviour
{
    public static FinishLineController Instance { get; private set; }
    
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip playerWinSound;
    [SerializeField] private AudioClip enemyWinSound;
    
    [Header("UI Elements")]
    [SerializeField] private GameObject finishUI;
    [SerializeField] private Image resultImage;
    [SerializeField] private Sprite playerWinSprite;
    [SerializeField] private Sprite enemyWinSprite;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI resultText;
    
    [Header("Menu Buttons")]
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    
    [Header("Display Settings")]
    [SerializeField] private float statisticsDelay = 3f; // Delay before showing statistics
    
    [Header("Game Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string enemyTag = "Enemy";
    
    [Header("Time Tracking")]
    [SerializeField] private float gameStartTime;
    [SerializeField] private float gameFinishTime;
    [SerializeField] private bool gameFinished = false;
    
    // Game state
    private bool playerFinished = false;
    private bool enemyFinished = false;
    private bool gameActive = true;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Initialize audio source if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Hide finish UI at start
        if (finishUI != null)
        {
            finishUI.SetActive(false);
        }
        
        // Record game start time
        gameStartTime = Time.time;
    }
    
    void Start()
    {
        // Auto find UI components if not assigned
        AutoFindUIComponents();
        
        // Setup button listeners
        SetupButtons();
    }
    
    private void AutoFindUIComponents()
    {
        if (finishUI == null)
        {
            GameObject foundUI = GameObject.Find("FinishUI");
            if (foundUI == null)
            {
                foundUI = GameObject.Find("GameOverUI");
            }
            if (foundUI == null)
            {
                foundUI = GameObject.Find("ResultUI");
            }
            finishUI = foundUI;
        }
        
        // Auto find result image
        if (resultImage == null && finishUI != null)
        {
            Transform resultImageTransform = finishUI.transform.Find("ResultImage");
            if (resultImageTransform != null)
            {
                resultImage = resultImageTransform.GetComponent<Image>();
                if (resultImage != null)
                {
                    Debug.Log("FinishLineController: Found ResultImage component");
                }
            }
        }
        
        if (timeText == null)
        {
            TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>();
            foreach (var text in allTexts)
            {
                string name = text.name.ToLower();
                if (name.Contains("time") || name.Contains("time"))
                {
                    timeText = text;
                    break;
                }
            }
        }
        
        if (resultText == null)
        {
            TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>();
            foreach (var text in allTexts)
            {
                string name = text.name.ToLower();
                if (name.Contains("result") || name.Contains("result") || name.Contains("winner"))
                {
                    resultText = text;
                    break;
                }
            }
        }
        
        // Auto find buttons if not assigned
        if (mainMenuButton == null)
        {
            Button[] allButtons = FindObjectsOfType<Button>();
            foreach (var button in allButtons)
            {
                string name = button.name.ToLower();
                if (name.Contains("menu") || name.Contains("menu") || name.Contains("main"))
                {
                    mainMenuButton = button;
                    break;
                }
            }
        }
        
        if (restartButton == null)
        {
            Button[] allButtons = FindObjectsOfType<Button>();
            foreach (var button in allButtons)
            {
                string name = button.name.ToLower();
                if (name.Contains("restart") || name.Contains("restart") || name.Contains("retry"))
                {
                    restartButton = button;
                    break;
                }
            }
        }
    }
    
    private void SetupButtons()
    {
        // Setup main menu button
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(LoadMainMenu);
            mainMenuButton.gameObject.SetActive(false); // Hide initially
        }
        
        // Setup restart button
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
            restartButton.gameObject.SetActive(false); // Hide initially
        }
    }
    
    public void OnPlayerFinish()
    {
        if (!gameActive || playerFinished) return;
        
        playerFinished = true;
        
        if (!enemyFinished)
        {
            // Player wins - first to finish
            HandlePlayerWin();
        }
        else
        {
            // Player finished but enemy was first
            // Game should already be in enemy win state
        }
    }
    
    public void OnEnemyFinish()
    {
        if (!gameActive || enemyFinished) return;
        
        enemyFinished = true;
        
        if (!playerFinished)
        {
            // Enemy wins - first to finish
            HandleEnemyWin();
        }
        else
        {
            // Enemy finished but player was first
            // Game should already be in player win state
        }
    }
    
    private void HandlePlayerWin()
    {
        gameFinishTime = Time.time;
        gameFinished = true;
        gameActive = false;
        
        Debug.Log("Player wins the race!");
        
        // Play player win sound
        if (audioSource != null && playerWinSound != null)
        {
            audioSource.PlayOneShot(playerWinSound);
        }
        
        // Stop all game systems
        StopGameSystems();
        
        // Show player win UI first (only image and result text)
        ShowInitialFinishUI(true);
        
        // Start coroutine to show statistics after delay
        StartCoroutine(ShowStatisticsAfterDelay());
    }
    
    private void HandleEnemyWin()
    {
        gameFinishTime = Time.time;
        gameFinished = true;
        gameActive = false; // Stop game immediately when enemy wins
        
        Debug.Log("Enemy wins the race!");
        
        // Play enemy win sound
        if (audioSource != null && enemyWinSound != null)
        {
            audioSource.PlayOneShot(enemyWinSound);
        }
        
        // Stop all game systems immediately
        StopGameSystems();
        
        // Show enemy win UI first (only image and result text)
        ShowInitialFinishUI(false);
        
        // Start coroutine to show statistics after delay
        StartCoroutine(ShowStatisticsAfterDelay());
    }
    
    private void StopGameSystems()
    {
        // Pause the game
        Time.timeScale = 0f;
        
        // Enable cursor for menu interaction
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Stop player movement and input systems
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            // Disable movement components
            MonoBehaviour[] playerComponents = player.GetComponents<MonoBehaviour>();
            foreach (var component in playerComponents)
            {
                if (component.GetType().Name.Contains("Movement") || 
                    component.GetType().Name.Contains("Input") ||
                    component.GetType().Name.Contains("Controller"))
                {
                    component.enabled = false;
                }
            }
            
            // Stop rigidbody
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector3.zero;
                playerRb.angularVelocity = Vector3.zero;
                playerRb.isKinematic = true;
            }
        }
    }
    
    private void ShowInitialFinishUI(bool playerWon)
    {
        if (finishUI != null)
        {
            finishUI.SetActive(true);
        }
        
        // Set result image
        if (resultImage != null)
        {
            if (playerWon && playerWinSprite != null)
            {
                resultImage.sprite = playerWinSprite;
            }
            else if (!playerWon && enemyWinSprite != null)
            {
                resultImage.sprite = enemyWinSprite;
            }
            resultImage.gameObject.SetActive(true);
        }
        
        // Set result text
        if (resultText != null)
        {
            if (playerWon)
            {
                resultText.text = "Victory! You reached the finish first!";
                resultText.color = Color.green;
            }
            else
            {
                resultText.text = "Defeat! Enemy reached the finish first!";
                resultText.color = Color.red;
            }
            resultText.gameObject.SetActive(true);
        }
        
        // Hide time text initially
        if (timeText != null)
        {
            timeText.gameObject.SetActive(false);
        }
        
        // Hide statistics UI elements initially
        HideStatisticsUI();
        
        // Hide buttons initially
        if (mainMenuButton != null)
        {
            mainMenuButton.gameObject.SetActive(false);
        }
        
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
        }
    }
    
    private void HideStatisticsUI()
    {
        // Hide finish screen statistics elements
        GameObject finishUIObj = GameObject.Find("FinishUI");
        if (finishUIObj != null)
        {
            Transform speedTextTransform = finishUIObj.transform.Find("MaxSpeedText");
            if (speedTextTransform != null)
            {
                speedTextTransform.gameObject.SetActive(false);
            }
            
            Transform itemsTextTransform = finishUIObj.transform.Find("ItemsEatenText");
            if (itemsTextTransform != null)
            {
                itemsTextTransform.gameObject.SetActive(false);
            }
        }
        
        // Hide GameStatsTracker UI elements
        if (GameStatsTracker.Instance != null)
        {
            GameStatsTracker.Instance.HideStatistics();
        }
    }
    
    private IEnumerator ShowStatisticsAfterDelay()
    {
        // Wait for the specified delay (using unscaled time since game is paused)
        yield return new WaitForSecondsRealtime(statisticsDelay);
        
        Debug.Log("Showing statistics after delay...");
        
        // Hide result image when showing statistics
        if (resultImage != null)
        {
            resultImage.gameObject.SetActive(false);
        }
        
        // Show time display
        UpdateTimeDisplay();
        if (timeText != null)
        {
            timeText.gameObject.SetActive(true);
        }
        
        // Show all statistics
        ShowStatisticsAndButtons();
    }
    
    private void UpdateTimeDisplay()
    {
        if (timeText != null && gameFinished)
        {
            float totalTime = gameFinishTime - gameStartTime;
            string timeString = FormatTime(totalTime);
            timeText.text = $"Time: {timeString}";
        }
    }
    
    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int milliseconds = Mathf.FloorToInt((timeInSeconds * 1000f) % 1000f);
        
        return $"{minutes:00}:{seconds:00}.{milliseconds:000}";
    }
    
    private void ShowStatisticsAndButtons()
    {
        // Show game statistics
        if (GameStatsTracker.Instance != null)
        {
            // Add time to statistics
            if (gameFinished)
            {
                float totalTime = gameFinishTime - gameStartTime;
                GameStatsTracker.Instance.SetGameTime(totalTime);
            }
            
            // Force show all statistics with current data
            GameStatsTracker.Instance.ForceShowStatistics();
        }
        
        // Show menu buttons
        if (mainMenuButton != null)
        {
            mainMenuButton.gameObject.SetActive(true);
        }
        
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
        }
    }
    
    // Button event handlers
    public void LoadMainMenu()
    {
        Debug.Log("Loading main menu...");
        
        // Restore time scale before loading
        Time.timeScale = 1f;
        
        // Restore cursor state
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Load main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }
    
    // Public methods for external access
    public float GetGameTime()
    {
        if (gameFinished)
        {
            return gameFinishTime - gameStartTime;
        }
        else
        {
            return Time.time - gameStartTime;
        }
    }
    
    public bool IsGameFinished()
    {
        return gameFinished;
    }
    
    public bool IsGameActive()
    {
        return gameActive;
    }
    
    // Method to restart the game
    public void RestartGame()
    {
        Debug.Log("Restarting game...");
        
        // Reset game statistics before restarting
        if (GameStatsTracker.Instance != null)
        {
            GameStatsTracker.Instance.ResetStats();
            Debug.Log("Game statistics reset for restart");
        }
        
        // Restore time scale
        Time.timeScale = 1f;
        
        // Reload current scene
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneName);
    }
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
} 