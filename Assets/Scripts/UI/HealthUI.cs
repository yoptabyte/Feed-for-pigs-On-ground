using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthUI : MonoBehaviour
{
    [Header("Health Display Settings")]
    [SerializeField] private Sprite healthSprite; // HP image sprite
    [SerializeField] private Sprite emptyHealthSprite; // Optional: empty/grey HP sprite
    [SerializeField] private int maxHealthIcons = 3; // Number of health icons to display
    
    [Header("UI Layout")]
    [SerializeField] private Vector2 iconSize = new Vector2(500f, 500f); // Size of each health icon
    [SerializeField] private float iconSpacing = 10f; // Spacing between icons
    [SerializeField] private Vector2 screenOffset = new Vector2(20f, -20f); // Offset from top-left corner
    
    [Header("References")]
    [SerializeField] private Canvas uiCanvas; // Canvas to place health icons on
    [SerializeField] private Health playerHealth; // Reference to player's health component
    
    private List<Image> healthIcons = new List<Image>();
    private RectTransform canvasRect;
    
    private void Start()
    {
        // Find canvas if not assigned
        if (uiCanvas == null)
        {
            uiCanvas = FindObjectOfType<Canvas>();
            if (uiCanvas == null)
            {
                Debug.LogError("HealthUI: No Canvas found! Please assign a Canvas or create one in the scene.");
                return;
            }
        }
        
        canvasRect = uiCanvas.GetComponent<RectTransform>();
        
        // Find player health if not assigned
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<Health>();
            if (playerHealth == null)
            {
                Debug.LogError("HealthUI: No Health component found! Please assign a Health component.");
                return;
            }
        }
        
        CreateHealthIcons();
        UpdateHealthDisplay();
        
        // Subscribe to health events
        playerHealth.OnDamageTaken.AddListener(OnPlayerDamageTaken);
        playerHealth.OnDeath.AddListener(OnPlayerDeath);
    }
    
    private void CreateHealthIcons()
    {
        // Create parent object for health icons
        GameObject healthContainer = new GameObject("HealthContainer");
        RectTransform containerRect = healthContainer.AddComponent<RectTransform>();
        healthContainer.transform.SetParent(uiCanvas.transform, false);
        
        // Position container in top-left corner
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(0, 1);
        containerRect.pivot = new Vector2(0, 1);
        containerRect.anchoredPosition = screenOffset;
        
        // Calculate container size
        float totalWidth = (iconSize.x * maxHealthIcons) + (iconSpacing * (maxHealthIcons - 1));
        containerRect.sizeDelta = new Vector2(totalWidth, iconSize.y);
        
        // Create individual health icons
        for (int i = 0; i < maxHealthIcons; i++)
        {
            GameObject iconObject = new GameObject($"HealthIcon_{i}");
            Image iconImage = iconObject.AddComponent<Image>();
            iconImage.sprite = healthSprite;
            iconImage.preserveAspect = true;
            
            RectTransform iconRect = iconObject.GetComponent<RectTransform>();
            iconObject.transform.SetParent(healthContainer.transform, false);
            
            // Position icon
            iconRect.anchorMin = new Vector2(0, 0.5f);
            iconRect.anchorMax = new Vector2(0, 0.5f);
            iconRect.pivot = new Vector2(0, 0.5f);
            iconRect.sizeDelta = iconSize;
            iconRect.anchoredPosition = new Vector2(i * (iconSize.x + iconSpacing), 0);
            
            healthIcons.Add(iconImage);
        }
    }
    
    private void UpdateHealthDisplay()
    {
        if (playerHealth == null || healthIcons.Count == 0) return;
        
        int currentHealth = playerHealth.CurrentHP;
        
        for (int i = 0; i < healthIcons.Count; i++)
        {
            if (i < currentHealth)
            {
                // Show full health icon
                healthIcons[i].sprite = healthSprite;
                healthIcons[i].color = Color.white;
            }
            else
            {
                // Show empty/grey health icon
                if (emptyHealthSprite != null)
                {
                    healthIcons[i].sprite = emptyHealthSprite;
                    healthIcons[i].color = Color.white;
                }
                else
                {
                    // If no empty sprite, make the icon grey
                    healthIcons[i].sprite = healthSprite;
                    healthIcons[i].color = Color.gray;
                }
            }
        }
    }
    
    private void OnPlayerDamageTaken(int damage)
    {
        UpdateHealthDisplay();
        Debug.Log($"HealthUI: Player took {damage} damage. Updating display.");
    }
    
    private void OnPlayerDeath()
    {
        UpdateHealthDisplay();
        Debug.Log("HealthUI: Player died. All health icons should be grey/empty.");
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (playerHealth != null)
        {
            playerHealth.OnDamageTaken.RemoveListener(OnPlayerDamageTaken);
            playerHealth.OnDeath.RemoveListener(OnPlayerDeath);
        }
    }
    
    // Public method to manually update health display (useful for testing)
    public void RefreshHealthDisplay()
    {
        UpdateHealthDisplay();
    }
    
    // Public method to change health sprite at runtime
    public void SetHealthSprite(Sprite newSprite)
    {
        healthSprite = newSprite;
        UpdateHealthDisplay();
    }
    
    // Public method to change empty health sprite at runtime
    public void SetEmptyHealthSprite(Sprite newEmptySprite)
    {
        emptyHealthSprite = newEmptySprite;
        UpdateHealthDisplay();
    }
} 