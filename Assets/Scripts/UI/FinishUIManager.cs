using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FinishUIManager : MonoBehaviour
{
    [Header("UI Creation Settings")]
    [SerializeField] private bool autoCreateUI = true;
    [SerializeField] private Canvas targetCanvas;
    
    [Header("Button Settings")]
    [SerializeField] private Vector2 buttonSize = new Vector2(200f, 50f);
    [SerializeField] private float buttonSpacing = 60f;
    [SerializeField] private Vector2 buttonsPosition = new Vector2(0f, -100f);
    
    [Header("Text Settings")]
    [SerializeField] private Font defaultFont;
    [SerializeField] private int fontSize = 24;
    [SerializeField] private Color textColor = Color.white;
    
    void Start()
    {
        if (autoCreateUI)
        {
            CreateFinishUI();
        }
    }
    
    [ContextMenu("Create Finish UI")]
    public void CreateFinishUI()
    {
        // Find or create canvas
        if (targetCanvas == null)
        {
            targetCanvas = FindObjectOfType<Canvas>();
            if (targetCanvas == null)
            {
                Debug.LogWarning("FinishUIManager: No Canvas found in scene!");
                return;
            }
        }
        
        // Create main finish UI panel
        GameObject finishPanel = CreateFinishPanel();
        
        // Create result image container
        CreateResultImage(finishPanel);
        
        // Create result text
        CreateResultText(finishPanel);
        
        // Create time text
        CreateTimeText(finishPanel);
        
        // Create statistics texts
        CreateStatisticsTexts(finishPanel);
        
        // Create buttons
        CreateMenuButtons(finishPanel);
        
        // Initially hide the panel
        finishPanel.SetActive(false);
        
        Debug.Log("FinishUIManager: Finish UI created successfully!");
    }
    
    private GameObject CreateFinishPanel()
    {
        GameObject panel = new GameObject("FinishUI");
        panel.transform.SetParent(targetCanvas.transform, false);
        
        // Add RectTransform
        RectTransform rectTransform = panel.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        // Add background image
        Image backgroundImage = panel.AddComponent<Image>();
        backgroundImage.color = new Color(0f, 0f, 0f, 0.8f); // Semi-transparent black
        
        return panel;
    }
    
    private void CreateResultImage(GameObject parent)
    {
        GameObject resultImageObj = new GameObject("ResultImage");
        resultImageObj.transform.SetParent(parent.transform, false);
        
        RectTransform rectTransform = resultImageObj.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0f, 100f);
        rectTransform.sizeDelta = new Vector2(200f, 200f);
        
        Image resultImage = resultImageObj.AddComponent<Image>();
        resultImage.color = Color.white;
        // Note: Sprite should be assigned from FinishLineController
        
        // Initially visible (will be hidden when statistics show)
        resultImageObj.SetActive(true);
    }
    
    private void CreateResultText(GameObject parent)
    {
        GameObject resultTextObj = new GameObject("ResultText");
        resultTextObj.transform.SetParent(parent.transform, false);
        
        RectTransform rectTransform = resultTextObj.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0f, 150f);
        rectTransform.sizeDelta = new Vector2(600f, 80f);
        
        TextMeshProUGUI resultText = resultTextObj.AddComponent<TextMeshProUGUI>();
        resultText.text = "Game Result";
        resultText.fontSize = fontSize + 10;
        resultText.color = textColor;
        resultText.alignment = TextAlignmentOptions.Center;
        resultText.fontStyle = FontStyles.Bold;
    }
    
    private void CreateTimeText(GameObject parent)
    {
        GameObject timeTextObj = new GameObject("TimeText");
        timeTextObj.transform.SetParent(parent.transform, false);
        
        RectTransform rectTransform = timeTextObj.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0f, 80f);
        rectTransform.sizeDelta = new Vector2(400f, 50f);
        
        TextMeshProUGUI timeText = timeTextObj.AddComponent<TextMeshProUGUI>();
        timeText.text = "Time: 00:00.000";
        timeText.fontSize = fontSize;
        timeText.color = textColor;
        timeText.alignment = TextAlignmentOptions.Center;
        
        // Initially hide time text
        timeTextObj.SetActive(false);
    }
    
    private void CreateStatisticsTexts(GameObject parent)
    {
        // Max Speed Text
        GameObject speedTextObj = new GameObject("MaxSpeedText");
        speedTextObj.transform.SetParent(parent.transform, false);
        
        RectTransform speedRect = speedTextObj.AddComponent<RectTransform>();
        speedRect.anchoredPosition = new Vector2(0f, 20f);
        speedRect.sizeDelta = new Vector2(400f, 40f);
        
        TextMeshProUGUI speedText = speedTextObj.AddComponent<TextMeshProUGUI>();
        speedText.text = "Max speed: 0.0";
        speedText.fontSize = fontSize - 4;
        speedText.color = textColor;
        speedText.alignment = TextAlignmentOptions.Center;
        
        // Initially hide statistics
        speedTextObj.SetActive(false);
        
        // Items Eaten Text
        GameObject itemsTextObj = new GameObject("ItemsEatenText");
        itemsTextObj.transform.SetParent(parent.transform, false);
        
        RectTransform itemsRect = itemsTextObj.AddComponent<RectTransform>();
        itemsRect.anchoredPosition = new Vector2(0f, -20f);
        itemsRect.sizeDelta = new Vector2(400f, 40f);
        
        TextMeshProUGUI itemsText = itemsTextObj.AddComponent<TextMeshProUGUI>();
        itemsText.text = "Collected items: 0";
        itemsText.fontSize = fontSize - 4;
        itemsText.color = textColor;
        itemsText.alignment = TextAlignmentOptions.Center;
        
        // Initially hide statistics
        itemsTextObj.SetActive(false);
    }
    
    private void CreateMenuButtons(GameObject parent)
    {
        // Main Menu Button
        GameObject mainMenuButtonObj = CreateButton(parent, "MainMenuButton", "Main Menu", 
            new Vector2(buttonsPosition.x - buttonSize.x/2 - buttonSpacing/2, buttonsPosition.y));
        
        // Restart Button
        GameObject restartButtonObj = CreateButton(parent, "RestartButton", "Restart", 
            new Vector2(buttonsPosition.x + buttonSize.x/2 + buttonSpacing/2, buttonsPosition.y));
        
        // Setup button colors
        SetupButtonColors(mainMenuButtonObj.GetComponent<Button>(), new Color(0.8f, 0.2f, 0.2f)); // Red-ish
        SetupButtonColors(restartButtonObj.GetComponent<Button>(), new Color(0.2f, 0.6f, 0.2f)); // Green-ish
        
        // Initially hide buttons
        mainMenuButtonObj.SetActive(false);
        restartButtonObj.SetActive(false);
    }
    
    private GameObject CreateButton(GameObject parent, string name, string text, Vector2 position)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent.transform, false);
        
        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = buttonSize;
        
        // Add Image component for button background
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = Color.white;
        
        // Add Button component
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        
        // Create text child
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = fontSize - 2;
        buttonText.color = Color.black;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontStyle = FontStyles.Bold;
        
        return buttonObj;
    }
    
    private void SetupButtonColors(Button button, Color normalColor)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = normalColor;
        colors.highlightedColor = normalColor * 1.2f;
        colors.pressedColor = normalColor * 0.8f;
        colors.selectedColor = normalColor;
        colors.disabledColor = normalColor * 0.5f;
        button.colors = colors;
    }
    
    [ContextMenu("Find And Setup Existing UI")]
    public void FindAndSetupExistingUI()
    {
        // Try to find existing finish UI elements and connect them to FinishLineController
        FinishLineController finishController = FinishLineController.Instance;
        if (finishController == null)
        {
            Debug.LogWarning("FinishUIManager: FinishLineController not found!");
            return;
        }
        
        // This would require reflection or public fields to set the references
        Debug.Log("FinishUIManager: Please manually assign UI elements to FinishLineController in inspector");
    }
    
    [ContextMenu("Test Show Finish UI")]
    public void TestShowFinishUI()
    {
        GameObject finishUI = GameObject.Find("FinishUI");
        if (finishUI != null)
        {
            finishUI.SetActive(true);
            
            // Test with sample data
            TextMeshProUGUI resultText = finishUI.transform.Find("ResultText")?.GetComponent<TextMeshProUGUI>();
            if (resultText != null)
            {
                resultText.text = "Test result!";
                resultText.color = Color.green;
            }
            
            TextMeshProUGUI timeText = finishUI.transform.Find("TimeText")?.GetComponent<TextMeshProUGUI>();
            if (timeText != null)
            {
                timeText.text = "Time: 01:23.456";
            }
            
            Debug.Log("FinishUIManager: Test finish UI shown");
        }
        else
        {
            Debug.LogWarning("FinishUIManager: FinishUI not found! Create it first.");
        }
    }
    
    [ContextMenu("Hide Finish UI")]
    public void HideFinishUI()
    {
        GameObject finishUI = GameObject.Find("FinishUI");
        if (finishUI != null)
        {
            finishUI.SetActive(false);
            Debug.Log("FinishUIManager: Finish UI hidden");
        }
    }
    
    [ContextMenu("Test Delayed Statistics Show")]
    public void TestDelayedStatisticsShow()
    {
        GameObject finishUI = GameObject.Find("FinishUI");
        if (finishUI != null)
        {
            finishUI.SetActive(true);
            
            // Show result image initially
            Transform resultImageTransform = finishUI.transform.Find("ResultImage");
            if (resultImageTransform != null)
            {
                resultImageTransform.gameObject.SetActive(true);
                Image resultImage = resultImageTransform.GetComponent<Image>();
                if (resultImage != null)
                {
                    resultImage.color = Color.green; // Test color
                }
            }
            
            // Show only result text initially
            TextMeshProUGUI resultText = finishUI.transform.Find("ResultText")?.GetComponent<TextMeshProUGUI>();
            if (resultText != null)
            {
                resultText.text = "Test victory!";
                resultText.color = Color.green;
                resultText.gameObject.SetActive(true);
            }
            
            // Hide statistics and buttons initially
            Transform speedText = finishUI.transform.Find("MaxSpeedText");
            if (speedText != null) speedText.gameObject.SetActive(false);
            
            Transform itemsText = finishUI.transform.Find("ItemsEatenText");
            if (itemsText != null) itemsText.gameObject.SetActive(false);
            
            Transform timeText = finishUI.transform.Find("TimeText");
            if (timeText != null) timeText.gameObject.SetActive(false);
            
            Transform mainMenuBtn = finishUI.transform.Find("MainMenuButton");
            if (mainMenuBtn != null) mainMenuBtn.gameObject.SetActive(false);
            
            Transform restartBtn = finishUI.transform.Find("RestartButton");
            if (restartBtn != null) restartBtn.gameObject.SetActive(false);
            
            Debug.Log("FinishUIManager: Test showing result image and text first, statistics will appear in 3 seconds...");
            
            // Start coroutine to show statistics after 3 seconds
            StartCoroutine(ShowStatisticsAfterDelay());
        }
        else
        {
            Debug.LogWarning("FinishUIManager: FinishUI not found! Create it first.");
        }
    }
    
    private System.Collections.IEnumerator ShowStatisticsAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        
        GameObject finishUI = GameObject.Find("FinishUI");
        if (finishUI != null)
        {
            // Hide result image when showing statistics
            Transform resultImageTransform = finishUI.transform.Find("ResultImage");
            if (resultImageTransform != null)
            {
                resultImageTransform.gameObject.SetActive(false);
                Debug.Log("FinishUIManager: Result image hidden");
            }
            
            // Show time text
            Transform timeText = finishUI.transform.Find("TimeText");
            if (timeText != null)
            {
                timeText.gameObject.SetActive(true);
                TextMeshProUGUI timeTextComponent = timeText.GetComponent<TextMeshProUGUI>();
                if (timeTextComponent != null)
                {
                    timeTextComponent.text = "Time: 01:23.456";
                }
            }
            
            // Show statistics
            Transform speedText = finishUI.transform.Find("MaxSpeedText");
            if (speedText != null)
            {
                speedText.gameObject.SetActive(true);
                TextMeshProUGUI speedTextComponent = speedText.GetComponent<TextMeshProUGUI>();
                if (speedTextComponent != null)
                {
                    speedTextComponent.text = "Max speed: 25.3";
                }
            }
            
            Transform itemsText = finishUI.transform.Find("ItemsEatenText");
            if (itemsText != null)
            {
                itemsText.gameObject.SetActive(true);
                TextMeshProUGUI itemsTextComponent = itemsText.GetComponent<TextMeshProUGUI>();
                if (itemsTextComponent != null)
                {
                    itemsTextComponent.text = "Collected items: 5";
                }
            }
            
            // Show buttons
            Transform mainMenuBtn = finishUI.transform.Find("MainMenuButton");
            if (mainMenuBtn != null) mainMenuBtn.gameObject.SetActive(true);
            
            Transform restartBtn = finishUI.transform.Find("RestartButton");
            if (restartBtn != null) restartBtn.gameObject.SetActive(true);
            
            Debug.Log("FinishUIManager: Statistics and buttons shown after delay, result image hidden!");
        }
    }
} 