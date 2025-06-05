using UnityEngine;
using UnityEngine.UI;

public class SimpleSettingsCreator : MonoBehaviour
{
    [Header("Quick Setup")]
    [SerializeField] private bool createSettingsPanel = false;
    
    [ContextMenu("Create Simple Settings Panel")]
    public void CreateSimpleSettingsPanel()
    {
        Debug.Log("Creating simple settings panel...");
        
        // Find Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in scene!");
            return;
        }
        
        // Find or create InGameMenuManager
        InGameMenuManager menuManager = FindObjectOfType<InGameMenuManager>();
        if (menuManager == null)
        {
            GameObject managerObj = new GameObject("InGameMenuManager");
            menuManager = managerObj.AddComponent<InGameMenuManager>();
            Debug.Log("Created InGameMenuManager");
        }
        
        // Create pause menu panel
        GameObject pausePanel = CreatePausePanel(canvas.transform);
        
        // Create settings panel
        GameObject settingsPanel = CreateSettingsPanel(canvas.transform);
        
        // Assign to menu manager
        menuManager.pauseMenuPanel = pausePanel;
        menuManager.settingsPanel = settingsPanel;
        
        // Find and assign buttons
        menuManager.resumeButton = pausePanel.transform.Find("ResumeButton")?.GetComponent<Button>();
        menuManager.settingsButton = pausePanel.transform.Find("SettingsButton")?.GetComponent<Button>();
        menuManager.mainMenuButton = pausePanel.transform.Find("MainMenuButton")?.GetComponent<Button>();
        menuManager.quitButton = pausePanel.transform.Find("QuitButton")?.GetComponent<Button>();
        
        // Find settings manager
        menuManager.settingsManager = settingsPanel.GetComponent<SettingsManager>();
        
        Debug.Log("Simple settings panel created! Press ESC to test.");
    }
    
    private GameObject CreatePausePanel(Transform parent)
    {
        // Create pause panel
        GameObject pausePanel = new GameObject("PauseMenuPanel");
        pausePanel.transform.SetParent(parent, false);
        
        // Add background
        Image bgImage = pausePanel.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.8f);
        
        RectTransform rect = pausePanel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        // Create title
        GameObject title = CreateText("Title", "PAUSE", pausePanel.transform);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.8f);
        titleRect.anchorMax = new Vector2(0.5f, 0.8f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = new Vector2(300, 60);
        
        Text titleText = title.GetComponent<Text>();
        titleText.fontSize = 36;
        titleText.fontStyle = FontStyle.Bold;
        
        // Create buttons
        CreateButton("ResumeButton", "RESUME", pausePanel.transform, new Vector2(0, 50));
        CreateButton("SettingsButton", "SETTINGS", pausePanel.transform, new Vector2(0, 0));
        CreateButton("MainMenuButton", "MAIN MENU", pausePanel.transform, new Vector2(0, -50));
        CreateButton("QuitButton", "QUIT", pausePanel.transform, new Vector2(0, -100));
        
        pausePanel.SetActive(false);
        return pausePanel;
    }
    
    private GameObject CreateSettingsPanel(Transform parent)
    {
        // Create settings panel
        GameObject settingsPanel = new GameObject("SettingsPanel");
        settingsPanel.transform.SetParent(parent, false);
        
        // Add background
        Image bgImage = settingsPanel.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        RectTransform rect = settingsPanel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        // Add SettingsManager
        SettingsManager settingsManager = settingsPanel.AddComponent<SettingsManager>();
        settingsManager.isInGameSettings = true;
        settingsManager.autoFindComponents = false; // We'll assign manually
        
        // Create title
        GameObject title = CreateText("Title", "SETTINGS", settingsPanel.transform);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.9f);
        titleRect.anchorMax = new Vector2(0.5f, 0.9f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = new Vector2(300, 60);
        
        Text titleText = title.GetComponent<Text>();
        titleText.fontSize = 32;
        titleText.fontStyle = FontStyle.Bold;
        
        // Create graphics section
        float yPos = 200;
        CreateText("GraphicsLabel", "GRAPHICS", settingsPanel.transform, new Vector2(0, yPos), 24);
        
        yPos -= 50;
        GameObject qualityDropdown = CreateDropdown("QualityDropdown", "Quality:", settingsPanel.transform, new Vector2(0, yPos));
        
        yPos -= 50;
        GameObject resolutionDropdown = CreateDropdown("ResolutionDropdown", "Resolution:", settingsPanel.transform, new Vector2(0, yPos));
        
        yPos -= 50;
        GameObject fullscreenToggle = CreateToggle("FullscreenToggle", "Fullscreen", settingsPanel.transform, new Vector2(0, yPos));
        
        // Create audio section
        yPos -= 80;
        CreateText("AudioLabel", "AUDIO", settingsPanel.transform, new Vector2(0, yPos), 24);
        
        yPos -= 50;
        GameObject masterSlider = CreateSlider("MasterVolumeSlider", "Master Volume:", settingsPanel.transform, new Vector2(0, yPos));
        
        yPos -= 50;
        GameObject musicSlider = CreateSlider("MusicVolumeSlider", "Music:", settingsPanel.transform, new Vector2(0, yPos));
        
        yPos -= 50;
        GameObject sfxSlider = CreateSlider("SFXVolumeSlider", "Sound Effects:", settingsPanel.transform, new Vector2(0, yPos));
        
        // Create buttons
        yPos -= 80;
        CreateButton("ApplyButton", "APPLY", settingsPanel.transform, new Vector2(-100, yPos));
        CreateButton("ResetButton", "RESET", settingsPanel.transform, new Vector2(0, yPos));
        CreateButton("CloseButton", "CLOSE", settingsPanel.transform, new Vector2(100, yPos));
        
        // Assign components to settings manager
        settingsManager.qualityDropdown = qualityDropdown.GetComponent<Dropdown>();
        settingsManager.resolutionDropdown = resolutionDropdown.GetComponent<Dropdown>();
        settingsManager.fullscreenToggle = fullscreenToggle.GetComponent<Toggle>();
        settingsManager.masterVolumeSlider = masterSlider.GetComponent<Slider>();
        settingsManager.musicVolumeSlider = musicSlider.GetComponent<Slider>();
        settingsManager.sfxVolumeSlider = sfxSlider.GetComponent<Slider>();
        settingsManager.applyButton = settingsPanel.transform.Find("ApplyButton").GetComponent<Button>();
        settingsManager.resetButton = settingsPanel.transform.Find("ResetButton").GetComponent<Button>();
        settingsManager.closeButton = settingsPanel.transform.Find("CloseButton").GetComponent<Button>();
        
        settingsPanel.SetActive(false);
        return settingsPanel;
    }
    
    private GameObject CreateText(string name, string text, Transform parent, Vector2 position = default, int fontSize = 18)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        Text textComponent = textObj.AddComponent<Text>();
        textComponent.text = text;
        textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComponent.fontSize = fontSize;
        textComponent.color = Color.white;
        textComponent.alignment = TextAnchor.MiddleCenter;
        
        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(300, 30);
        
        return textObj;
    }
    
    private GameObject CreateButton(string name, string text, Transform parent, Vector2 position)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        Button button = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.3f, 0.5f, 0.8f);
        
        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(200, 40);
        
        // Create text child
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = text;
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 16;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        button.targetGraphic = buttonImage;
        return buttonObj;
    }
    
    private GameObject CreateDropdown(string name, string label, Transform parent, Vector2 position)
    {
        // Create container
        GameObject container = new GameObject(name + "Container");
        container.transform.SetParent(parent, false);
        
        RectTransform containerRect = container.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = position;
        containerRect.sizeDelta = new Vector2(400, 30);
        
        // Create label
        GameObject labelObj = CreateText("Label", label, container.transform, new Vector2(-100, 0), 14);
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(150, 30);
        labelRect.anchorMin = new Vector2(0, 0.5f);
        labelRect.anchorMax = new Vector2(0, 0.5f);
        
        // Create dropdown
        GameObject dropdownObj = new GameObject(name);
        dropdownObj.transform.SetParent(container.transform, false);
        
        Dropdown dropdown = dropdownObj.AddComponent<Dropdown>();
        Image dropdownImage = dropdownObj.AddComponent<Image>();
        dropdownImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        
        RectTransform dropdownRect = dropdownObj.GetComponent<RectTransform>();
        dropdownRect.anchorMin = new Vector2(1, 0.5f);
        dropdownRect.anchorMax = new Vector2(1, 0.5f);
        dropdownRect.anchoredPosition = new Vector2(-100, 0);
        dropdownRect.sizeDelta = new Vector2(200, 30);
        
        // Add some default options
        dropdown.options.Clear();
        if (name.Contains("Quality"))
        {
            dropdown.options.Add(new Dropdown.OptionData("Low"));
            dropdown.options.Add(new Dropdown.OptionData("Medium"));
            dropdown.options.Add(new Dropdown.OptionData("High"));
        }
        else if (name.Contains("Resolution"))
        {
            dropdown.options.Add(new Dropdown.OptionData("1920x1080"));
            dropdown.options.Add(new Dropdown.OptionData("1366x768"));
            dropdown.options.Add(new Dropdown.OptionData("1280x720"));
        }
        
        return dropdownObj;
    }
    
    private GameObject CreateSlider(string name, string label, Transform parent, Vector2 position)
    {
        // Create container
        GameObject container = new GameObject(name + "Container");
        container.transform.SetParent(parent, false);
        
        RectTransform containerRect = container.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = position;
        containerRect.sizeDelta = new Vector2(400, 30);
        
        // Create label
        GameObject labelObj = CreateText("Label", label, container.transform, new Vector2(-100, 0), 14);
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(150, 30);
        labelRect.anchorMin = new Vector2(0, 0.5f);
        labelRect.anchorMax = new Vector2(0, 0.5f);
        
        // Create slider
        GameObject sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(container.transform, false);
        
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.75f;
        
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(1, 0.5f);
        sliderRect.anchorMax = new Vector2(1, 0.5f);
        sliderRect.anchoredPosition = new Vector2(-100, 0);
        sliderRect.sizeDelta = new Vector2(200, 30);
        
        // Create background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderObj.transform, false);
        
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Create fill
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;
        
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.5f, 0.8f, 1f);
        
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        slider.fillRect = fillRect;
        
        return sliderObj;
    }
    
    private GameObject CreateToggle(string name, string label, Transform parent, Vector2 position)
    {
        // Create container
        GameObject container = new GameObject(name + "Container");
        container.transform.SetParent(parent, false);
        
        RectTransform containerRect = container.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = position;
        containerRect.sizeDelta = new Vector2(400, 30);
        
        // Create toggle
        GameObject toggleObj = new GameObject(name);
        toggleObj.transform.SetParent(container.transform, false);
        
        Toggle toggle = toggleObj.AddComponent<Toggle>();
        Image toggleImage = toggleObj.AddComponent<Image>();
        toggleImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        
        RectTransform toggleRect = toggleObj.GetComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(0, 0.5f);
        toggleRect.anchorMax = new Vector2(0, 0.5f);
        toggleRect.anchoredPosition = new Vector2(20, 0);
        toggleRect.sizeDelta = new Vector2(20, 20);
        
        // Create checkmark
        GameObject checkmark = new GameObject("Checkmark");
        checkmark.transform.SetParent(toggleObj.transform, false);
        
        Image checkImage = checkmark.AddComponent<Image>();
        checkImage.color = Color.white;
        
        RectTransform checkRect = checkmark.GetComponent<RectTransform>();
        checkRect.anchorMin = Vector2.zero;
        checkRect.anchorMax = Vector2.one;
        checkRect.offsetMin = Vector2.zero;
        checkRect.offsetMax = Vector2.zero;
        
        toggle.graphic = checkImage;
        toggle.isOn = true;
        
        // Create label
        GameObject labelObj = CreateText("Label", label, container.transform, new Vector2(100, 0), 14);
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0.5f);
        labelRect.anchorMax = new Vector2(0, 0.5f);
        labelRect.anchoredPosition = new Vector2(150, 0);
        
        return toggleObj;
    }
} 