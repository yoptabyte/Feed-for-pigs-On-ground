using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DeathSystemDiagnostics : MonoBehaviour
{
    [Header("Diagnostic Settings")]
    [Tooltip("Show detailed diagnostics")]
    public bool enableDiagnostics = true;
    
    [Tooltip("Diagnostics interval in seconds")]
    public float diagnosticInterval = 2f;
    
    private float lastDiagnosticTime;
    
    void Start()
    {
        if (enableDiagnostics)
        {
            Debug.Log("=== DEATH SYSTEM DIAGNOSTICS ===");
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
        Debug.Log("=== FULL DEATH SYSTEM DIAGNOSTIC ===");
        
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
        
        Debug.Log("=== DIAGNOSTICS COMPLETE ===");
    }
    
    [ContextMenu("Quick Diagnostic")]
    public void PerformQuickDiagnostic()
    {
        GameStatsTracker statsTracker = GameStatsTracker.Instance;
        if (statsTracker != null)
        {
            Debug.Log($"[DIAGNOSTICS] Max speed: {statsTracker.MaxSpeedReached:F1}, Items: {statsTracker.TotalItemsEaten}");
        }
    }
    
    private void CheckGameStatsTracker()
    {
        GameStatsTracker statsTracker = GameStatsTracker.Instance;
        
        if (statsTracker == null)
        {
            Debug.LogError("[DIAGNOSTICS] GameStatsTracker.Instance = NULL! Singleton not created.");
            return;
        }
        
        Debug.Log($"[DIAGNOSTICS] GameStatsTracker found: {statsTracker.name}");
        Debug.Log($"[DIAGNOSTICS] - Max speed: {statsTracker.MaxSpeedReached:F1}");
        Debug.Log($"[DIAGNOSTICS] - Collected items: {statsTracker.TotalItemsEaten}");
        Debug.Log($"[DIAGNOSTICS] - Hidden at start: {statsTracker.hideStatsAtStart}");
        
        // Check UI references
        bool hasSpeedUI = statsTracker.maxSpeedText != null || statsTracker.maxSpeedTextTMP != null;
        bool hasItemsUI = statsTracker.itemsEatenText != null || statsTracker.itemsEatenTextTMP != null;
        
        Debug.Log($"[DIAGNOSTICS] - UI speed assigned: {hasSpeedUI}");
        Debug.Log($"[DIAGNOSTICS] - UI items assigned: {hasItemsUI}");
        
        if (statsTracker.maxSpeedTextTMP != null)
        {
            Debug.Log($"[DIAGNOSTICS] - MaxSpeedTextTMP: {statsTracker.maxSpeedTextTMP.name}, active: {statsTracker.maxSpeedTextTMP.gameObject.activeInHierarchy}, text: '{statsTracker.maxSpeedTextTMP.text}'");
        }
        
        if (statsTracker.itemsEatenTextTMP != null)
        {
            Debug.Log($"[DIAGNOSTICS] - ItemsEatenTextTMP: {statsTracker.itemsEatenTextTMP.name}, active: {statsTracker.itemsEatenTextTMP.gameObject.activeInHierarchy}, text: '{statsTracker.itemsEatenTextTMP.text}'");
        }
    }
    
    private void CheckAdvancedDeathController()
    {
        AdvancedDeathController deathController = FindObjectOfType<AdvancedDeathController>();
        
        if (deathController == null)
        {
            Debug.LogWarning("[DIAGNOSTICS] AdvancedDeathController not found!");
            return;
        }
        
        Debug.Log($"[DIAGNOSTICS] AdvancedDeathController found: {deathController.name}");
        Debug.Log($"[DIAGNOSTICS] - Auto find enabled: {deathController.autoFindComponents}");
        Debug.Log($"[DIAGNOSTICS] - Debug enabled: {deathController.showDebugInfo}");
        Debug.Log($"[DIAGNOSTICS] - Health found: {deathController.playerHealth != null}");
        Debug.Log($"[DIAGNOSTICS] - AllPigs image: {deathController.allPigsImage != null}");
        Debug.Log($"[DIAGNOSTICS] - Main menu button: {deathController.mainMenuButton != null}");
        Debug.Log($"[DIAGNOSTICS] - Show main menu button: {deathController.showMainMenuButton}");
        Debug.Log($"[DIAGNOSTICS] - Create main menu button: {deathController.createMainMenuButton}");
        Debug.Log($"[DIAGNOSTICS] - Main menu scene: {deathController.mainMenuSceneName}");
        Debug.Log($"[DIAGNOSTICS] - Death delay: {deathController.deathDelaySeconds} sec");
        Debug.Log($"[DIAGNOSTICS] - Enable cursor on death: {deathController.enableCursorOnDeath}");
        Debug.Log($"[DIAGNOSTICS] - Unlock cursor: {deathController.unlockCursorOnDeath}");
        Debug.Log($"[DIAGNOSTICS] - Current cursor: Visible={Cursor.visible}, LockState={Cursor.lockState}");
        
        if (deathController.playerHealth != null)
        {
            Debug.Log($"[DIAGNOSTICS] - Player Health: {deathController.playerHealth.gameObject.name}");
        }
        
        if (deathController.mainMenuButton != null)
        {
            Debug.Log($"[DIAGNOSTICS] - Main menu button: {deathController.mainMenuButton.name}, active: {deathController.mainMenuButton.gameObject.activeInHierarchy}");
        }
    }
    
    private void CheckSpeedTracker()
    {
        SpeedTracker[] speedTrackers = FindObjectsOfType<SpeedTracker>();
        
        if (speedTrackers.Length == 0)
        {
            Debug.LogWarning("[DIAGNOSTICS] SpeedTracker not found! Speed not tracked.");
            return;
        }
        
        Debug.Log($"[DIAGNOSTICS] Found SpeedTracker: {speedTrackers.Length}");
        
        foreach (SpeedTracker tracker in speedTrackers)
        {
            Debug.Log($"[DIAGNOSTICS] - SpeedTracker on: {tracker.gameObject.name}");
            Debug.Log($"[DIAGNOSTICS] - - Debug: {tracker.showDebugInfo}");
            Debug.Log($"[DIAGNOSTICS] - - Interval: {tracker.updateInterval}");
            Debug.Log($"[DIAGNOSTICS] - - Min threshold: {tracker.minimumSpeedThreshold}");
            
            // Check if Rigidbody exists
            Rigidbody rb = tracker.GetComponent<Rigidbody>();
            if (rb == null) rb = tracker.GetComponentInChildren<Rigidbody>();
            if (rb == null) rb = tracker.GetComponentInParent<Rigidbody>();
            
            if (rb != null)
            {
                Debug.Log($"[DIAGNOSTICS] - - Rigidbody found, speed: {rb.linearVelocity.magnitude:F1}");
            }
            else
            {
                Debug.LogError($"[DIAGNOSTICS] - - Rigidbody NOT FOUND on {tracker.name}!");
            }
        }
    }
    
    private void CheckUIElements()
    {
        Debug.Log("[DIAGNOSTICS] Checking UI elements...");
        
        // Find all Text and TextMeshPro components
        Text[] allTexts = FindObjectsOfType<Text>(true); // Include inactive
        TextMeshProUGUI[] allTextsTMP = FindObjectsOfType<TextMeshProUGUI>(true); // Include inactive
        
        Debug.Log($"[DIAGNOSTICS] Found Text components: {allTexts.Length}");
        Debug.Log($"[DIAGNOSTICS] Found TextMeshPro components: {allTextsTMP.Length}");
        
        // Check for speed-related UI
        foreach (Text text in allTexts)
        {
            string name = text.name.ToLower();
            if (name.Contains("speed") || name.Contains("velocity") || name.Contains("max"))
            {
                Debug.Log($"[DIAGNOSTICS] Speed Text found: {text.name}, active: {text.gameObject.activeInHierarchy}, parent active: {text.transform.parent?.gameObject.activeInHierarchy}, text: '{text.text}'");
            }
        }
        
        foreach (TextMeshProUGUI textTMP in allTextsTMP)
        {
            string name = textTMP.name.ToLower();
            if (name.Contains("speed") || name.Contains("velocity") || name.Contains("max"))
            {
                Debug.Log($"[DIAGNOSTICS] Speed TextMeshPro found: {textTMP.name}, active: {textTMP.gameObject.activeInHierarchy}, parent active: {textTMP.transform.parent?.gameObject.activeInHierarchy}, text: '{textTMP.text}'");
            }
        }
        
        // Check for items-related UI
        foreach (Text text in allTexts)
        {
            string name = text.name.ToLower();
            if (name.Contains("item") || name.Contains("item") || name.Contains("eaten"))
            {
                Debug.Log($"[DIAGNOSTICS] Items Text found: {text.name}, active: {text.gameObject.activeInHierarchy}, parent active: {text.transform.parent?.gameObject.activeInHierarchy}, text: '{text.text}'");
            }
        }
        
        foreach (TextMeshProUGUI textTMP in allTextsTMP)
        {
            string name = textTMP.name.ToLower();
            if (name.Contains("item") || name.Contains("item") || name.Contains("eaten"))
            {
                Debug.Log($"[DIAGNOSTICS] Items TextMeshPro found: {textTMP.name}, active: {textTMP.gameObject.activeInHierarchy}, parent active: {textTMP.transform.parent?.gameObject.activeInHierarchy}, text: '{textTMP.text}'");
            }
        }
        
        // Check Canvas
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        Debug.Log($"[DIAGNOSTICS] Found Canvas: {allCanvases.Length}");
        foreach (Canvas canvas in allCanvases)
        {
            Debug.Log($"[DIAGNOSTICS] Canvas: {canvas.name}, active: {canvas.gameObject.activeInHierarchy}, render mode: {canvas.renderMode}");
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
                Debug.Log($"[DIAGNOSTICS] Player Health found by tag: {playerByTag.name}");
            }
        }
        
        // Method 2: By name
        if (playerHealth == null)
        {
            Health[] allHealthComponents = FindObjectsOfType<Health>();
            foreach (Health health in allHealthComponents)
            {
                string name = health.gameObject.name.ToLower();
                if (name.Contains("pig") || name.Contains("player") || name.Contains("pig"))
                {
                    playerHealth = health;
                    Debug.Log($"[DIAGNOSTICS] Player Health found by name: {health.gameObject.name}");
                    break;
                }
            }
        }
        
        if (playerHealth == null)
        {
            Debug.LogError("[DIAGNOSTICS] Player Health NOT FOUND!");
            return;
        }
        
        Debug.Log($"[DIAGNOSTICS] Player Health: {playerHealth.CurrentHP}/{playerHealth.MaxHP} HP");
        Debug.Log($"[DIAGNOSTICS] - Alive: {playerHealth.IsAlive}");
    }
    
    [ContextMenu("Test Speed Update")]
    public void TestSpeedUpdate()
    {
        GameStatsTracker statsTracker = GameStatsTracker.Instance;
        if (statsTracker != null)
        {
            float testSpeed = Random.Range(10f, 50f);
            statsTracker.UpdateMaxSpeed(testSpeed);
            Debug.Log($"[TEST] Test speed set: {testSpeed:F1}");
        }
        else
        {
            Debug.LogError("[TEST] GameStatsTracker not found for speed test!");
        }
    }
    
    [ContextMenu("Test Add Item")]
    public void TestAddItem()
    {
        GameStatsTracker statsTracker = GameStatsTracker.Instance;
        if (statsTracker != null)
        {
            statsTracker.AddItemEaten("TestItem");
            Debug.Log("[TEST] Test item added");
        }
        else
        {
            Debug.LogError("[TEST] GameStatsTracker not found for test item!");
        }
    }
    
    [ContextMenu("Show Statistics")]
    public void TestShowStatistics()
    {
        GameStatsTracker statsTracker = GameStatsTracker.Instance;
        if (statsTracker != null)
        {
            statsTracker.ShowStatistics();
            Debug.Log("[TEST] Statistics shown");
        }
        else
        {
            Debug.LogError("[TEST] GameStatsTracker not found!");
        }
    }
    
    [ContextMenu("Test Death")]
    public void TestDeath()
    {
        AdvancedDeathController deathController = FindObjectOfType<AdvancedDeathController>();
        if (deathController != null)
        {
            deathController.TestDeath();
            Debug.Log("[TEST] Death simulated");
        }
        else
        {
            Debug.LogError("[TEST] AdvancedDeathController not found!");
        }
    }
    
    [ContextMenu("Test Death With Full Diagnostic")]
    public void TestDeathWithDiagnostic()
    {
        Debug.Log("=== TESTING DEATH WITH DIAGNOSTICS ===");
        
        // Step 1: Add some test data
        GameStatsTracker statsTracker = GameStatsTracker.Instance;
        if (statsTracker != null)
        {
            Debug.Log("[TEST] Adding test data...");
            statsTracker.UpdateMaxSpeed(25.7f);
            statsTracker.AddItemEaten("Apple");
            statsTracker.AddItemEaten("Berry");
            Debug.Log($"[TEST] Test data: speed {statsTracker.MaxSpeedReached:F1}, items {statsTracker.TotalItemsEaten}");
        }
        
        // Step 2: Check UI before death
        Debug.Log("[TEST] Checking UI before death:");
        CheckStatisticsVisibility();
        
        // Step 3: Trigger death
        AdvancedDeathController deathController = FindObjectOfType<AdvancedDeathController>();
        if (deathController != null)
        {
            Debug.Log("[TEST] Simulating death...");
            deathController.TestDeath();
            
            // Wait a frame and check again
            StartCoroutine(CheckDeathAfterDelay());
        }
        else
        {
            Debug.LogError("[TEST] AdvancedDeathController not found!");
        }
    }
    
    private System.Collections.IEnumerator CheckDeathAfterDelay()
    {
        yield return new WaitForSeconds(0.1f); // Short delay to allow death to process
        
        Debug.Log("[TEST] Checking UI immediately after death:");
        CheckStatisticsVisibility();
        
        yield return new WaitForSeconds(4.5f); // Wait for death delay + buffer
        
        Debug.Log("[TEST] Checking UI after death delay:");
        CheckStatisticsVisibility();
    }
    
    private void CheckStatisticsVisibility()
    {
        GameStatsTracker statsTracker = GameStatsTracker.Instance;
        if (statsTracker == null)
        {
            Debug.LogError("[VISIBILITY] GameStatsTracker not found!");
            return;
        }
        
        // Check regular Text components
        if (statsTracker.maxSpeedText != null)
        {
            bool visible = statsTracker.maxSpeedText.gameObject.activeInHierarchy;
            Debug.Log($"[VISIBILITY] MaxSpeedText: active={visible}, text='{statsTracker.maxSpeedText.text}'");
        }
        else
        {
            Debug.Log("[VISIBILITY] MaxSpeedText: NOT ASSIGNED");
        }
        
        if (statsTracker.itemsEatenText != null)
        {
            bool visible = statsTracker.itemsEatenText.gameObject.activeInHierarchy;
            Debug.Log($"[VISIBILITY] ItemsEatenText: active={visible}, text='{statsTracker.itemsEatenText.text}'");
        }
        else
        {
            Debug.Log("[VISIBILITY] ItemsEatenText: NOT ASSIGNED");
        }
        
        // Check TextMeshPro components
        if (statsTracker.maxSpeedTextTMP != null)
        {
            bool visible = statsTracker.maxSpeedTextTMP.gameObject.activeInHierarchy;
            Debug.Log($"[VISIBILITY] MaxSpeedTextTMP: active={visible}, text='{statsTracker.maxSpeedTextTMP.text}'");
        }
        else
        {
            Debug.Log("[VISIBILITY] MaxSpeedTextTMP: NOT ASSIGNED");
        }
        
        if (statsTracker.itemsEatenTextTMP != null)
        {
            bool visible = statsTracker.itemsEatenTextTMP.gameObject.activeInHierarchy;
            Debug.Log($"[VISIBILITY] ItemsEatenTextTMP: active={visible}, text='{statsTracker.itemsEatenTextTMP.text}'");
        }
        else
        {
            Debug.Log("[VISIBILITY] ItemsEatenTextTMP: NOT ASSIGNED");
        }
    }
    
    [ContextMenu("Force Show Statistics")]
    public void ForceShowStatistics()
    {
        GameStatsTracker statsTracker = GameStatsTracker.Instance;
        if (statsTracker != null)
        {
            Debug.Log("[FORCE] Showing statistics...");
            
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
            
            Debug.Log("[FORCE] Statistics shown!");
            CheckStatisticsVisibility();
        }
        else
        {
            Debug.LogError("[FORCE] GameStatsTracker not found!");
        }
    }
    
    [ContextMenu("Quick Setup Death System")]
    public void QuickSetupDeathSystem()
    {
        Debug.Log("=== QUICK DEATH SYSTEM SETUP ===");
        
        // Find or create AdvancedDeathController
        AdvancedDeathController deathController = FindObjectOfType<AdvancedDeathController>();
        if (deathController == null)
        {
            GameObject deathGO = new GameObject("AdvancedDeathController");
            deathController = deathGO.AddComponent<AdvancedDeathController>();
            Debug.Log("[SETUP] Created AdvancedDeathController");
        }
        
        // Setup the system
        deathController.SetupCompleteDeathSystem();
        
        // Find or create GameStatsTracker
        GameStatsTracker statsTracker = GameStatsTracker.Instance;
        if (statsTracker == null)
        {
            GameObject statsGO = new GameObject("GameStatsTracker");
            statsTracker = statsGO.AddComponent<GameStatsTracker>();
            Debug.Log("[SETUP] Created GameStatsTracker");
        }
        
        Debug.Log("=== QUICK DEATH SYSTEM SETUP COMPLETE ===");
        
        // Run diagnostic
        PerformFullDiagnostic();
    }
} 