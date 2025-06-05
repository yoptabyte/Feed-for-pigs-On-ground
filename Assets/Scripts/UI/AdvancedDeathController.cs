using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class AdvancedDeathController : MonoBehaviour
{
    [Header("Death UI Settings")]
    [Tooltip("AllPigs UI image to show on death")]
    public GameObject allPigsImage;
    
    [Tooltip("Main menu button")]
    public Button mainMenuButton;
    
    [Tooltip("Player/pig Health component")]
    public Health playerHealth;
    
    [Header("Audio Settings")]
    [Tooltip("AudioSource for playing death music")]
    public AudioSource deathAudioSource;
    
    [Tooltip("Audio clip to play on death")]
    public AudioClip deathMusicClip;
    
    [Tooltip("Death music volume")]
    [Range(0f, 1f)]
    public float deathMusicVolume = 0.8f;
    
    [Header("Statistics UI")]
    [Tooltip("Text component for displaying maximum speed")]
    public Text maxSpeedText;
    
    [Tooltip("TextMeshPro component for displaying maximum speed")]
    public TextMeshProUGUI maxSpeedTextTMP;
    
    [Tooltip("Text component for displaying number of eaten items")]
    public Text itemsEatenText;
    
    [Tooltip("TextMeshPro component for displaying number of eaten items")]
    public TextMeshProUGUI itemsEatenTextTMP;
    
    [Header("System Control")]
    [Tooltip("Stop MovementSystem on death")]
    public bool stopMovementSystem = true;
    
    [Tooltip("Stop PlayerInputSystem on death")]
    public bool stopPlayerInput = true;
    
    [Tooltip("Stop Rigidbody on death")]
    public bool stopRigidbody = true;
    
    [Tooltip("Stop CameraSwitcher on death")]
    public bool stopCameraSwitcher = true;
    
    [Header("Auto Find Settings")]
    [Tooltip("Automatically find components on start")]
    public bool autoFindComponents = true;
    
    [Header("Debug")]
    [Tooltip("Show debug information")]
    public bool showDebugInfo = false;
    
    [Header("Death Timing")]
    [Tooltip("Delay in seconds before showing statistics and removing AllPigs")]
    public float deathDelaySeconds = 4f;
    
    [Header("Main Menu Settings")]
    [Tooltip("Main menu scene name")]
    public string mainMenuSceneName = "MainMenu";
    
    [Tooltip("Show main menu button on death")]
    public bool showMainMenuButton = true;
    
    [Tooltip("Create main menu button if not found")]
    public bool createMainMenuButton = true;
    
    [Tooltip("Main menu button position on screen")]
    public Vector2 mainMenuButtonPosition = new Vector2(0, -100);
    
    [Tooltip("Main menu button size")]
    public Vector2 mainMenuButtonSize = new Vector2(200, 50);
    
    [Tooltip("Main menu button text")]
    public string mainMenuButtonText = "Main Menu";
    
    [Tooltip("Use TextMeshPro for button")]
    public bool useTextMeshProForButton = true;
    
    [Header("Cursor Settings")]
    [Tooltip("Enable cursor on death")]
    public bool enableCursorOnDeath = true;
    
    [Tooltip("Unlock cursor on death")]
    public bool unlockCursorOnDeath = true;
    
    // Private variables
    private bool hasPlayedDeathMusic = false;
    private bool isDeathHandled = false;
    private Image allPigsImageComponent;
    private GameObject playerObject;
    private MovementSystem movementSystem;
    private PlayerInputSystem playerInputSystem;
    private Rigidbody playerRigidbody;
    private CameraSwitcher cameraSwitcher;
    
    // Cursor state storage
    private bool originalCursorVisible;
    private CursorLockMode originalCursorLockState;
    
    void Start()
    {
        // Save original cursor state
        originalCursorVisible = Cursor.visible;
        originalCursorLockState = Cursor.lockState;
        
        if (autoFindComponents)
        {
            AutoFindComponents();
        }
        
        SetupDeathUI();
        SubscribeToDeathEvent();
    }
    
    private void AutoFindComponents()
    {
        if (showDebugInfo)
        {
            Debug.Log("AdvancedDeathController: Auto-finding components...");
        }
        
        // Find AllPigs image
        if (allPigsImage == null)
        {
            allPigsImage = FindObjectByName("AllPigs");
            if (allPigsImage != null)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"AdvancedDeathController: Found AllPigs: {allPigsImage.name}");
                }
            }
            else
            {
                Debug.LogWarning("AdvancedDeathController: AllPigs image not found!");
            }
        }
        
        // Find player health and related components
        if (playerHealth == null)
        {
            // Try to find by Player tag
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
                playerObject = player;
            }
            
            // If not found, search all Health components
            if (playerHealth == null)
            {
                Health[] allHealthComponents = FindObjectsOfType<Health>();
                foreach (Health health in allHealthComponents)
                {
                    string name = health.gameObject.name.ToLower();
                    if (name.Contains("pig") || name.Contains("player"))
                    {
                        playerHealth = health;
                        playerObject = health.gameObject;
                        if (showDebugInfo)
                        {
                            Debug.Log($"AdvancedDeathController: Found Health component: {health.gameObject.name}");
                        }
                        break;
                    }
                }
            }
        }
        else
        {
            playerObject = playerHealth.gameObject;
        }
        
        // Find systems to stop
        if (playerObject != null)
        {
            FindSystemsToStop();
        }
        
        // Find statistics UI
        AutoFindStatisticsUI();
        
        // Find or create AudioSource
        if (deathAudioSource == null)
        {
            deathAudioSource = GetComponent<AudioSource>();
            if (deathAudioSource == null)
            {
                deathAudioSource = gameObject.AddComponent<AudioSource>();
                if (showDebugInfo)
                {
                    Debug.Log("AdvancedDeathController: Created new AudioSource");
                }
            }
        }
        
        // Find main menu button
        if (mainMenuButton == null && showMainMenuButton)
        {
            mainMenuButton = FindMainMenuButton();
            if (mainMenuButton != null)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"AdvancedDeathController: Found main menu button: {mainMenuButton.name}");
                }
            }
            else if (createMainMenuButton)
            {
                CreateMainMenuButton();
            }
            else
            {
                Debug.LogWarning("AdvancedDeathController: Main menu button not found and auto-creation is disabled!");
            }
        }
    }
    
    private void FindSystemsToStop()
    {
        if (playerObject == null) return;
        
        // Find MovementSystem
        movementSystem = FindObjectOfType<MovementSystem>();
        if (movementSystem != null && showDebugInfo)
        {
            Debug.Log($"AdvancedDeathController: Found MovementSystem: {movementSystem.name}");
        }
        
        // Find PlayerInputSystem
        playerInputSystem = FindObjectOfType<PlayerInputSystem>();
        if (playerInputSystem != null && showDebugInfo)
        {
            Debug.Log($"AdvancedDeathController: Found PlayerInputSystem: {playerInputSystem.name}");
        }
        
        // Find Rigidbody on player
        playerRigidbody = playerObject.GetComponent<Rigidbody>();
        if (playerRigidbody == null)
        {
            playerRigidbody = playerObject.GetComponentInChildren<Rigidbody>();
        }
        if (playerRigidbody != null && showDebugInfo)
        {
            Debug.Log($"AdvancedDeathController: Found Rigidbody: {playerRigidbody.name}");
        }
        
        // Find CameraSwitcher
        cameraSwitcher = FindObjectOfType<CameraSwitcher>();
        if (cameraSwitcher != null && showDebugInfo)
        {
            Debug.Log($"AdvancedDeathController: Found CameraSwitcher: {cameraSwitcher.name}");
        }
    }
    
    private void AutoFindStatisticsUI()
    {
        // Find all Text components in scene
        Text[] allTexts = FindObjectsOfType<Text>();
        TextMeshProUGUI[] allTextsTMP = FindObjectsOfType<TextMeshProUGUI>();
        
        // Try to find speed text (regular Text)
        if (maxSpeedText == null)
        {
            foreach (Text text in allTexts)
            {
                string name = text.name.ToLower();
                if (name.Contains("speed") || name.Contains("max"))
                {
                    maxSpeedText = text;
                    if (showDebugInfo)
                    {
                        Debug.Log($"AdvancedDeathController: Found Speed Text: {text.name}");
                    }
                    break;
                }
            }
        }
        
        // Try to find speed text (TextMeshPro)
        if (maxSpeedTextTMP == null)
        {
            foreach (TextMeshProUGUI textTMP in allTextsTMP)
            {
                string name = textTMP.name.ToLower();
                if (name.Contains("speed") || name.Contains("max"))
                {
                    maxSpeedTextTMP = textTMP;
                    if (showDebugInfo)
                    {
                        Debug.Log($"AdvancedDeathController: Found Speed TextMeshPro: {textTMP.name}");
                    }
                    break;
                }
            }
        }
        
        // Try to find items text (regular Text)
        if (itemsEatenText == null)
        {
            foreach (Text text in allTexts)
            {
                string name = text.name.ToLower();
                if (name.Contains("item") || name.Contains("eaten"))
                {
                    itemsEatenText = text;
                    if (showDebugInfo)
                    {
                        Debug.Log($"AdvancedDeathController: Found Items Text: {text.name}");
                    }
                    break;
                }
            }
        }
        
        // Try to find items text (TextMeshPro)
        if (itemsEatenTextTMP == null)
        {
            foreach (TextMeshProUGUI textTMP in allTextsTMP)
            {
                string name = textTMP.name.ToLower();
                if (name.Contains("item") || name.Contains("eaten"))
                {
                    itemsEatenTextTMP = textTMP;
                    if (showDebugInfo)
                    {
                        Debug.Log($"AdvancedDeathController: Found Items TextMeshPro: {textTMP.name}");
                    }
                    break;
                }
            }
        }
    }
    
    private GameObject FindObjectByName(string objectName)
    {
        // Search in all GameObjects
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == objectName)
            {
                return obj;
            }
        }
        return null;
    }
    
    private Button FindMainMenuButton()
    {
        // Search for buttons with main menu related names
        Button[] allButtons = FindObjectsOfType<Button>(true); // Include inactive
        
        foreach (Button button in allButtons)
        {
            string name = button.name.ToLower();
            if (name.Contains("mainmenu") || name.Contains("main_menu") || name.Contains("menu") || 
                name.Contains("main") || name.Contains("exit"))
            {
                return button;
            }
            
            // Also check button text
            Text buttonText = button.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                string text = buttonText.text.ToLower();
                if (text.Contains("main menu") || text.Contains("menu") || text.Contains("exit"))
                {
                    return button;
                }
            }
            
            // Check TextMeshPro text
            TextMeshProUGUI buttonTextTMP = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonTextTMP != null)
            {
                string text = buttonTextTMP.text.ToLower();
                if (text.Contains("main menu") || text.Contains("menu") || text.Contains("exit"))
                {
                    return button;
                }
            }
        }
        
        return null;
    }
    
    private void CreateMainMenuButton()
    {
        // Find Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("AdvancedDeathController: Canvas not found for creating main menu button!");
            return;
        }
        
        // Create button GameObject
        GameObject buttonGO = new GameObject("MainMenuButton");
        buttonGO.transform.SetParent(canvas.transform, false);
        
        // Add Image component for button background
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Dark semi-transparent
        
        // Add Button component
        Button button = buttonGO.AddComponent<Button>();
        mainMenuButton = button;
        
        // Set button size and position
        RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
        buttonRect.sizeDelta = mainMenuButtonSize;
        buttonRect.anchoredPosition = mainMenuButtonPosition;
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f); // Center anchor
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        
        // Create text child
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        
        if (useTextMeshProForButton)
        {
            TextMeshProUGUI textComponent = textGO.AddComponent<TextMeshProUGUI>();
            textComponent.text = mainMenuButtonText;
            textComponent.fontSize = 18;
            textComponent.color = Color.white;
            textComponent.alignment = TextAlignmentOptions.Center;
            
            // Set text to fill button
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
        else
        {
            Text textComponent = textGO.AddComponent<Text>();
            textComponent.text = mainMenuButtonText;
            textComponent.fontSize = 18;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.MiddleCenter;
            
            // Try to find font
            Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font != null)
            {
                textComponent.font = font;
            }
            
            // Set text to fill button
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"AdvancedDeathController: Created main menu button at position {mainMenuButtonPosition}");
        }
    }
    
    private void SetupDeathUI()
    {
        // Setup AllPigs image
        if (allPigsImage != null)
        {
            allPigsImageComponent = allPigsImage.GetComponent<Image>();
            
            // Initially hide the death UI
            allPigsImage.SetActive(false);
            
            if (showDebugInfo)
            {
                Debug.Log("AdvancedDeathController: AllPigs image set up and hidden");
            }
        }
        
        // Setup main menu button
        if (mainMenuButton != null)
        {
            // Initially hide the button
            mainMenuButton.gameObject.SetActive(false);
            
            // Setup button click event
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(LoadMainMenu);
            
            if (showDebugInfo)
            {
                Debug.Log("AdvancedDeathController: Main menu button set up and hidden");
            }
        }
        
        // Setup audio source
        if (deathAudioSource != null)
        {
            deathAudioSource.volume = deathMusicVolume;
            deathAudioSource.playOnAwake = false;
            deathAudioSource.loop = false;
            
            if (deathMusicClip != null)
            {
                deathAudioSource.clip = deathMusicClip;
            }
        }
    }
    
    private void SubscribeToDeathEvent()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath.AddListener(OnPlayerDeath);
            
            if (showDebugInfo)
            {
                Debug.Log($"AdvancedDeathController: Subscribed to death event for {playerHealth.gameObject.name}");
            }
        }
        else
        {
            Debug.LogError("AdvancedDeathController: Health component not found! Please assign it manually or enable autoFindComponents");
        }
    }
    
    private void OnPlayerDeath()
    {
        if (isDeathHandled)
        {
            if (showDebugInfo)
            {
                Debug.Log("AdvancedDeathController: Death already handled, skipping");
            }
            return;
        }
        
        isDeathHandled = true;
        
        if (showDebugInfo)
        {
            Debug.Log("AdvancedDeathController: Death event received!");
        }
        
        StopGameSystems();
        ShowDeathUI();
        PlayDeathMusic();
        EnableCursor();
        
        // Start coroutine for delayed statistics and AllPigs removal
        StartCoroutine(HandleDelayedDeathSequence());
    }
    
    private IEnumerator HandleDelayedDeathSequence()
    {
        if (showDebugInfo)
        {
            Debug.Log($"AdvancedDeathController: Starting delay wait for {deathDelaySeconds} seconds...");
        }
        
        // Wait for the specified delay
        yield return new WaitForSeconds(deathDelaySeconds);
        
        if (showDebugInfo)
        {
            Debug.Log("AdvancedDeathController: Delay completed, showing statistics and main menu button");
        }
        
        // Show statistics after delay
        UpdateStatisticsUI();
        
        // Show main menu button after delay
        if (mainMenuButton != null && showMainMenuButton)
        {
            mainMenuButton.gameObject.SetActive(true);
            if (showDebugInfo)
            {
                Debug.Log("AdvancedDeathController: Main menu button shown");
            }
        }
        
        // Remove AllPigs image after delay
        if (allPigsImage != null)
        {
            Destroy(allPigsImage);
            if (showDebugInfo)
            {
                Debug.Log("AdvancedDeathController: AllPigs image removed");
            }
        }
    }
    
    private void StopGameSystems()
    {
        if (showDebugInfo)
        {
            Debug.Log("AdvancedDeathController: Stopping game systems...");
        }
        
        // Stop MovementSystem
        if (stopMovementSystem && movementSystem != null)
        {
            movementSystem.enabled = false;
            if (showDebugInfo)
            {
                Debug.Log("AdvancedDeathController: MovementSystem stopped");
            }
        }
        
        // Stop PlayerInputSystem
        if (stopPlayerInput && playerInputSystem != null)
        {
            playerInputSystem.enabled = false;
            if (showDebugInfo)
            {
                Debug.Log("AdvancedDeathController: PlayerInputSystem stopped");
            }
        }
        
        // Stop Rigidbody
        if (stopRigidbody && playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            playerRigidbody.isKinematic = true;
            if (showDebugInfo)
            {
                Debug.Log("AdvancedDeathController: Rigidbody stopped");
            }
        }
        
        // Stop CameraSwitcher
        if (stopCameraSwitcher && cameraSwitcher != null)
        {
            cameraSwitcher.enabled = false;
            if (showDebugInfo)
            {
                Debug.Log("AdvancedDeathController: CameraSwitcher stopped");
            }
        }
    }
    
    private void ShowDeathUI()
    {
        if (allPigsImage != null)
        {
            allPigsImage.SetActive(true);
            
            if (showDebugInfo)
            {
                Debug.Log("AdvancedDeathController: Shown AllPigs image");
            }
        }
        else
        {
            Debug.LogError("AdvancedDeathController: AllPigs image not assigned!");
        }
    }
    
    private void UpdateStatisticsUI()
    {
        GameStatsTracker statsTracker = GameStatsTracker.Instance;
        if (statsTracker != null)
        {
            // Show statistics when death occurs
            statsTracker.ShowStatistics();
            
            string speedText = $"Max speed: {statsTracker.MaxSpeedReached:F1}";
            string itemsText = $"Collected items: {statsTracker.TotalItemsEaten}";
            
            // Update max speed text (regular Text)
            if (maxSpeedText != null)
            {
                maxSpeedText.text = speedText;
                if (showDebugInfo)
                {
                    Debug.Log($"AdvancedDeathController: Updated speed text: {statsTracker.MaxSpeedReached:F1}");
                }
            }
            
            // Update max speed text (TextMeshPro)
            if (maxSpeedTextTMP != null)
            {
                maxSpeedTextTMP.text = speedText;
                if (showDebugInfo)
                {
                    Debug.Log($"AdvancedDeathController: Updated TextMeshPro speed: {statsTracker.MaxSpeedReached:F1}");
                }
            }
            
            // Update items eaten text (regular Text)
            if (itemsEatenText != null)
            {
                itemsEatenText.text = itemsText;
                if (showDebugInfo)
                {
                    Debug.Log($"AdvancedDeathController: Updated items text: {statsTracker.TotalItemsEaten}");
                }
            }
            
            // Update items eaten text (TextMeshPro)
            if (itemsEatenTextTMP != null)
            {
                itemsEatenTextTMP.text = itemsText;
                if (showDebugInfo)
                {
                    Debug.Log($"AdvancedDeathController: Updated TextMeshPro items: {statsTracker.TotalItemsEaten}");
                }
            }
        }
        else
        {
            Debug.LogWarning("AdvancedDeathController: GameStatsTracker not found for updating statistics");
        }
    }
    
    private void PlayDeathMusic()
    {
        if (deathAudioSource != null && deathMusicClip != null && !hasPlayedDeathMusic)
        {
            deathAudioSource.clip = deathMusicClip;
            deathAudioSource.volume = deathMusicVolume;
            deathAudioSource.Play();
            
            hasPlayedDeathMusic = true;
            
            if (showDebugInfo)
            {
                Debug.Log("AdvancedDeathController: Death music played");
            }
        }
        else if (hasPlayedDeathMusic)
        {
            if (showDebugInfo)
            {
                Debug.Log("AdvancedDeathController: Death music already played");
            }
        }
        else
        {
            Debug.LogWarning("AdvancedDeathController: AudioSource or death music clip not assigned!");
        }
    }
    
    // Public methods for external control
    public void HideDeathUI()
    {
        if (allPigsImage != null)
        {
            allPigsImage.SetActive(false);
            Debug.Log("AdvancedDeathController: AllPigs image hidden");
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.gameObject.SetActive(false);
            Debug.Log("AdvancedDeathController: Main menu button hidden");
        }
    }
    
    public void LoadMainMenu()
    {
        if (showDebugInfo)
        {
            Debug.Log($"AdvancedDeathController: Loading main menu: {mainMenuSceneName}");
        }
        
        // Stop death music before loading scene
        StopDeathMusic();
        
        // IMPORTANT: DO NOT restore cursor, leave it visible for main menu
        // RestoreCursor(); // Commented out - let MainMenuManager set up the cursor itself
        
        // Ensure cursor is visible and unlocked for main menu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        if (showDebugInfo)
        {
            Debug.Log($"AdvancedDeathController: Cursor prepared for main menu - Visible: {Cursor.visible}, LockState: {Cursor.lockState}");
        }
        
        // Ensure time scale is normal before loading scene
        Time.timeScale = 1f;
        
        // Load main menu scene with multiple fallback options
        try
        {
            // First try: Load by scene name
            if (!string.IsNullOrEmpty(mainMenuSceneName))
            {
                if (Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
                {
                    Debug.Log($"AdvancedDeathController: Loading scene by name: {mainMenuSceneName}");
                    UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
                    return;
                }
                else
                {
                    Debug.LogWarning($"AdvancedDeathController: Scene '{mainMenuSceneName}' cannot be loaded by name");
                }
            }
            
            // Second try: Load by build index (MainMenu should be at index 0 or 1)
            Debug.Log("AdvancedDeathController: Trying to load main menu by index...");
            
            // Check if we have scenes in build settings
            if (UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings > 1)
            {
                // Try to find MainMenu scene by checking build settings
                for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
                {
                    string scenePath = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
                    string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                    
                    if (sceneName.ToLower().Contains("mainmenu") || sceneName.ToLower().Contains("menu"))
                    {
                        Debug.Log($"AdvancedDeathController: Found main menu scene by index {i}: {sceneName}");
                        UnityEngine.SceneManagement.SceneManager.LoadScene(i);
                        return;
                    }
                }
                
                // If no MainMenu found, try index 0 (usually main menu)
                Debug.Log("AdvancedDeathController: Loading scene by index 0 (assumed main menu)");
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
                return;
            }
            
            // If all else fails
            Debug.LogError("AdvancedDeathController: Unable to find suitable scene for loading!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"AdvancedDeathController: Critical loading scene error: {e.Message}");
            Debug.LogError("AdvancedDeathController: Check that scenes are added to Build Settings!");
            
            // Last resort: try to quit application or restart current scene
            Debug.LogError("AdvancedDeathController: Trying to reload current scene as last resort...");
            try
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
            catch (System.Exception restartException)
            {
                Debug.LogError($"AdvancedDeathController: Unable to reload scene: {restartException.Message}");
            }
        }
    }
    
    public void ResetDeathState()
    {
        isDeathHandled = false;
        hasPlayedDeathMusic = false;
        
        // Re-enable systems
        if (movementSystem != null) movementSystem.enabled = true;
        if (playerInputSystem != null) playerInputSystem.enabled = true;
        if (cameraSwitcher != null) cameraSwitcher.enabled = true;
        if (playerRigidbody != null) playerRigidbody.isKinematic = false;
        
        // Restore cursor state
        RestoreCursor();
        
        HideDeathUI();
        
        Debug.Log("AdvancedDeathController: Death state reset");
    }
    
    public void StopDeathMusic()
    {
        if (deathAudioSource != null && deathAudioSource.isPlaying)
        {
            deathAudioSource.Stop();
            Debug.Log("AdvancedDeathController: Death music stopped");
        }
    }
    
    // Context menu methods for testing
    [ContextMenu("Test Death")]
    public void TestDeath()
    {
        OnPlayerDeath();
    }
    
    [ContextMenu("Test Delayed Death Sequence")]
    public void TestDelayedDeathSequence()
    {
        StartCoroutine(HandleDelayedDeathSequence());
    }
    
    [ContextMenu("Reset Death State")]
    public void TestResetDeathState()
    {
        ResetDeathState();
    }
    
    [ContextMenu("Find Components")]
    public void ManualFindComponents()
    {
        AutoFindComponents();
    }
    
    [ContextMenu("Update Statistics UI")]
    public void TestUpdateStatisticsUI()
    {
        UpdateStatisticsUI();
    }
    
    [ContextMenu("Create Main Menu Button")]
    public void ManualCreateMainMenuButton()
    {
        CreateMainMenuButton();
    }
    
    [ContextMenu("Setup Complete Death System")]
    public void SetupCompleteDeathSystem()
    {
        Debug.Log("=== SETUP COMPLETE DEATH SYSTEM ===");
        
        // Auto find all components
        AutoFindComponents();
        
        // Setup UI
        SetupDeathUI();
        
        // Subscribe to death event
        SubscribeToDeathEvent();
        
        Debug.Log("=== DEATH SYSTEM SETUP COMPLETE ===");
    }
    
    [ContextMenu("Test Enable Cursor")]
    public void TestEnableCursor()
    {
        EnableCursor();
        Debug.Log($"AdvancedDeathController: Test cursor - Visible: {Cursor.visible}, LockState: {Cursor.lockState}");
    }
    
    [ContextMenu("Test Restore Cursor")]
    public void TestRestoreCursor()
    {
        RestoreCursor();
        Debug.Log($"AdvancedDeathController: Cursor restored - Visible: {Cursor.visible}, LockState: {Cursor.lockState}");
    }
    
    [ContextMenu("Test Load Main Menu")]
    public void TestLoadMainMenu()
    {
        Debug.Log("AdvancedDeathController: Testing main menu loading...");
        LoadMainMenu();
    }
    
    [ContextMenu("Diagnose Scene Loading")]
    public void DiagnoseSceneLoading()
    {
        Debug.Log("=== SCENE LOADING DIAGNOSTICS ===");
        
        Debug.Log($"Current scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        Debug.Log($"Target main menu scene: '{mainMenuSceneName}'");
        Debug.Log($"Scenes in Build Settings: {UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings}");
        
        // Check if target scene can be loaded
        bool canLoad = !string.IsNullOrEmpty(mainMenuSceneName) && Application.CanStreamedLevelBeLoaded(mainMenuSceneName);
        Debug.Log($"Can load '{mainMenuSceneName}': {canLoad}");
        
        // List all scenes in build settings
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            Debug.Log($"Build Index {i}: {sceneName}");
        }
        
        // Check button setup
        if (mainMenuButton != null)
        {
            Debug.Log($"Main menu button: {mainMenuButton.name}");
            Debug.Log($"Button active: {mainMenuButton.gameObject.activeInHierarchy}");
            Debug.Log($"Button listeners: {mainMenuButton.onClick.GetPersistentEventCount()}");
        }
        else
        {
            Debug.LogWarning("Main menu button NOT ASSIGNED!");
        }
        
        Debug.Log("=== END DIAGNOSTICS ===");
    }
    
    [ContextMenu("Force Create New Main Menu Button")]
    public void ForceCreateNewMainMenuButton()
    {
        Debug.Log("AdvancedDeathController: Force creating new main menu button...");
        
        // Destroy existing button if any
        if (mainMenuButton != null)
        {
            DestroyImmediate(mainMenuButton.gameObject);
            mainMenuButton = null;
            Debug.Log("Old button removed");
        }
        
        // Create new button
        CreateMainMenuButton();
        
        // Setup the button
        if (mainMenuButton != null)
        {
            SetupDeathUI();
            Debug.Log("New button created and set up");
        }
        else
        {
            Debug.LogError("Unable to create new button!");
        }
    }
    
    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath.RemoveListener(OnPlayerDeath);
        }
    }
    
    private void EnableCursor()
    {
        if (enableCursorOnDeath)
        {
            Cursor.visible = true;
            
            if (unlockCursorOnDeath)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            
            if (showDebugInfo)
            {
                Debug.Log("AdvancedDeathController: Cursor enabled for UI interaction");
            }
        }
    }
    
    private void RestoreCursor()
    {
        Cursor.visible = originalCursorVisible;
        Cursor.lockState = originalCursorLockState;
        
        if (showDebugInfo)
        {
            Debug.Log("AdvancedDeathController: Cursor state restored");
        }
    }
} 