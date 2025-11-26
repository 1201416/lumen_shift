using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// Conclusion screen UI - shows when player completes the level with all lightning bolts
/// </summary>
public class ConclusionScreen : MonoBehaviour
{
    [Header("UI References")]
    public Canvas conclusionCanvas;
    public Text conclusionMessageText;
    public Button continueButton;
    public Text continueButtonText;
    
    [Header("Settings")]
    public string conclusionMessage = "Congratulations!\nYou've completed the level!";
    public string continueButtonTextStr = "Continue";
    
    private bool isShowing = false;
    
    void Start()
    {
        // Create UI if it doesn't exist
        if (conclusionCanvas == null)
        {
            CreateConclusionUI();
        }
        
        // Hide by default
        HideConclusionScreen();
    }
    
    void Update()
    {
        // Check for Enter/Space key to continue (only when conclusion screen is showing)
        if (isShowing && Keyboard.current != null)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame || 
                Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                Continue();
            }
        }
    }
    
    /// <summary>
    /// Create conclusion screen UI
    /// </summary>
    void CreateConclusionUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("ConclusionCanvas");
        conclusionCanvas = canvasObj.AddComponent<Canvas>();
        conclusionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        conclusionCanvas.sortingOrder = 200; // Above everything, including death screen
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create background overlay (semi-transparent dark)
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.8f); // Dark semi-transparent
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        // Create conclusion message
        GameObject messageObj = new GameObject("ConclusionMessage");
        messageObj.transform.SetParent(canvasObj.transform, false);
        conclusionMessageText = messageObj.AddComponent<Text>();
        conclusionMessageText.text = conclusionMessage;
        conclusionMessageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        conclusionMessageText.fontSize = 48;
        conclusionMessageText.color = Color.white;
        conclusionMessageText.alignment = TextAnchor.MiddleCenter;
        conclusionMessageText.fontStyle = FontStyle.Bold;
        
        RectTransform msgRect = messageObj.GetComponent<RectTransform>();
        msgRect.anchorMin = new Vector2(0.5f, 0.6f);
        msgRect.anchorMax = new Vector2(0.5f, 0.6f);
        msgRect.pivot = new Vector2(0.5f, 0.5f);
        msgRect.sizeDelta = new Vector2(600f, 120f);
        msgRect.anchoredPosition = Vector2.zero;
        
        // Create continue button
        GameObject buttonObj = new GameObject("ContinueButton");
        buttonObj.transform.SetParent(canvasObj.transform, false);
        
        RectTransform btnRect = buttonObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.4f);
        btnRect.anchorMax = new Vector2(0.5f, 0.4f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.sizeDelta = new Vector2(200f, 60f);
        btnRect.anchoredPosition = Vector2.zero;
        
        Image btnImage = buttonObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.6f, 0.2f, 0.9f); // Green
        
        continueButton = buttonObj.AddComponent<Button>();
        continueButton.targetGraphic = btnImage;
        
        ColorBlock colors = continueButton.colors;
        colors.normalColor = new Color(0.2f, 0.6f, 0.2f);
        colors.highlightedColor = new Color(0.3f, 0.7f, 0.3f);
        colors.pressedColor = new Color(0.1f, 0.5f, 0.1f);
        continueButton.colors = colors;
        
        continueButton.onClick.AddListener(Continue);
        
        // Create button text
        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.sizeDelta = Vector2.zero;
        
        continueButtonText = btnTextObj.AddComponent<Text>();
        continueButtonText.text = continueButtonTextStr;
        continueButtonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        continueButtonText.fontSize = 32;
        continueButtonText.color = Color.white;
        continueButtonText.alignment = TextAnchor.MiddleCenter;
        continueButtonText.fontStyle = FontStyle.Bold;
        
        btnTextObj.AddComponent<CanvasGroup>().interactable = false;
    }
    
    /// <summary>
    /// Show conclusion screen
    /// </summary>
    public void ShowConclusionScreen()
    {
        if (isShowing) return;
        isShowing = true;
        
        if (conclusionCanvas != null)
        {
            conclusionCanvas.gameObject.SetActive(true);
        }
        
        // Disable player movement
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.enabled = false;
            }
            
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
    
    /// <summary>
    /// Hide conclusion screen
    /// </summary>
    public void HideConclusionScreen()
    {
        isShowing = false;
        
        if (conclusionCanvas != null)
        {
            conclusionCanvas.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Continue button action - return to menu or load next level
    /// </summary>
    public void Continue()
    {
        HideConclusionScreen();
        
        // For now, just reload the scene (you can change this to load a menu scene)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
        // Or uncomment to load a menu scene:
        // SceneManager.LoadScene("MainMenu");
    }
}

