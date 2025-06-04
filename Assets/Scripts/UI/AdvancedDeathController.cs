using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class AdvancedDeathController : MonoBehaviour
{
    [Header("Death UI Settings")]
    [Tooltip("UI изображение AllPigs для показа при смерти")]
    public GameObject allPigsImage;
    
    [Tooltip("Кнопка выхода в главное меню")]
    public Button mainMenuButton;
    
    [Tooltip("Компонент Health свиньи/игрока")]
    public Health playerHealth;
    
    [Header("Audio Settings")]
    [Tooltip("AudioSource для проигрывания музыки смерти")]
    public AudioSource deathAudioSource;
    
    [Tooltip("Аудиоклип для проигрывания при смерти")]
    public AudioClip deathMusicClip;
    
    [Tooltip("Громкость музыки смерти")]
    [Range(0f, 1f)]
    public float deathMusicVolume = 0.8f;
    
    [Header("Statistics UI")]
    [Tooltip("Text для отображения максимальной скорости")]
    public Text maxSpeedText;
    
    [Tooltip("TextMeshPro для отображения максимальной скорости")]
    public TextMeshProUGUI maxSpeedTextTMP;
    
    [Tooltip("Text для отображения количества съеденных предметов")]
    public Text itemsEatenText;
    
    [Tooltip("TextMeshPro для отображения количества съеденных предметов")]
    public TextMeshProUGUI itemsEatenTextTMP;
    
    [Header("System Control")]
    [Tooltip("Остановить MovementSystem при смерти")]
    public bool stopMovementSystem = true;
    
    [Tooltip("Остановить PlayerInputSystem при смерти")]
    public bool stopPlayerInput = true;
    
    [Tooltip("Остановить Rigidbody при смерти")]
    public bool stopRigidbody = true;
    
    [Tooltip("Остановить CameraSwitcher при смерти")]
    public bool stopCameraSwitcher = true;
    
    [Header("Auto Find Settings")]
    [Tooltip("Автоматически найти компоненты при старте")]
    public bool autoFindComponents = true;
    
    [Header("Debug")]
    [Tooltip("Показывать отладочную информацию")]
    public bool showDebugInfo = false;
    
    [Header("Death Timing")]
    [Tooltip("Задержка в секундах перед показом статистики и удалением AllPigs")]
    public float deathDelaySeconds = 4f;
    
    [Header("Main Menu Settings")]
    [Tooltip("Название сцены главного меню")]
    public string mainMenuSceneName = "MainMenu";
    
    [Tooltip("Показывать кнопку главного меню при смерти")]
    public bool showMainMenuButton = true;
    
    [Tooltip("Создать кнопку главного меню если не найдена")]
    public bool createMainMenuButton = true;
    
    [Tooltip("Позиция кнопки главного меню на экране")]
    public Vector2 mainMenuButtonPosition = new Vector2(0, -100);
    
    [Tooltip("Размер кнопки главного меню")]
    public Vector2 mainMenuButtonSize = new Vector2(200, 50);
    
    [Tooltip("Текст кнопки главного меню")]
    public string mainMenuButtonText = "Главное меню";
    
    [Tooltip("Использовать TextMeshPro для кнопки")]
    public bool useTextMeshProForButton = true;
    
    [Header("Cursor Settings")]
    [Tooltip("Включить курсор при смерти")]
    public bool enableCursorOnDeath = true;
    
    [Tooltip("Разблокировать курсор при смерти")]
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
            Debug.Log("AdvancedDeathController: Автопоиск компонентов...");
        }
        
        // Find AllPigs image
        if (allPigsImage == null)
        {
            allPigsImage = FindObjectByName("AllPigs");
            if (allPigsImage != null)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"AdvancedDeathController: Найден AllPigs: {allPigsImage.name}");
                }
            }
            else
            {
                Debug.LogWarning("AdvancedDeathController: AllPigs изображение не найдено!");
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
                    if (name.Contains("pig") || name.Contains("player") || name.Contains("свин"))
                    {
                        playerHealth = health;
                        playerObject = health.gameObject;
                        if (showDebugInfo)
                        {
                            Debug.Log($"AdvancedDeathController: Найден Health компонент: {health.gameObject.name}");
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
                    Debug.Log("AdvancedDeathController: Создан новый AudioSource");
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
                    Debug.Log($"AdvancedDeathController: Найдена кнопка главного меню: {mainMenuButton.name}");
                }
            }
            else if (createMainMenuButton)
            {
                CreateMainMenuButton();
            }
            else
            {
                Debug.LogWarning("AdvancedDeathController: Кнопка главного меню не найдена и автосоздание отключено!");
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
            Debug.Log($"AdvancedDeathController: Найден MovementSystem: {movementSystem.name}");
        }
        
        // Find PlayerInputSystem
        playerInputSystem = FindObjectOfType<PlayerInputSystem>();
        if (playerInputSystem != null && showDebugInfo)
        {
            Debug.Log($"AdvancedDeathController: Найден PlayerInputSystem: {playerInputSystem.name}");
        }
        
        // Find Rigidbody on player
        playerRigidbody = playerObject.GetComponent<Rigidbody>();
        if (playerRigidbody == null)
        {
            playerRigidbody = playerObject.GetComponentInChildren<Rigidbody>();
        }
        if (playerRigidbody != null && showDebugInfo)
        {
            Debug.Log($"AdvancedDeathController: Найден Rigidbody: {playerRigidbody.name}");
        }
        
        // Find CameraSwitcher
        cameraSwitcher = FindObjectOfType<CameraSwitcher>();
        if (cameraSwitcher != null && showDebugInfo)
        {
            Debug.Log($"AdvancedDeathController: Найден CameraSwitcher: {cameraSwitcher.name}");
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
                if (name.Contains("speed") || name.Contains("скорость") || name.Contains("max"))
                {
                    maxSpeedText = text;
                    if (showDebugInfo)
                    {
                        Debug.Log($"AdvancedDeathController: Найден Speed Text: {text.name}");
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
                if (name.Contains("speed") || name.Contains("скорость") || name.Contains("max"))
                {
                    maxSpeedTextTMP = textTMP;
                    if (showDebugInfo)
                    {
                        Debug.Log($"AdvancedDeathController: Найден Speed TextMeshPro: {textTMP.name}");
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
                if (name.Contains("item") || name.Contains("предмет") || name.Contains("съед") || name.Contains("eaten"))
                {
                    itemsEatenText = text;
                    if (showDebugInfo)
                    {
                        Debug.Log($"AdvancedDeathController: Найден Items Text: {text.name}");
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
                if (name.Contains("item") || name.Contains("предмет") || name.Contains("съед") || name.Contains("eaten"))
                {
                    itemsEatenTextTMP = textTMP;
                    if (showDebugInfo)
                    {
                        Debug.Log($"AdvancedDeathController: Найден Items TextMeshPro: {textTMP.name}");
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
                name.Contains("главное") || name.Contains("выход") || name.Contains("exit"))
            {
                return button;
            }
            
            // Also check button text
            Text buttonText = button.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                string text = buttonText.text.ToLower();
                if (text.Contains("главное меню") || text.Contains("main menu") || text.Contains("меню") || 
                    text.Contains("выход") || text.Contains("exit"))
                {
                    return button;
                }
            }
            
            // Check TextMeshPro text
            TextMeshProUGUI buttonTextTMP = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonTextTMP != null)
            {
                string text = buttonTextTMP.text.ToLower();
                if (text.Contains("главное меню") || text.Contains("main menu") || text.Contains("меню") || 
                    text.Contains("выход") || text.Contains("exit"))
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
            Debug.LogError("AdvancedDeathController: Canvas не найден для создания кнопки главного меню!");
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
            Debug.Log($"AdvancedDeathController: Создана кнопка главного меню в позиции {mainMenuButtonPosition}");
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
                Debug.Log("AdvancedDeathController: AllPigs изображение настроено и скрыто");
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
                Debug.Log("AdvancedDeathController: Кнопка главного меню настроена и скрыта");
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
                Debug.Log($"AdvancedDeathController: Подписан на событие смерти {playerHealth.gameObject.name}");
            }
        }
        else
        {
            Debug.LogError("AdvancedDeathController: Health компонент не найден! Назначьте его вручную или включите autoFindComponents");
        }
    }
    
    private void OnPlayerDeath()
    {
        if (isDeathHandled)
        {
            if (showDebugInfo)
            {
                Debug.Log("AdvancedDeathController: Смерть уже обработана, пропускаю");
            }
            return;
        }
        
        isDeathHandled = true;
        
        if (showDebugInfo)
        {
            Debug.Log("AdvancedDeathController: Получено событие смерти игрока!");
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
            Debug.Log($"AdvancedDeathController: Начинаю ожидание {deathDelaySeconds} секунд...");
        }
        
        // Wait for the specified delay
        yield return new WaitForSeconds(deathDelaySeconds);
        
        if (showDebugInfo)
        {
            Debug.Log("AdvancedDeathController: Задержка завершена, показываю статистику и кнопку главного меню");
        }
        
        // Show statistics after delay
        UpdateStatisticsUI();
        
        // Show main menu button after delay
        if (mainMenuButton != null && showMainMenuButton)
        {
            mainMenuButton.gameObject.SetActive(true);
            if (showDebugInfo)
            {
                Debug.Log("AdvancedDeathController: Кнопка главного меню показана");
            }
        }
        
        // Remove AllPigs image after delay
        if (allPigsImage != null)
        {
            Destroy(allPigsImage);
            if (showDebugInfo)
            {
                Debug.Log("AdvancedDeathController: AllPigs изображение удалено");
            }
        }
    }
    
    private void StopGameSystems()
    {
        if (showDebugInfo)
        {
            Debug.Log("AdvancedDeathController: Остановка игровых систем...");
        }
        
        // Stop MovementSystem
        if (stopMovementSystem && movementSystem != null)
        {
            movementSystem.enabled = false;
            if (showDebugInfo)
            {
                Debug.Log("AdvancedDeathController: MovementSystem остановлен");
            }
        }
        
        // Stop PlayerInputSystem
        if (stopPlayerInput && playerInputSystem != null)
        {
            playerInputSystem.enabled = false;
            if (showDebugInfo)
            {
                Debug.Log("AdvancedDeathController: PlayerInputSystem остановлен");
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
                Debug.Log("AdvancedDeathController: Rigidbody остановлен");
            }
        }
        
        // Stop CameraSwitcher
        if (stopCameraSwitcher && cameraSwitcher != null)
        {
            cameraSwitcher.enabled = false;
            if (showDebugInfo)
            {
                Debug.Log("AdvancedDeathController: CameraSwitcher остановлен");
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
                Debug.Log("AdvancedDeathController: Показано AllPigs изображение");
            }
        }
        else
        {
            Debug.LogError("AdvancedDeathController: AllPigs изображение не назначено!");
        }
    }
    
    private void UpdateStatisticsUI()
    {
        GameStatsTracker statsTracker = GameStatsTracker.Instance;
        if (statsTracker != null)
        {
            // Show statistics when death occurs
            statsTracker.ShowStatistics();
            
            string speedText = $"Макс. скорость: {statsTracker.MaxSpeedReached:F1}";
            string itemsText = $"Съедено предметов: {statsTracker.TotalItemsEaten}";
            
            // Update max speed text (regular Text)
            if (maxSpeedText != null)
            {
                maxSpeedText.text = speedText;
                if (showDebugInfo)
                {
                    Debug.Log($"AdvancedDeathController: Обновлен текст скорости: {statsTracker.MaxSpeedReached:F1}");
                }
            }
            
            // Update max speed text (TextMeshPro)
            if (maxSpeedTextTMP != null)
            {
                maxSpeedTextTMP.text = speedText;
                if (showDebugInfo)
                {
                    Debug.Log($"AdvancedDeathController: Обновлен TextMeshPro скорости: {statsTracker.MaxSpeedReached:F1}");
                }
            }
            
            // Update items eaten text (regular Text)
            if (itemsEatenText != null)
            {
                itemsEatenText.text = itemsText;
                if (showDebugInfo)
                {
                    Debug.Log($"AdvancedDeathController: Обновлен текст предметов: {statsTracker.TotalItemsEaten}");
                }
            }
            
            // Update items eaten text (TextMeshPro)
            if (itemsEatenTextTMP != null)
            {
                itemsEatenTextTMP.text = itemsText;
                if (showDebugInfo)
                {
                    Debug.Log($"AdvancedDeathController: Обновлен TextMeshPro предметов: {statsTracker.TotalItemsEaten}");
                }
            }
        }
        else
        {
            Debug.LogWarning("AdvancedDeathController: GameStatsTracker не найден для обновления статистики");
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
                Debug.Log("AdvancedDeathController: Запущена музыка смерти");
            }
        }
        else if (hasPlayedDeathMusic)
        {
            if (showDebugInfo)
            {
                Debug.Log("AdvancedDeathController: Музыка смерти уже была проиграна");
            }
        }
        else
        {
            Debug.LogWarning("AdvancedDeathController: AudioSource или аудиоклип не назначены!");
        }
    }
    
    // Public methods for external control
    public void HideDeathUI()
    {
        if (allPigsImage != null)
        {
            allPigsImage.SetActive(false);
            Debug.Log("AdvancedDeathController: AllPigs изображение скрыто");
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.gameObject.SetActive(false);
            Debug.Log("AdvancedDeathController: Кнопка главного меню скрыта");
        }
    }
    
    public void LoadMainMenu()
    {
        if (showDebugInfo)
        {
            Debug.Log($"AdvancedDeathController: Загружаю главное меню: {mainMenuSceneName}");
        }
        
        // Stop death music before loading scene
        StopDeathMusic();
        
        // ВАЖНО: НЕ восстанавливаем курсор, оставляем его видимым для главного меню
        // RestoreCursor(); // Закомментировано - пусть MainMenuManager сам настроит курсор
        
        // Ensure cursor is visible and unlocked for main menu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        if (showDebugInfo)
        {
            Debug.Log($"AdvancedDeathController: Курсор подготовлен для главного меню - Visible: {Cursor.visible}, LockState: {Cursor.lockState}");
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
                    Debug.Log($"AdvancedDeathController: Загружаю сцену по имени: {mainMenuSceneName}");
                    UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
                    return;
                }
                else
                {
                    Debug.LogWarning($"AdvancedDeathController: Сцена '{mainMenuSceneName}' не может быть загружена по имени");
                }
            }
            
            // Second try: Load by build index (MainMenu should be at index 0 or 1)
            Debug.Log("AdvancedDeathController: Пытаюсь загрузить главное меню по индексу...");
            
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
                        Debug.Log($"AdvancedDeathController: Найдена сцена главного меню по индексу {i}: {sceneName}");
                        UnityEngine.SceneManagement.SceneManager.LoadScene(i);
                        return;
                    }
                }
                
                // If no MainMenu found, try index 0 (usually main menu)
                Debug.Log("AdvancedDeathController: Загружаю сцену по индексу 0 (предполагаемое главное меню)");
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
                return;
            }
            
            // If all else fails
            Debug.LogError("AdvancedDeathController: Не удалось найти подходящую сцену для загрузки!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"AdvancedDeathController: Критическая ошибка загрузки сцены: {e.Message}");
            Debug.LogError("AdvancedDeathController: Проверьте что сцены добавлены в Build Settings!");
            
            // Last resort: try to quit application or restart current scene
            Debug.LogError("AdvancedDeathController: Попытка перезагрузки текущей сцены как последний вариант...");
            try
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
            catch (System.Exception restartException)
            {
                Debug.LogError($"AdvancedDeathController: Не удалось перезагрузить сцену: {restartException.Message}");
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
        
        Debug.Log("AdvancedDeathController: Состояние смерти сброшено");
    }
    
    public void StopDeathMusic()
    {
        if (deathAudioSource != null && deathAudioSource.isPlaying)
        {
            deathAudioSource.Stop();
            Debug.Log("AdvancedDeathController: Остановлена музыка смерти");
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
        Debug.Log("=== НАСТРОЙКА ПОЛНОЙ СИСТЕМЫ СМЕРТИ ===");
        
        // Auto find all components
        AutoFindComponents();
        
        // Setup UI
        SetupDeathUI();
        
        // Subscribe to death event
        SubscribeToDeathEvent();
        
        Debug.Log("=== СИСТЕМА СМЕРТИ НАСТРОЕНА ===");
    }
    
    [ContextMenu("Test Enable Cursor")]
    public void TestEnableCursor()
    {
        EnableCursor();
        Debug.Log($"AdvancedDeathController: Тест курсора - Visible: {Cursor.visible}, LockState: {Cursor.lockState}");
    }
    
    [ContextMenu("Test Restore Cursor")]
    public void TestRestoreCursor()
    {
        RestoreCursor();
        Debug.Log($"AdvancedDeathController: Курсор восстановлен - Visible: {Cursor.visible}, LockState: {Cursor.lockState}");
    }
    
    [ContextMenu("Test Load Main Menu")]
    public void TestLoadMainMenu()
    {
        Debug.Log("AdvancedDeathController: Тестирование загрузки главного меню...");
        LoadMainMenu();
    }
    
    [ContextMenu("Diagnose Scene Loading")]
    public void DiagnoseSceneLoading()
    {
        Debug.Log("=== ДИАГНОСТИКА ЗАГРУЗКИ СЦЕН ===");
        
        Debug.Log($"Текущая сцена: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        Debug.Log($"Целевая сцена главного меню: '{mainMenuSceneName}'");
        Debug.Log($"Сцен в Build Settings: {UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings}");
        
        // Check if target scene can be loaded
        bool canLoad = !string.IsNullOrEmpty(mainMenuSceneName) && Application.CanStreamedLevelBeLoaded(mainMenuSceneName);
        Debug.Log($"Можно загрузить '{mainMenuSceneName}': {canLoad}");
        
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
            Debug.Log($"Кнопка главного меню: {mainMenuButton.name}");
            Debug.Log($"Кнопка активна: {mainMenuButton.gameObject.activeInHierarchy}");
            Debug.Log($"Слушателей у кнопки: {mainMenuButton.onClick.GetPersistentEventCount()}");
        }
        else
        {
            Debug.LogWarning("Кнопка главного меню НЕ НАЗНАЧЕНА!");
        }
        
        Debug.Log("=== КОНЕЦ ДИАГНОСТИКИ ===");
    }
    
    [ContextMenu("Force Create New Main Menu Button")]
    public void ForceCreateNewMainMenuButton()
    {
        Debug.Log("AdvancedDeathController: Принудительное создание новой кнопки главного меню...");
        
        // Destroy existing button if any
        if (mainMenuButton != null)
        {
            DestroyImmediate(mainMenuButton.gameObject);
            mainMenuButton = null;
            Debug.Log("Старая кнопка удалена");
        }
        
        // Create new button
        CreateMainMenuButton();
        
        // Setup the button
        if (mainMenuButton != null)
        {
            SetupDeathUI();
            Debug.Log("Новая кнопка создана и настроена");
        }
        else
        {
            Debug.LogError("Не удалось создать новую кнопку!");
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
                Debug.Log("AdvancedDeathController: Курсор включён для взаимодействия с UI");
            }
        }
    }
    
    private void RestoreCursor()
    {
        Cursor.visible = originalCursorVisible;
        Cursor.lockState = originalCursorLockState;
        
        if (showDebugInfo)
        {
            Debug.Log("AdvancedDeathController: Состояние курсора восстановлено");
        }
    }
} 