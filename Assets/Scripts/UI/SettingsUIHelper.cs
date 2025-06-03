using UnityEngine;
using UnityEngine.UI;

public class SettingsUIHelper : MonoBehaviour
{
    [Header("Create Settings UI")]
    [SerializeField] private bool createUI = false;
    
    [ContextMenu("Create Settings UI")]
    public void CreateSettingsUI()
    {
        Debug.Log("Creating Settings UI structure...");
        
        // Create main sections
        CreateGraphicsSection();
        CreateAudioSection();
        CreateButtonsSection();
        
        Debug.Log("Settings UI created! Make sure to assign SettingsManager component.");
    }
    
    private void CreateGraphicsSection()
    {
        // Graphics section
        GameObject graphicsSection = new GameObject("GraphicsSection");
        graphicsSection.transform.SetParent(transform);
        
        // Quality Dropdown
        CreateDropdown("QualityDropdown", graphicsSection.transform);
        
        // Resolution Dropdown  
        CreateDropdown("ResolutionDropdown", graphicsSection.transform);
        
        // Fullscreen Toggle
        CreateToggle("FullscreenToggle", graphicsSection.transform);
    }
    
    private void CreateAudioSection()
    {
        // Audio section
        GameObject audioSection = new GameObject("AudioSection");
        audioSection.transform.SetParent(transform);
        
        // Volume Sliders
        CreateSlider("MasterVolumeSlider", audioSection.transform);
        CreateSlider("MusicVolumeSlider", audioSection.transform);
        CreateSlider("SFXVolumeSlider", audioSection.transform);
    }
    
    private void CreateButtonsSection()
    {
        // Buttons section
        GameObject buttonsSection = new GameObject("ButtonsSection");
        buttonsSection.transform.SetParent(transform);
        
        // Control Buttons
        CreateButton("ApplyButton", "Apply", buttonsSection.transform);
        CreateButton("ResetButton", "Reset", buttonsSection.transform);
        CreateButton("CloseButton", "Close", buttonsSection.transform);
    }
    
    private void CreateDropdown(string name, Transform parent)
    {
        GameObject dropdownGO = new GameObject(name);
        dropdownGO.transform.SetParent(parent);
        
        // Add RectTransform
        RectTransform rectTransform = dropdownGO.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, 30);
        
        // Add Image component
        Image image = dropdownGO.AddComponent<Image>();
        
        // Add Dropdown component
        Dropdown dropdown = dropdownGO.AddComponent<Dropdown>();
        
        // Create Template
        GameObject template = new GameObject("Template");
        template.transform.SetParent(dropdownGO.transform);
        RectTransform templateRect = template.AddComponent<RectTransform>();
        templateRect.sizeDelta = new Vector2(200, 150);
        
        // Create Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(template.transform);
        viewport.AddComponent<RectTransform>();
        viewport.AddComponent<Image>();
        viewport.AddComponent<Mask>();
        
        // Create Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform);
        content.AddComponent<RectTransform>();
        
        // Create Item
        GameObject item = new GameObject("Item");
        item.transform.SetParent(content.transform);
        item.AddComponent<RectTransform>();
        item.AddComponent<Toggle>();
        
        // Create Item Background
        GameObject itemBackground = new GameObject("Item Background");
        itemBackground.transform.SetParent(item.transform);
        itemBackground.AddComponent<RectTransform>();
        itemBackground.AddComponent<Image>();
        
        // Create Item Checkmark
        GameObject itemCheckmark = new GameObject("Item Checkmark");
        itemCheckmark.transform.SetParent(item.transform);
        itemCheckmark.AddComponent<RectTransform>();
        itemCheckmark.AddComponent<Image>();
        
        // Create Item Label
        GameObject itemLabel = new GameObject("Item Label");
        itemLabel.transform.SetParent(item.transform);
        itemLabel.AddComponent<RectTransform>();
        itemLabel.AddComponent<Text>();
        
        // Setup dropdown references
        dropdown.template = templateRect;
        dropdown.captionText = itemLabel.GetComponent<Text>();
        dropdown.itemText = itemLabel.GetComponent<Text>();
        
        template.SetActive(false);
        
        Debug.Log($"Created Dropdown: {name}");
    }
    
    private void CreateSlider(string name, Transform parent)
    {
        GameObject sliderGO = new GameObject(name);
        sliderGO.transform.SetParent(parent);
        
        // Add RectTransform
        RectTransform rectTransform = sliderGO.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, 20);
        
        // Add Slider component
        Slider slider = sliderGO.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.75f;
        
        // Create Background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderGO.transform);
        background.AddComponent<RectTransform>();
        background.AddComponent<Image>();
        
        // Create Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGO.transform);
        fillArea.AddComponent<RectTransform>();
        
        // Create Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform);
        fill.AddComponent<RectTransform>();
        fill.AddComponent<Image>();
        
        // Create Handle Slide Area
        GameObject handleSlideArea = new GameObject("Handle Slide Area");
        handleSlideArea.transform.SetParent(sliderGO.transform);
        handleSlideArea.AddComponent<RectTransform>();
        
        // Create Handle
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleSlideArea.transform);
        handle.AddComponent<RectTransform>();
        handle.AddComponent<Image>();
        
        // Setup slider references
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handle.GetComponent<RectTransform>();
        
        Debug.Log($"Created Slider: {name}");
    }
    
    private void CreateToggle(string name, Transform parent)
    {
        GameObject toggleGO = new GameObject(name);
        toggleGO.transform.SetParent(parent);
        
        // Add RectTransform
        RectTransform rectTransform = toggleGO.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(160, 20);
        
        // Add Toggle component
        Toggle toggle = toggleGO.AddComponent<Toggle>();
        
        // Create Background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(toggleGO.transform);
        background.AddComponent<RectTransform>();
        background.AddComponent<Image>();
        
        // Create Checkmark
        GameObject checkmark = new GameObject("Checkmark");
        checkmark.transform.SetParent(background.transform);
        checkmark.AddComponent<RectTransform>();
        checkmark.AddComponent<Image>();
        
        // Create Label
        GameObject label = new GameObject("Label");
        label.transform.SetParent(toggleGO.transform);
        label.AddComponent<RectTransform>();
        Text labelText = label.AddComponent<Text>();
        labelText.text = name;
        
        // Setup toggle references
        toggle.targetGraphic = background.GetComponent<Image>();
        toggle.graphic = checkmark.GetComponent<Image>();
        
        Debug.Log($"Created Toggle: {name}");
    }
    
    private void CreateButton(string name, string text, Transform parent)
    {
        GameObject buttonGO = new GameObject(name);
        buttonGO.transform.SetParent(parent);
        
        // Add RectTransform
        RectTransform rectTransform = buttonGO.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(100, 30);
        
        // Add Image component
        Image image = buttonGO.AddComponent<Image>();
        
        // Add Button component
        Button button = buttonGO.AddComponent<Button>();
        button.targetGraphic = image;
        
        // Create Text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform);
        textGO.AddComponent<RectTransform>();
        Text buttonText = textGO.AddComponent<Text>();
        buttonText.text = text;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        Debug.Log($"Created Button: {name}");
    }
} 