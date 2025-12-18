using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Main Menu - shows the game logo and a "New Game" button.
/// The game only starts when the player clicks the button.
/// This menu appears after the BootScreen disappears.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    public Canvas menuCanvas;
    public Image logoImage;
    public Button newGameButton;
    
    [Header("Settings")]
    public string logoResourceName = "menu_logo";
    public Color buttonColor = new Color(0.2f, 0.6f, 1f); // Blue button
    public Color buttonHoverColor = new Color(0.3f, 0.7f, 1f);
    public Color buttonTextColor = Color.white;
    public int buttonFontSize = 32;
    
    private bool isMenuActive = true;
    
    /// <summary>
    /// Static flag to indicate if main menu has been dismissed
    /// </summary>
    public static bool HasStartedGame { get; private set; } = false;
    
    /// <summary>
    /// Static reference to the current MainMenu instance
    /// </summary>
    public static MainMenu Instance { get; private set; }
    
    void Awake()
    {
        Instance = this;
        HasStartedGame = false;
    }
    
    void Start()
    {
        CreateMainMenuUI();
    }
    
    /// <summary>
    /// Create the main menu UI programmatically
    /// </summary>
    void CreateMainMenuUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("MainMenuCanvas");
        canvasObj.transform.SetParent(transform, false);
        menuCanvas = canvasObj.AddComponent<Canvas>();
        menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        menuCanvas.sortingOrder = 500; // Above game, below boot screen
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Ensure EventSystem exists with Input System support
        EnsureEventSystem();
        
        // Create black background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = Color.black;
        
        // Create logo image
        GameObject logoObj = new GameObject("Logo");
        logoObj.transform.SetParent(canvasObj.transform, false);
        logoImage = logoObj.AddComponent<Image>();
        RectTransform logoRect = logoObj.GetComponent<RectTransform>();
        logoRect.anchorMin = new Vector2(0.5f, 0.65f);
        logoRect.anchorMax = new Vector2(0.5f, 0.65f);
        logoRect.pivot = new Vector2(0.5f, 0.5f);
        logoRect.sizeDelta = new Vector2(800f, 400f);
        logoRect.anchoredPosition = Vector2.zero;
        
        // Load logo sprite from Resources
        Sprite logoSprite = LoadLogoSprite();
        if (logoSprite != null)
        {
            logoImage.sprite = logoSprite;
            logoImage.preserveAspect = true;
            Debug.Log("MainMenu: Logo loaded successfully");
        }
        else
        {
            // Fallback: show text if no logo found
            logoImage.color = Color.clear;
            CreateFallbackTitle(canvasObj.transform);
            Debug.LogWarning("MainMenu: menu_logo.png not found in Resources folder");
        }
        
        // Create "New Game" button
        CreateNewGameButton(canvasObj.transform);
        
        Debug.Log("MainMenu: UI created successfully");
    }
    
    /// <summary>
    /// Load the logo sprite from Resources folder
    /// </summary>
    Sprite LoadLogoSprite()
    {
        // Try loading as Sprite first
        Sprite sprite = Resources.Load<Sprite>(logoResourceName);
        if (sprite != null) return sprite;
        
        // Try loading as Texture2D and convert to Sprite
        Texture2D tex = Resources.Load<Texture2D>(logoResourceName);
        if (tex != null)
        {
            sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            return sprite;
        }
        
        return null;
    }
    
    /// <summary>
    /// Create fallback title text if logo is not found
    /// </summary>
    void CreateFallbackTitle(Transform parent)
    {
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(parent, false);
        
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.65f);
        titleRect.anchorMax = new Vector2(0.5f, 0.65f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.sizeDelta = new Vector2(600f, 100f);
        titleRect.anchoredPosition = Vector2.zero;
        
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "LUMEN SHIFT";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (titleText.font == null)
            titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 64;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;
    }
    
    /// <summary>
    /// Create the "New Game" button
    /// </summary>
    void CreateNewGameButton(Transform parent)
    {
        // Button container
        GameObject buttonObj = new GameObject("NewGameButton");
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.25f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.25f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(300f, 70f);
        buttonRect.anchoredPosition = Vector2.zero;
        
        // Button background image
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = buttonColor;
        
        // Add rounded corners effect (optional - basic color fill)
        buttonImage.type = Image.Type.Sliced;
        
        // Button component
        newGameButton = buttonObj.AddComponent<Button>();
        newGameButton.targetGraphic = buttonImage;
        
        // Button colors
        ColorBlock colors = newGameButton.colors;
        colors.normalColor = buttonColor;
        colors.highlightedColor = buttonHoverColor;
        colors.pressedColor = new Color(buttonColor.r * 0.8f, buttonColor.g * 0.8f, buttonColor.b * 0.8f);
        colors.selectedColor = buttonHoverColor;
        newGameButton.colors = colors;
        
        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = "New Game";
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (buttonText.font == null)
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buttonText.fontSize = buttonFontSize;
        buttonText.fontStyle = FontStyle.Bold;
        buttonText.color = buttonTextColor;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        // Add click listener
        newGameButton.onClick.AddListener(OnNewGameClicked);
        
        Debug.Log("MainMenu: New Game button created");
    }
    
    /// <summary>
    /// Ensure EventSystem exists with Input System UI module
    /// </summary>
    void EnsureEventSystem()
    {
        UnityEngine.EventSystems.EventSystem eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystem = eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<InputSystemUIInputModule>();
            Debug.Log("MainMenu: Created EventSystem with InputSystemUIInputModule");
        }
        else
        {
            // Ensure it has InputSystemUIInputModule
            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                // Remove old StandaloneInputModule if present
                var oldModule = eventSystem.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                if (oldModule != null)
                {
                    Destroy(oldModule);
                }
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                Debug.Log("MainMenu: Added InputSystemUIInputModule to existing EventSystem");
            }
        }
    }
    
    /// <summary>
    /// Called when "New Game" button is clicked
    /// </summary>
    void OnNewGameClicked()
    {
        Debug.Log("MainMenu: New Game clicked - starting game!");
        HasStartedGame = true;
        isMenuActive = false;
        
        // Hide the menu
        if (menuCanvas != null)
        {
            menuCanvas.enabled = false;
        }
        
        // Destroy the menu object
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Check if the main menu is currently active
    /// </summary>
    public bool IsMenuActive()
    {
        return isMenuActive;
    }
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
