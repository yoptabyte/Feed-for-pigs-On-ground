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
    [SerializeField] private Dictionary<string, int> itemsEatenByType = new Dictionary<string, int>();
    
    [Header("UI References - Auto Find")]
    [Tooltip("Text компонент для отображения максимальной скорости")]
    public Text maxSpeedText;
    
    [Tooltip("TextMeshPro компонент для отображения максимальной скорости")]
    public TextMeshProUGUI maxSpeedTextTMP;
    
    [Tooltip("Text компонент для отображения количества съеденных предметов")]
    public Text itemsEatenText;
    
    [Tooltip("TextMeshPro компонент для отображения количества съеденных предметов")]
    public TextMeshProUGUI itemsEatenTextTMP;
    
    [Header("Display Settings")]
    [Tooltip("Формат отображения скорости")]
    public string speedFormat = "Макс. скорость: {0:F1}";
    
    [Tooltip("Формат отображения предметов")]
    public string itemsFormat = "Съедено предметов: {0}";
    
    [Tooltip("Скрывать статистику в начале игры")]
    public bool hideStatsAtStart = true;
    
    [Header("Auto Find Settings")]
    [Tooltip("Автопоиск UI компонентов")]
    public bool autoFindUIComponents = true;
    
    [Header("Debug")]
    [Tooltip("Показывать отладочную информацию")]
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
                Debug.Log("GameStatsTracker: Singleton создан");
            }
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.Log("GameStatsTracker: Дублированный экземпляр уничтожен");
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
            Debug.Log("GameStatsTracker: Автопоиск UI компонентов...");
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
                if (name.Contains("speed") || name.Contains("скорость") || name.Contains("max"))
                {
                    maxSpeedText = text;
                    if (showDebugInfo)
                    {
                        Debug.Log($"GameStatsTracker: Найден Speed Text: {text.name}");
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
                        Debug.Log($"GameStatsTracker: Найден Speed TextMeshPro: {textTMP.name}");
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
                        Debug.Log($"GameStatsTracker: Найден Items Text: {text.name}");
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
                        Debug.Log($"GameStatsTracker: Найден Items TextMeshPro: {textTMP.name}");
                    }
                    break;
                }
            }
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
                Debug.Log($"GameStatsTracker: Новая максимальная скорость: {maxSpeedReached:F1}");
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
            Debug.Log($"GameStatsTracker: Добавлен предмет '{itemName}'. Всего: {totalItemsEaten}");
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
            
        return visible;
    }
    
    // Show statistics (called on death or finish)
    public void ShowStatistics()
    {
        SetStatisticsVisibility(true);
        UpdateUI();
        
        if (showDebugInfo)
        {
            Debug.Log("GameStatsTracker: Статистика показана");
        }
    }
    
    // Hide statistics
    public void HideStatistics()
    {
        SetStatisticsVisibility(false);
        
        if (showDebugInfo)
        {
            Debug.Log("GameStatsTracker: Статистика скрыта");
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
    
    public void UpdateUI()
    {
        UpdateSpeedUI();
        UpdateItemsUI();
    }
    
    // Public getters
    public float MaxSpeedReached => maxSpeedReached;
    public int TotalItemsEaten => totalItemsEaten;
    public Dictionary<string, int> ItemsEatenByType => new Dictionary<string, int>(itemsEatenByType);
    
    // Reset methods
    public void ResetStats()
    {
        maxSpeedReached = 0f;
        totalItemsEaten = 0;
        itemsEatenByType.Clear();
        
        UpdateUI();
        
        if (showDebugInfo)
        {
            Debug.Log("GameStatsTracker: Статистика сброшена");
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