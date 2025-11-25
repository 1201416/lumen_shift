using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI component to display lightning bolt count in the top right corner
/// Automatically creates UI elements if they don't exist
/// </summary>
public class LightningBoltCounter : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public Text counterText;
    
    [Header("Display Settings")]
    public string displayFormat = "Lightning: {0}"; // Format: "Lightning: 5"
    public Color textColor = Color.yellow;
    public int fontSize = 24;
    
    private Canvas canvas;
    private RectTransform counterRect;
    
    void Start()
    {
        // Find or create GameManager
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
        
        // Find or create Canvas
        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            CreateCanvas();
        }
        
        // Find or create text component
        if (counterText == null)
        {
            CreateCounterText();
        }
        
        // Subscribe to bolt collection events
        if (gameManager != null)
        {
            gameManager.OnBoltCollected += UpdateCounter;
            // Set initial count
            UpdateCounter(gameManager.totalBoltsCollected);
        }
    }
    
    void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnBoltCollected -= UpdateCounter;
        }
    }
    
    /// <summary>
    /// Create a Canvas if one doesn't exist
    /// </summary>
    void CreateCanvas()
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
    
    /// <summary>
    /// Create the counter text in the top right corner
    /// </summary>
    void CreateCounterText()
    {
        GameObject textObj = new GameObject("LightningBoltCounterText");
        textObj.transform.SetParent(canvas.transform, false);
        
        // Use legacy Text component
        counterText = textObj.AddComponent<Text>();
        counterText.text = string.Format(displayFormat, 0);
        counterText.color = textColor;
        counterText.fontSize = fontSize;
        counterText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        counterText.alignment = TextAnchor.UpperRight;
        
        // Position in top right corner
        counterRect = textObj.GetComponent<RectTransform>();
        if (counterRect != null)
        {
            counterRect.anchorMin = new Vector2(1f, 1f);
            counterRect.anchorMax = new Vector2(1f, 1f);
            counterRect.pivot = new Vector2(1f, 1f);
            counterRect.anchoredPosition = new Vector2(-20f, -20f); // 20px from top-right corner
        }
    }
    
    /// <summary>
    /// Update the counter display
    /// </summary>
    void UpdateCounter(int count)
    {
        if (counterText != null)
        {
            counterText.text = string.Format(displayFormat, count);
        }
    }
}

