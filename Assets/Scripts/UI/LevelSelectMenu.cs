using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Level selection menu - shows available levels and allows player to select one
/// </summary>
public class LevelSelectMenu : MonoBehaviour
{
    [Header("UI References")]
    public Button level1Button;
    public Text level1Text;
    
    [Header("Level Settings")]
    public string level1SceneName = "SampleScene"; // Name of the scene for Level 1
    
    [Header("Menu Settings")]
    public Color normalButtonColor = Color.white;
    public Color hoverButtonColor = new Color(0.8f, 0.8f, 0.8f);
    public Color pressedButtonColor = new Color(0.6f, 0.6f, 0.6f);
    
    [Header("Title Settings")]
    public Text titleText;
    public string gameTitle = "LUMEN SHIFT";
    
    void Start()
    {
        SetupMenu();
    }
    
    /// <summary>
    /// Setup the level selection menu
    /// </summary>
    void SetupMenu()
    {
        // Create Canvas if it doesn't exist
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Create EventSystem if it doesn't exist
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }
        
        // Create title if it doesn't exist
        if (titleText == null)
        {
            CreateTitle(canvas.transform);
        }
        
        // Create Level 1 button if it doesn't exist
        if (level1Button == null)
        {
            CreateLevel1Button(canvas.transform);
        }
        else
        {
            // Setup existing button
            level1Button.onClick.AddListener(LoadLevel1);
        }
    }
    
    /// <summary>
    /// Create game title
    /// </summary>
    void CreateTitle(Transform parent)
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(parent, false);
        
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.8f);
        titleRect.anchorMax = new Vector2(0.5f, 0.8f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = new Vector2(400f, 80f);
        
        titleText = titleObj.AddComponent<Text>();
        titleText.text = gameTitle;
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 48;
        titleText.color = new Color(1f, 0.9f, 0.2f); // Gold color
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.fontStyle = FontStyle.Bold;
    }
    
    /// <summary>
    /// Create the Level 1 button
    /// </summary>
    void CreateLevel1Button(Transform parent)
    {
        // Create button GameObject
        GameObject buttonObj = new GameObject("Level1Button");
        buttonObj.transform.SetParent(parent, false);
        
        // Add RectTransform and position it
        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0f, -50f); // Below title
        rectTransform.sizeDelta = new Vector2(250f, 60f);
        
        // Add Image component for button background
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = normalButtonColor;
        
        // Add Button component
        level1Button = buttonObj.AddComponent<Button>();
        level1Button.targetGraphic = buttonImage;
        
        // Create button colors
        ColorBlock colors = level1Button.colors;
        colors.normalColor = normalButtonColor;
        colors.highlightedColor = hoverButtonColor;
        colors.pressedColor = pressedButtonColor;
        colors.selectedColor = hoverButtonColor;
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f);
        level1Button.colors = colors;
        
        // Add click listener
        level1Button.onClick.AddListener(LoadLevel1);
        
        // Create text for button
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        level1Text = textObj.AddComponent<Text>();
        level1Text.text = "Level 1";
        level1Text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        level1Text.fontSize = 24;
        level1Text.color = Color.black;
        level1Text.alignment = TextAnchor.MiddleCenter;
        
        // Make text not interactable (so clicks go to button)
        textObj.AddComponent<CanvasGroup>().interactable = false;
    }
    
    /// <summary>
    /// Load Level 1 scene
    /// </summary>
    public void LoadLevel1()
    {
        Debug.Log($"Loading Level 1: {level1SceneName}");
        SceneManager.LoadScene(level1SceneName);
    }
}

