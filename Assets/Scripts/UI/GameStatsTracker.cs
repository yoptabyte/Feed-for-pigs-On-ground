using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameStatsTracker : MonoBehaviour
{
    public static GameStatsTracker Instance { get; private set; }
    
    [Header("Statistics")]
    [SerializeField] private float maxSpeedReached = 0f;
    [SerializeField] private int totalItemsEaten = 0;
    [SerializeField] private float gameTime = 0f;
    [SerializeField] private Dictionary<string, int> itemsEatenByType = new Dictionary<string, int>();
    
    [Header("UI References - Auto Find")]
    [Tooltip("Text component for displaying max speed")]
    public Text maxSpeedText;
    
    [Tooltip("TextMeshPro component for displaying max speed")]
    public TextMeshProUGUI maxSpeedTextTMP;
    
    [Tooltip("Text component for displaying number of eaten items")]
    public Text itemsEatenText;
    
    [Tooltip("TextMeshPro component for displaying number of eaten items")]
    public TextMeshProUGUI itemsEatenTextTMP;
    
    [Tooltip("Text component for displaying game time")]
    public Text gameTimeText;
    
    [Tooltip("TextMeshPro component for displaying game time")]
    public TextMeshProUGUI gameTimeTextTMP;
    
    [Header("Display Settings")]
    [Tooltip("Speed display format")]
    public string speedFormat = "Max speed: {0:F1}";
    
    [Tooltip("Items display format")]
    public string itemsFormat = "Eaten food: {0}";
    
    [Tooltip("Time display format")]
    public string timeFormat = "Time: {0}";
    
    [Tooltip("Hide statistics at start")]
    public bool hideStatsAtStart = true;
    
    [Header("Auto Find Settings")]
    [Tooltip("Auto find UI components")]
    public bool autoFindUIComponents = true;
    
    [Header("Debug")]
    [Tooltip("Show debug information")]
    public bool showDebugInfo = false;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (showDebugInfo)
            {
                Debug.Log("GameStatsTracker: Singleton created");
            }
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.Log("GameStatsTracker: Duplicate instance destroyed");
            }
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if (autoFindUIComponents)
        {
            AutoFindUIComponents();
        }
        
        if (hideStatsAtStart)
        {
            HideStatistics();
        }
        else
        {
            UpdateUI();
        }
    }
    
    private void AutoFindUIComponents()
    {
        if (showDebugInfo)
        {
            Debug.Log("GameStatsTracker: Auto find UI components...");
        }
        
        // Find all Text components in scene
        Text[] allTexts = FindObjectsOfType<Text>();
        TextMeshProUGUI[] allTextsTMP = FindObjectsOfType<TextMeshProUGUI>();
        
        // Try to find speed text (regular Text)
        if (maxSpeedText == null)
        {
            foreach (Text text in allTexts)
            {
                string name = text.name.ToLower();
                if (name.Contains("speed") || name.Contains("velocity") || name.Contains("max"))
                {
                    maxSpeedText = text;
                    if (showDebugInfo)
                    {
                        Debug.Log($"GameStatsTracker: Found Speed Text: {text.name}");
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
                if (name.Contains("speed") || name.Contains("velocity") || name.Contains("max"))
                {
                    maxSpeedTextTMP = textTMP;
                    if (showDebugInfo)
                    {
                        Debug.Log($"GameStatsTracker: Found Speed TextMeshPro: {textTMP.name}");
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
                if (name.Contains("item") || name.Contains("eaten") || name.Contains("food"))
                {
                    itemsEatenText = text;
                    if (showDebugInfo)
                    {
                        Debug.Log($"GameStatsTracker: Found Items Text: {text.name}");
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
                if (name.Contains("item") || name.Contains("eaten") || name.Contains("food"))
                {
                    itemsEatenTextTMP = textTMP;
                    if (showDebugInfo)
                    {
                        Debug.Log($"GameStatsTracker: Found Items TextMeshPro: {textTMP.name}");
                    }
                    break;
                }
            }
        }
        
        // Try to find time text (regular Text)
        if (gameTimeText == null)
        {
            foreach (Text text in allTexts)
            {
                string name = text.name.ToLower();
                if (name.Contains("time") || name.Contains("timer") || name.Contains("duration"))
                {
                    gameTimeText = text;
                    if (showDebugInfo)
                    {
                        Debug.Log($"GameStatsTracker: Found Time Text: {text.name}");
                    }
                    break;
                }
            }
        }
        
        // Try to find time text (TextMeshPro)
        if (gameTimeTextTMP == null)
        {
            foreach (TextMeshProUGUI textTMP in allTextsTMP)
            {
                string name = textTMP.name.ToLower();
                if (name.Contains("time") || name.Contains("timer") || name.Contains("duration"))
                {
                    gameTimeTextTMP = textTMP;
                    if (showDebugInfo)
                    {
                        Debug.Log($"GameStatsTracker: Found Time TextMeshPro: {textTMP.name}");
                    }
                    break;
                }
            }
        }
    }
    
    // Method to set game time
    public void SetGameTime(float timeInSeconds)
    {
        gameTime = timeInSeconds;
        
        if (showDebugInfo)
        {
            Debug.Log($"GameStatsTracker: Game time set to: {FormatTime(gameTime)}");
        }
        
        // Only update UI if statistics are currently visible
        if (!hideStatsAtStart || AreStatisticsVisible())
        {
            UpdateTimeUI();
        }
    }
    
    // Method to update max speed
    public void UpdateMaxSpeed(float currentSpeed)
    {
        if (currentSpeed > maxSpeedReached)
        {
            maxSpeedReached = currentSpeed;
            
            if (showDebugInfo)
            {
                Debug.Log($"GameStatsTracker: New max speed: {maxSpeedReached:F1}");
            }
            
            // Only update UI if statistics are currently visible
            if (!hideStatsAtStart || AreStatisticsVisible())
            {
                UpdateSpeedUI();
            }
        }
    }
    
    // Method to add eaten item
    public void AddItemEaten(string itemName)
    {
        totalItemsEaten++;
        
        // Count by type
        if (itemsEatenByType.ContainsKey(itemName))
        {
            itemsEatenByType[itemName]++;
        }
        else
        {
            itemsEatenByType[itemName] = 1;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"GameStatsTracker: Added item '{itemName}'. Total: {totalItemsEaten}");
        }
        
        // Only update UI if statistics are currently visible
        if (!hideStatsAtStart || AreStatisticsVisible())
        {
            UpdateItemsUI();
        }
    }
    
    // Check if any statistics UI is currently visible
    private bool AreStatisticsVisible()
    {
        bool visible = false;
        
        if (maxSpeedText != null && maxSpeedText.gameObject.activeInHierarchy)
            visible = true;
        if (maxSpeedTextTMP != null && maxSpeedTextTMP.gameObject.activeInHierarchy)
            visible = true;
        if (itemsEatenText != null && itemsEatenText.gameObject.activeInHierarchy)
            visible = true;
        if (itemsEatenTextTMP != null && itemsEatenTextTMP.gameObject.activeInHierarchy)
            visible = true;
        if (gameTimeText != null && gameTimeText.gameObject.activeInHierarchy)
            visible = true;
        if (gameTimeTextTMP != null && gameTimeTextTMP.gameObject.activeInHierarchy)
            visible = true;
            
        return visible;
    }
    
    // Show statistics (called on death or finish)
    public void ShowStatistics()
    {
        SetStatisticsVisibility(true);
        UpdateUI();
        
        if (showDebugInfo)
        {
            Debug.Log("GameStatsTracker: Statistics shown");
            Debug.Log($"GameStatsTracker: Max Speed: {maxSpeedReached:F1}");
            Debug.Log($"GameStatsTracker: Items Eaten: {totalItemsEaten}");
            Debug.Log($"GameStatsTracker: Game Time: {FormatTime(gameTime)}");
        }
    }
    
    // Force show statistics with current data (for finish screen)
    public void ForceShowStatistics()
    {
        // Auto find UI components if not found
        if (autoFindUIComponents)
        {
            AutoFindUIComponents();
        }
        
        // Force visibility
        SetStatisticsVisibility(true);
        
        // Update with current data
        UpdateUI();
        
        // Also try to update finish screen specific elements
        UpdateFinishScreenStatistics();
        
        if (showDebugInfo)
        {
            Debug.Log("GameStatsTracker: Statistics force shown");
            Debug.Log($"GameStatsTracker: Max Speed: {maxSpeedReached:F1}");
            Debug.Log($"GameStatsTracker: Items Eaten: {totalItemsEaten}");
            Debug.Log($"GameStatsTracker: Game Time: {FormatTime(gameTime)}");
        }
    }
    
    // Update finish screen specific statistics
    private void UpdateFinishScreenStatistics()
    {
        // Try to find finish screen specific text elements
        GameObject finishUI = GameObject.Find("FinishUI");
        if (finishUI != null)
        {
            // Update max speed in finish screen
            Transform speedTextTransform = finishUI.transform.Find("MaxSpeedText");
            if (speedTextTransform != null)
            {
                TextMeshProUGUI speedText = speedTextTransform.GetComponent<TextMeshProUGUI>();
                if (speedText != null)
                {
                    speedText.text = $"Max speed: {maxSpeedReached:F1}";
                    speedText.gameObject.SetActive(true);
                }
            }
            
            // Update items eaten in finish screen
            Transform itemsTextTransform = finishUI.transform.Find("ItemsEatenText");
            if (itemsTextTransform != null)
            {
                TextMeshProUGUI itemsText = itemsTextTransform.GetComponent<TextMeshProUGUI>();
                if (itemsText != null)
                {
                    itemsText.text = $"Collected items: {totalItemsEaten}";
                    itemsText.gameObject.SetActive(true);
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log("GameStatsTracker: Finish screen statistics updated");
            }
        }
    }
    
    // Hide statistics
    public void HideStatistics()
    {
        SetStatisticsVisibility(false);
        
        if (showDebugInfo)
        {
            Debug.Log("GameStatsTracker: Statistics hidden");
        }
    }
    
    private void SetStatisticsVisibility(bool visible)
    {
        if (maxSpeedText != null)
            maxSpeedText.gameObject.SetActive(visible);
        if (maxSpeedTextTMP != null)
            maxSpeedTextTMP.gameObject.SetActive(visible);
        if (itemsEatenText != null)
            itemsEatenText.gameObject.SetActive(visible);
        if (itemsEatenTextTMP != null)
            itemsEatenTextTMP.gameObject.SetActive(visible);
        if (gameTimeText != null)
            gameTimeText.gameObject.SetActive(visible);
        if (gameTimeTextTMP != null)
            gameTimeTextTMP.gameObject.SetActive(visible);
    }
    
    // Update UI methods
    private void UpdateSpeedUI()
    {
        string speedText = string.Format(speedFormat, maxSpeedReached);
        
        if (maxSpeedText != null)
        {
            maxSpeedText.text = speedText;
        }
        
        if (maxSpeedTextTMP != null)
        {
            maxSpeedTextTMP.text = speedText;
        }
    }
    
    private void UpdateItemsUI()
    {
        string itemsText = string.Format(itemsFormat, totalItemsEaten);
        
        if (itemsEatenText != null)
        {
            itemsEatenText.text = itemsText;
        }
        
        if (itemsEatenTextTMP != null)
        {
            itemsEatenTextTMP.text = itemsText;
        }
    }
    
    private void UpdateTimeUI()
    {
        string timeText = string.Format(timeFormat, FormatTime(gameTime));
        
        if (gameTimeText != null)
        {
            gameTimeText.text = timeText;
        }
        
        if (gameTimeTextTMP != null)
        {
            gameTimeTextTMP.text = timeText;
        }
    }
    
    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int milliseconds = Mathf.FloorToInt((timeInSeconds * 1000f) % 1000f);
        
        return $"{minutes:00}:{seconds:00}.{milliseconds:000}";
    }
    
    public void UpdateUI()
    {
        UpdateSpeedUI();
        UpdateItemsUI();
        UpdateTimeUI();
    }
    
    // Public getters
    public float MaxSpeedReached => maxSpeedReached;
    public int TotalItemsEaten => totalItemsEaten;
    public float GameTime => gameTime;
    public Dictionary<string, int> ItemsEatenByType => new Dictionary<string, int>(itemsEatenByType);
    
    // Reset methods
    public void ResetStats()
    {
        maxSpeedReached = 0f;
        totalItemsEaten = 0;
        gameTime = 0f;
        itemsEatenByType.Clear();
        
        UpdateUI();
        
        if (showDebugInfo)
        {
            Debug.Log("GameStatsTracker: Statistics reset");
        }
    }
    
    // Context menu methods for testing
    [ContextMenu("Reset All Stats")]
    public void TestResetStats()
    {
        ResetStats();
    }
    
    [ContextMenu("Add Test Item")]
    public void TestAddItem()
    {
        AddItemEaten("TestItem");
    }
    
    [ContextMenu("Update Test Speed")]
    public void TestUpdateSpeed()
    {
        UpdateMaxSpeed(Random.Range(10f, 50f));
    }
    
    [ContextMenu("Show Statistics")]
    public void TestShowStatistics()
    {
        ShowStatistics();
    }
    
    [ContextMenu("Hide Statistics")]
    public void TestHideStatistics()
    {
        HideStatistics();
    }
    
    [ContextMenu("Find UI Components")]
    public void ManualFindUIComponents()
    {
        AutoFindUIComponents();
        if (!hideStatsAtStart)
        {
            UpdateUI();
        }
    }
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
} 