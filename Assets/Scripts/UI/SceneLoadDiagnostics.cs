using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoadDiagnostics : MonoBehaviour
{
    [Header("Diagnostic Settings")]
    [Tooltip("Показывать подробную диагностику")]
    public bool enableDiagnostics = true;
    
    [Header("Scene Names")]
    [Tooltip("Название сцены главного меню")]
    public string mainMenuSceneName = "MainMenu";
    
    [Tooltip("Название игровой сцены")]
    public string gameSceneName = "New Scene1";
    
    void Start()
    {
        if (enableDiagnostics)
        {
            PerformSceneDiagnostic();
        }
    }
    
    [ContextMenu("Full Scene Diagnostic")]
    public void PerformSceneDiagnostic()
    {
        Debug.Log("=== ДИАГНОСТИКА ЗАГРУЗКИ СЦЕН ===");
        
        // Check current scene
        Scene currentScene = SceneManager.GetActiveScene();
        Debug.Log($"[СЦЕНА] Текущая сцена: {currentScene.name}");
        Debug.Log($"[СЦЕНА] Путь к сцене: {currentScene.path}");
        Debug.Log($"[СЦЕНА] Build Index: {currentScene.buildIndex}");
        
        // Check build settings
        Debug.Log($"[BUILD] Всего сцен в Build Settings: {SceneManager.sceneCountInBuildSettings}");
        
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
        
        Debug.Log("=== ДИАГНОСТИКА ЗАВЕРШЕНА ===");
    }
    
    private void TestSceneLoadCapability(string sceneName)
    {
        Debug.Log($"[ТЕСТ] Проверка возможности загрузки сцены: {sceneName}");
        
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError($"[ТЕСТ] Название сцены пустое!");
            return;
        }
        
        bool canLoad = Application.CanStreamedLevelBeLoaded(sceneName);
        Debug.Log($"[ТЕСТ] - CanStreamedLevelBeLoaded: {canLoad}");
        
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
        
        Debug.Log($"[ТЕСТ] - Найдена в Build Settings: {foundInBuild}");
        if (foundInBuild)
        {
            Debug.Log($"[ТЕСТ] - Build Index: {buildIndex}");
        }
        
        if (!canLoad && !foundInBuild)
        {
            Debug.LogError($"[ТЕСТ] ПРОБЛЕМА: Сцена '{sceneName}' не может быть загружена!");
            Debug.LogError($"[ТЕСТ] Проверьте что сцена добавлена в File > Build Settings");
        }
    }
    
    private void CheckForUIConflicts()
    {
        Debug.Log("[UI] Проверка конфликтов UI...");
        
        // Check for multiple Canvas objects
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        Debug.Log($"[UI] Найдено Canvas объектов: {allCanvases.Length}");
        
        foreach (Canvas canvas in allCanvases)
        {
            Debug.Log($"[UI] Canvas: {canvas.name}, активен: {canvas.gameObject.activeInHierarchy}, render mode: {canvas.renderMode}");
        }
        
        // Check for main menu buttons in current scene
        Button[] allButtons = FindObjectsOfType<Button>(true);
        Debug.Log($"[UI] Найдено кнопок: {allButtons.Length}");
        
        foreach (Button button in allButtons)
        {
            string name = button.name.ToLower();
            if (name.Contains("mainmenu") || name.Contains("main_menu") || name.Contains("menu") || 
                name.Contains("главное") || name.Contains("выход") || name.Contains("exit"))
            {
                Debug.Log($"[UI] Кнопка главного меню найдена: {button.name}, активна: {button.gameObject.activeInHierarchy}");
                
                // Check button listeners
                int listenerCount = button.onClick.GetPersistentEventCount();
                Debug.Log($"[UI] - Количество слушателей: {listenerCount}");
                
                for (int i = 0; i < listenerCount; i++)
                {
                    Object target = button.onClick.GetPersistentTarget(i);
                    string methodName = button.onClick.GetPersistentMethodName(i);
                    Debug.Log($"[UI] - Слушатель {i}: {target?.name}.{methodName}");
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
                Debug.Log($"[UI] Панель/Меню: {obj.name}, активен: {obj.activeInHierarchy}");
            }
        }
    }
    
    private void CheckDeathControllerSettings()
    {
        Debug.Log("[DEATH] Проверка настроек AdvancedDeathController...");
        
        AdvancedDeathController deathController = FindObjectOfType<AdvancedDeathController>();
        if (deathController == null)
        {
            Debug.LogWarning("[DEATH] AdvancedDeathController не найден!");
            return;
        }
        
        Debug.Log($"[DEATH] AdvancedDeathController найден: {deathController.name}");
        Debug.Log($"[DEATH] - Название сцены главного меню: '{deathController.mainMenuSceneName}'");
        Debug.Log($"[DEATH] - Показывать кнопку главного меню: {deathController.showMainMenuButton}");
        Debug.Log($"[DEATH] - Создавать кнопку главного меню: {deathController.createMainMenuButton}");
        Debug.Log($"[DEATH] - Кнопка главного меню назначена: {deathController.mainMenuButton != null}");
        
        if (deathController.mainMenuButton != null)
        {
            Debug.Log($"[DEATH] - Кнопка: {deathController.mainMenuButton.name}, активна: {deathController.mainMenuButton.gameObject.activeInHierarchy}");
            
            // Check if button has correct listener
            int listenerCount = deathController.mainMenuButton.onClick.GetPersistentEventCount();
            Debug.Log($"[DEATH] - Количество слушателей кнопки: {listenerCount}");
            
            for (int i = 0; i < listenerCount; i++)
            {
                Object target = deathController.mainMenuButton.onClick.GetPersistentTarget(i);
                string methodName = deathController.mainMenuButton.onClick.GetPersistentMethodName(i);
                Debug.Log($"[DEATH] - Слушатель {i}: {target?.name}.{methodName}");
            }
        }
        
        // Test scene name validity
        TestSceneLoadCapability(deathController.mainMenuSceneName);
    }
    
    [ContextMenu("Test Load Main Menu")]
    public void TestLoadMainMenu()
    {
        Debug.Log($"[ТЕСТ] Попытка загрузки главного меню: {mainMenuSceneName}");
        
        try
        {
            SceneManager.LoadScene(mainMenuSceneName);
            Debug.Log("[ТЕСТ] Загрузка сцены инициирована успешно");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ТЕСТ] Ошибка загрузки сцены: {e.Message}");
        }
    }
    
    [ContextMenu("Test Load Game Scene")]
    public void TestLoadGameScene()
    {
        Debug.Log($"[ТЕСТ] Попытка загрузки игровой сцены: {gameSceneName}");
        
        try
        {
            SceneManager.LoadScene(gameSceneName);
            Debug.Log("[ТЕСТ] Загрузка сцены инициирована успешно");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ТЕСТ] Ошибка загрузки сцены: {e.Message}");
        }
    }
    
    [ContextMenu("Force Load Main Menu By Index")]
    public void ForceLoadMainMenuByIndex()
    {
        Debug.Log("[ТЕСТ] Принудительная загрузка главного меню по индексу 0");
        
        try
        {
            SceneManager.LoadScene(0);
            Debug.Log("[ТЕСТ] Загрузка по индексу инициирована");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ТЕСТ] Ошибка загрузки по индексу: {e.Message}");
        }
    }
    
    [ContextMenu("Check Scene Manager State")]
    public void CheckSceneManagerState()
    {
        Debug.Log("=== СОСТОЯНИЕ SCENE MANAGER ===");
        
        Debug.Log($"Загруженных сцен: {SceneManager.sceneCount}");
        
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            Debug.Log($"Сцена {i}: {scene.name}, загружена: {scene.isLoaded}, активна: {scene == SceneManager.GetActiveScene()}");
        }
        
        Debug.Log("=== КОНЕЦ СОСТОЯНИЯ ===");
    }
    
    [ContextMenu("Simulate Death Controller Button Click")]
    public void SimulateDeathControllerButtonClick()
    {
        Debug.Log("[СИМУЛЯЦИЯ] Симуляция нажатия кнопки главного меню из AdvancedDeathController");
        
        AdvancedDeathController deathController = FindObjectOfType<AdvancedDeathController>();
        if (deathController != null)
        {
            deathController.LoadMainMenu();
            Debug.Log("[СИМУЛЯЦИЯ] Метод LoadMainMenu() вызван");
        }
        else
        {
            Debug.LogError("[СИМУЛЯЦИЯ] AdvancedDeathController не найден!");
        }
    }
} 