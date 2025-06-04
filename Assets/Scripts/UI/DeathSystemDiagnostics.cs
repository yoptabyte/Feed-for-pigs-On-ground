using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DeathSystemDiagnostics : MonoBehaviour
{
    [Header("Diagnostic Settings")]
    [Tooltip("Показывать подробную диагностику")]
    public bool enableDiagnostics = true;
    
    [Tooltip("Интервал диагностики в секундах")]
    public float diagnosticInterval = 2f;
    
    private float lastDiagnosticTime;
    
    void Start()
    {
        if (enableDiagnostics)
        {
            Debug.Log("=== ДИАГНОСТИКА СИСТЕМЫ СМЕРТИ ===");
            PerformFullDiagnostic();
        }
    }
    
    void Update()
    {
        if (enableDiagnostics && Time.time - lastDiagnosticTime >= diagnosticInterval)
        {
            PerformQuickDiagnostic();
            lastDiagnosticTime = Time.time;
        }
    }
    
    [ContextMenu("Full Diagnostic")]
    public void PerformFullDiagnostic()
    {
        Debug.Log("=== ПОЛНАЯ ДИАГНОСТИКА СИСТЕМЫ СМЕРТИ ===");
        
        // Check GameStatsTracker
        CheckGameStatsTracker();
        
        // Check AdvancedDeathController
        CheckAdvancedDeathController();
        
        // Check SpeedTracker
        CheckSpeedTracker();
        
        // Check UI Elements
        CheckUIElements();
        
        // Check Player Health
        CheckPlayerHealth();
        
        Debug.Log("=== ДИАГНОСТИКА ЗАВЕРШЕНА ===");
    }
    
    [ContextMenu("Quick Diagnostic")]
    public void PerformQuickDiagnostic()
    {
        GameStatsTracker statsTracker = GameStatsTracker.Instance;
        if (statsTracker != null)
        {
            Debug.Log($"[ДИАГНОСТИКА] Макс. скорость: {statsTracker.MaxSpeedReached:F1}, Предметов: {statsTracker.TotalItemsEaten}");
        }
    }
    
    private void CheckGameStatsTracker()
    {
        GameStatsTracker statsTracker = GameStatsTracker.Instance;
        
        if (statsTracker == null)
        {
            Debug.LogError("[ДИАГНОСТИКА] GameStatsTracker.Instance = NULL! Singleton не создан.");
            return;
        }
        
        Debug.Log($"[ДИАГНОСТИКА] GameStatsTracker найден: {statsTracker.name}");
        Debug.Log($"[ДИАГНОСТИКА] - Макс. скорость: {statsTracker.MaxSpeedReached:F1}");
        Debug.Log($"[ДИАГНОСТИКА] - Съедено предметов: {statsTracker.TotalItemsEaten}");
        Debug.Log($"[ДИАГНОСТИКА] - Скрыта на старте: {statsTracker.hideStatsAtStart}");
        
        // Check UI references
        bool hasSpeedUI = statsTracker.maxSpeedText != null || statsTracker.maxSpeedTextTMP != null;
        bool hasItemsUI = statsTracker.itemsEatenText != null || statsTracker.itemsEatenTextTMP != null;
        
        Debug.Log($"[ДИАГНОСТИКА] - UI скорости назначен: {hasSpeedUI}");
        Debug.Log($"[ДИАГНОСТИКА] - UI предметов назначен: {hasItemsUI}");
        
        if (statsTracker.maxSpeedTextTMP != null)
        {
            Debug.Log($"[ДИАГНОСТИКА] - MaxSpeedTextTMP: {statsTracker.maxSpeedTextTMP.name}, активен: {statsTracker.maxSpeedTextTMP.gameObject.activeInHierarchy}, текст: '{statsTracker.maxSpeedTextTMP.text}'");
        }
        
        if (statsTracker.itemsEatenTextTMP != null)
        {
            Debug.Log($"[ДИАГНОСТИКА] - ItemsEatenTextTMP: {statsTracker.itemsEatenTextTMP.name}, активен: {statsTracker.itemsEatenTextTMP.gameObject.activeInHierarchy}, текст: '{statsTracker.itemsEatenTextTMP.text}'");
        }
    }
    
    private void CheckAdvancedDeathController()
    {
        AdvancedDeathController deathController = FindObjectOfType<AdvancedDeathController>();
        
        if (deathController == null)
        {
            Debug.LogWarning("[ДИАГНОСТИКА] AdvancedDeathController не найден!");
            return;
        }
        
        Debug.Log($"[ДИАГНОСТИКА] AdvancedDeathController найден: {deathController.name}");
        Debug.Log($"[ДИАГНОСТИКА] - Автопоиск включён: {deathController.autoFindComponents}");
        Debug.Log($"[ДИАГНОСТИКА] - Отладка включена: {deathController.showDebugInfo}");
        Debug.Log($"[ДИАГНОСТИКА] - Health найден: {deathController.playerHealth != null}");
        Debug.Log($"[ДИАГНОСТИКА] - AllPigs изображение: {deathController.allPigsImage != null}");
        Debug.Log($"[ДИАГНОСТИКА] - Кнопка главного меню: {deathController.mainMenuButton != null}");
        Debug.Log($"[ДИАГНОСТИКА] - Показывать кнопку меню: {deathController.showMainMenuButton}");
        Debug.Log($"[ДИАГНОСТИКА] - Создавать кнопку меню: {deathController.createMainMenuButton}");
        Debug.Log($"[ДИАГНОСТИКА] - Сцена главного меню: {deathController.mainMenuSceneName}");
        Debug.Log($"[ДИАГНОСТИКА] - Задержка смерти: {deathController.deathDelaySeconds} сек");
        Debug.Log($"[ДИАГНОСТИКА] - Включать курсор при смерти: {deathController.enableCursorOnDeath}");
        Debug.Log($"[ДИАГНОСТИКА] - Разблокировать курсор: {deathController.unlockCursorOnDeath}");
        Debug.Log($"[ДИАГНОСТИКА] - Текущий курсор: Visible={Cursor.visible}, LockState={Cursor.lockState}");
        
        if (deathController.playerHealth != null)
        {
            Debug.Log($"[ДИАГНОСТИКА] - Player Health: {deathController.playerHealth.gameObject.name}");
        }
        
        if (deathController.mainMenuButton != null)
        {
            Debug.Log($"[ДИАГНОСТИКА] - Кнопка меню: {deathController.mainMenuButton.name}, активна: {deathController.mainMenuButton.gameObject.activeInHierarchy}");
        }
    }
    
    private void CheckSpeedTracker()
    {
        SpeedTracker[] speedTrackers = FindObjectsOfType<SpeedTracker>();
        
        if (speedTrackers.Length == 0)
        {
            Debug.LogWarning("[ДИАГНОСТИКА] SpeedTracker не найден! Скорость не отслеживается.");
            return;
        }
        
        Debug.Log($"[ДИАГНОСТИКА] Найдено SpeedTracker: {speedTrackers.Length}");
        
        foreach (SpeedTracker tracker in speedTrackers)
        {
            Debug.Log($"[ДИАГНОСТИКА] - SpeedTracker на: {tracker.gameObject.name}");
            Debug.Log($"[ДИАГНОСТИКА] - - Отладка: {tracker.showDebugInfo}");
            Debug.Log($"[ДИАГНОСТИКА] - - Интервал: {tracker.updateInterval}");
            Debug.Log($"[ДИАГНОСТИКА] - - Мин. порог: {tracker.minimumSpeedThreshold}");
            
            // Check if Rigidbody exists
            Rigidbody rb = tracker.GetComponent<Rigidbody>();
            if (rb == null) rb = tracker.GetComponentInChildren<Rigidbody>();
            if (rb == null) rb = tracker.GetComponentInParent<Rigidbody>();
            
            if (rb != null)
            {
                Debug.Log($"[ДИАГНОСТИКА] - - Rigidbody найден, скорость: {rb.linearVelocity.magnitude:F1}");
            }
            else
            {
                Debug.LogError($"[ДИАГНОСТИКА] - - Rigidbody НЕ НАЙДЕН на {tracker.name}!");
            }
        }
    }
    
    private void CheckUIElements()
    {
        Debug.Log("[ДИАГНОСТИКА] Проверка UI элементов...");
        
        // Find all Text and TextMeshPro components
        Text[] allTexts = FindObjectsOfType<Text>(true); // Include inactive
        TextMeshProUGUI[] allTextsTMP = FindObjectsOfType<TextMeshProUGUI>(true); // Include inactive
        
        Debug.Log($"[ДИАГНОСТИКА] Найдено Text компонентов: {allTexts.Length}");
        Debug.Log($"[ДИАГНОСТИКА] Найдено TextMeshPro компонентов: {allTextsTMP.Length}");
        
        // Check for speed-related UI
        foreach (Text text in allTexts)
        {
            string name = text.name.ToLower();
            if (name.Contains("speed") || name.Contains("скорость") || name.Contains("max"))
            {
                Debug.Log($"[ДИАГНОСТИКА] Speed Text найден: {text.name}, активен: {text.gameObject.activeInHierarchy}, parent активен: {text.transform.parent?.gameObject.activeInHierarchy}, текст: '{text.text}'");
            }
        }
        
        foreach (TextMeshProUGUI textTMP in allTextsTMP)
        {
            string name = textTMP.name.ToLower();
            if (name.Contains("speed") || name.Contains("скорость") || name.Contains("max"))
            {
                Debug.Log($"[ДИАГНОСТИКА] Speed TextMeshPro найден: {textTMP.name}, активен: {textTMP.gameObject.activeInHierarchy}, parent активен: {textTMP.transform.parent?.gameObject.activeInHierarchy}, текст: '{textTMP.text}'");
            }
        }
        
        // Check for items-related UI
        foreach (Text text in allTexts)
        {
            string name = text.name.ToLower();
            if (name.Contains("item") || name.Contains("предмет") || name.Contains("съед") || name.Contains("eaten"))
            {
                Debug.Log($"[ДИАГНОСТИКА] Items Text найден: {text.name}, активен: {text.gameObject.activeInHierarchy}, parent активен: {text.transform.parent?.gameObject.activeInHierarchy}, текст: '{text.text}'");
            }
        }
        
        foreach (TextMeshProUGUI textTMP in allTextsTMP)
        {
            string name = textTMP.name.ToLower();
            if (name.Contains("item") || name.Contains("предмет") || name.Contains("съед") || name.Contains("eaten"))
            {
                Debug.Log($"[ДИАГНОСТИКА] Items TextMeshPro найден: {textTMP.name}, активен: {textTMP.gameObject.activeInHierarchy}, parent активен: {textTMP.transform.parent?.gameObject.activeInHierarchy}, текст: '{textTMP.text}'");
            }
        }
        
        // Check Canvas
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        Debug.Log($"[ДИАГНОСТИКА] Найдено Canvas: {allCanvases.Length}");
        foreach (Canvas canvas in allCanvases)
        {
            Debug.Log($"[ДИАГНОСТИКА] Canvas: {canvas.name}, активен: {canvas.gameObject.activeInHierarchy}, render mode: {canvas.renderMode}");
        }
    }
    
    private void CheckPlayerHealth()
    {
        // Try to find player Health
        Health playerHealth = null;
        
        // Method 1: By tag
        GameObject playerByTag = GameObject.FindGameObjectWithTag("Player");
        if (playerByTag != null)
        {
            playerHealth = playerByTag.GetComponent<Health>();
            if (playerHealth != null)
            {
                Debug.Log($"[ДИАГНОСТИКА] Player Health найден по тегу: {playerByTag.name}");
            }
        }
        
        // Method 2: By name
        if (playerHealth == null)
        {
            Health[] allHealthComponents = FindObjectsOfType<Health>();
            foreach (Health health in allHealthComponents)
            {
                string name = health.gameObject.name.ToLower();
                if (name.Contains("pig") || name.Contains("player") || name.Contains("свин"))
                {
                    playerHealth = health;
                    Debug.Log($"[ДИАГНОСТИКА] Player Health найден по имени: {health.gameObject.name}");
                    break;
                }
            }
        }
        
        if (playerHealth == null)
        {
            Debug.LogError("[ДИАГНОСТИКА] Player Health НЕ НАЙДЕН!");
            return;
        }
        
        Debug.Log($"[ДИАГНОСТИКА] Player Health: {playerHealth.CurrentHP}/{playerHealth.MaxHP} HP");
        Debug.Log($"[ДИАГНОСТИКА] - Жив: {playerHealth.IsAlive}");
    }
    
    [ContextMenu("Test Speed Update")]
    public void TestSpeedUpdate()
    {
        GameStatsTracker statsTracker = GameStatsTracker.Instance;
        if (statsTracker != null)
        {
            float testSpeed = Random.Range(10f, 50f);
            statsTracker.UpdateMaxSpeed(testSpeed);
            Debug.Log($"[ТЕСТ] Установлена тестовая скорость: {testSpeed:F1}");
        }
        else
        {
            Debug.LogError("[ТЕСТ] GameStatsTracker не найден для теста скорости!");
        }
    }
    
    [ContextMenu("Test Add Item")]
    public void TestAddItem()
    {
        GameStatsTracker statsTracker = GameStatsTracker.Instance;
        if (statsTracker != null)
        {
            statsTracker.AddItemEaten("TestItem");
            Debug.Log("[ТЕСТ] Добавлен тестовый предмет");
        }
        else
        {
            Debug.LogError("[ТЕСТ] GameStatsTracker не найден для теста предмета!");
        }
    }
    
    [ContextMenu("Show Statistics")]
    public void TestShowStatistics()
    {
        GameStatsTracker statsTracker = GameStatsTracker.Instance;
        if (statsTracker != null)
        {
            statsTracker.ShowStatistics();
            Debug.Log("[ТЕСТ] Статистика показана");
        }
        else
        {
            Debug.LogError("[ТЕСТ] GameStatsTracker не найден!");
        }
    }
    
    [ContextMenu("Test Death")]
    public void TestDeath()
    {
        AdvancedDeathController deathController = FindObjectOfType<AdvancedDeathController>();
        if (deathController != null)
        {
            deathController.TestDeath();
            Debug.Log("[ТЕСТ] Смерть симулирована");
        }
        else
        {
            Debug.LogError("[ТЕСТ] AdvancedDeathController не найден!");
        }
    }
    
    [ContextMenu("Test Death With Full Diagnostic")]
    public void TestDeathWithDiagnostic()
    {
        Debug.Log("=== ТЕСТИРОВАНИЕ СМЕРТИ С ДИАГНОСТИКОЙ ===");
        
        // Step 1: Add some test data
        GameStatsTracker statsTracker = GameStatsTracker.Instance;
        if (statsTracker != null)
        {
            Debug.Log("[ТЕСТ] Добавляю тестовые данные...");
            statsTracker.UpdateMaxSpeed(25.7f);
            statsTracker.AddItemEaten("Apple");
            statsTracker.AddItemEaten("Berry");
            Debug.Log($"[ТЕСТ] Тестовые данные: скорость {statsTracker.MaxSpeedReached:F1}, предметов {statsTracker.TotalItemsEaten}");
        }
        
        // Step 2: Check UI before death
        Debug.Log("[ТЕСТ] Проверка UI до смерти:");
        CheckStatisticsVisibility();
        
        // Step 3: Trigger death
        AdvancedDeathController deathController = FindObjectOfType<AdvancedDeathController>();
        if (deathController != null)
        {
            Debug.Log("[ТЕСТ] Симулирую смерть...");
            deathController.TestDeath();
            
            // Wait a frame and check again
            StartCoroutine(CheckDeathAfterDelay());
        }
        else
        {
            Debug.LogError("[ТЕСТ] AdvancedDeathController не найден!");
        }
    }
    
    private System.Collections.IEnumerator CheckDeathAfterDelay()
    {
        yield return new WaitForSeconds(0.1f); // Short delay to allow death to process
        
        Debug.Log("[ТЕСТ] Проверка UI сразу после смерти:");
        CheckStatisticsVisibility();
        
        yield return new WaitForSeconds(4.5f); // Wait for death delay + buffer
        
        Debug.Log("[ТЕСТ] Проверка UI после задержки смерти:");
        CheckStatisticsVisibility();
    }
    
    private void CheckStatisticsVisibility()
    {
        GameStatsTracker statsTracker = GameStatsTracker.Instance;
        if (statsTracker == null)
        {
            Debug.LogError("[ВИДИМОСТЬ] GameStatsTracker не найден!");
            return;
        }
        
        // Check regular Text components
        if (statsTracker.maxSpeedText != null)
        {
            bool visible = statsTracker.maxSpeedText.gameObject.activeInHierarchy;
            Debug.Log($"[ВИДИМОСТЬ] MaxSpeedText: активен={visible}, текст='{statsTracker.maxSpeedText.text}'");
        }
        else
        {
            Debug.Log("[ВИДИМОСТЬ] MaxSpeedText: НЕ НАЗНАЧЕН");
        }
        
        if (statsTracker.itemsEatenText != null)
        {
            bool visible = statsTracker.itemsEatenText.gameObject.activeInHierarchy;
            Debug.Log($"[ВИДИМОСТЬ] ItemsEatenText: активен={visible}, текст='{statsTracker.itemsEatenText.text}'");
        }
        else
        {
            Debug.Log("[ВИДИМОСТЬ] ItemsEatenText: НЕ НАЗНАЧЕН");
        }
        
        // Check TextMeshPro components
        if (statsTracker.maxSpeedTextTMP != null)
        {
            bool visible = statsTracker.maxSpeedTextTMP.gameObject.activeInHierarchy;
            Debug.Log($"[ВИДИМОСТЬ] MaxSpeedTextTMP: активен={visible}, текст='{statsTracker.maxSpeedTextTMP.text}'");
        }
        else
        {
            Debug.Log("[ВИДИМОСТЬ] MaxSpeedTextTMP: НЕ НАЗНАЧЕН");
        }
        
        if (statsTracker.itemsEatenTextTMP != null)
        {
            bool visible = statsTracker.itemsEatenTextTMP.gameObject.activeInHierarchy;
            Debug.Log($"[ВИДИМОСТЬ] ItemsEatenTextTMP: активен={visible}, текст='{statsTracker.itemsEatenTextTMP.text}'");
        }
        else
        {
            Debug.Log("[ВИДИМОСТЬ] ItemsEatenTextTMP: НЕ НАЗНАЧЕН");
        }
    }
    
    [ContextMenu("Force Show Statistics")]
    public void ForceShowStatistics()
    {
        GameStatsTracker statsTracker = GameStatsTracker.Instance;
        if (statsTracker != null)
        {
            Debug.Log("[ПРИНУДИТЕЛЬНО] Показываю статистику...");
            
            // Add some test data if none exists
            if (statsTracker.MaxSpeedReached == 0f)
            {
                statsTracker.UpdateMaxSpeed(15.3f);
            }
            if (statsTracker.TotalItemsEaten == 0)
            {
                statsTracker.AddItemEaten("TestApple");
            }
            
            // Force show statistics
            statsTracker.ShowStatistics();
            
            Debug.Log("[ПРИНУДИТЕЛЬНО] Статистика показана!");
            CheckStatisticsVisibility();
        }
        else
        {
            Debug.LogError("[ПРИНУДИТЕЛЬНО] GameStatsTracker не найден!");
        }
    }
    
    [ContextMenu("Quick Setup Death System")]
    public void QuickSetupDeathSystem()
    {
        Debug.Log("=== БЫСТРАЯ НАСТРОЙКА СИСТЕМЫ СМЕРТИ ===");
        
        // Find or create AdvancedDeathController
        AdvancedDeathController deathController = FindObjectOfType<AdvancedDeathController>();
        if (deathController == null)
        {
            GameObject deathGO = new GameObject("AdvancedDeathController");
            deathController = deathGO.AddComponent<AdvancedDeathController>();
            Debug.Log("[НАСТРОЙКА] Создан AdvancedDeathController");
        }
        
        // Setup the system
        deathController.SetupCompleteDeathSystem();
        
        // Find or create GameStatsTracker
        GameStatsTracker statsTracker = GameStatsTracker.Instance;
        if (statsTracker == null)
        {
            GameObject statsGO = new GameObject("GameStatsTracker");
            statsTracker = statsGO.AddComponent<GameStatsTracker>();
            Debug.Log("[НАСТРОЙКА] Создан GameStatsTracker");
        }
        
        Debug.Log("=== БЫСТРАЯ НАСТРОЙКА ЗАВЕРШЕНА ===");
        
        // Run diagnostic
        PerformFullDiagnostic();
    }
} 