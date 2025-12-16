using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;

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
    public string displayFormat = "⚡ {0}/{1}"; // Format: "⚡ 5/12"
    public Color textColor = new Color(1f, 0.9f, 0f); // Bright yellow/gold
    public int fontSize = 36; // Larger font for visibility
    
    private Canvas canvas;
    private RectTransform counterRect;
    private Text shadowText; // Shadow text for better visibility
    private int totalBoltsInLevel = 12; // Total number of lightning bolts in the level
    private int originalTotalBolts = 12; // Store original total (before any are collected/destroyed)
    
    void Start()
    {
        // Find or create GameManager
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
        
        // Find or create Canvas attached to camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        if (mainCamera != null)
        {
            // Check if camera already has a canvas
            canvas = mainCamera.GetComponentInChildren<Canvas>();
            if (canvas == null)
            {
                CreateCanvasForCamera(mainCamera);
            }
            else
            {
                canvas = mainCamera.GetComponentInChildren<Canvas>();
            }
        }
        else
        {
            // Fallback: create overlay canvas
            canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                CreateCanvas();
            }
        }
        
        // Ensure canvas is set correctly
        if (canvas != null)
        {
            canvas.sortingOrder = 10; // High sorting order to ensure visibility
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
        
        // Get total bolt count from GameManager and store original
        RefreshTotalBolts();
        originalTotalBolts = totalBoltsInLevel; // Store original count
    }
    
    void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnBoltCollected -= UpdateCounter;
        }
    }
    
    /// <summary>
    /// Create a Canvas attached to the camera (ScreenSpaceCamera mode)
    /// </summary>
    void CreateCanvasForCamera(Camera targetCamera)
    {
        GameObject canvasObj = new GameObject("CameraCanvas");
        canvasObj.transform.SetParent(targetCamera.transform, false);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = targetCamera;
        canvas.planeDistance = 1f; // Close to camera
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
    }
    
    /// <summary>
    /// Create a Canvas if one doesn't exist (fallback)
    /// </summary>
    void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("Canvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create or update EventSystem to use Input System
        UnityEngine.EventSystems.EventSystem eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystem = eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            // Use InputSystemUIInputModule for Input System compatibility
            eventSystemObj.AddComponent<InputSystemUIInputModule>();
        }
        else
        {
            // If EventSystem exists but uses old input module, replace it
            var oldModule = eventSystem.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (oldModule != null)
            {
                Destroy(oldModule);
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }
            // If no input module exists, add InputSystemUIInputModule
            else if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }
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
        RefreshTotalBolts();
        counterText.text = string.Format(displayFormat, 0, totalBoltsInLevel);
        counterText.color = textColor;
        counterText.fontSize = fontSize;
        counterText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        counterText.alignment = TextAnchor.UpperRight;
        counterText.fontStyle = FontStyle.Bold; // Make it bold for better visibility
        
        // Add outline/shadow effect by creating a duplicate behind
        GameObject shadowObj = new GameObject("Shadow");
        shadowObj.transform.SetParent(textObj.transform, false);
        RectTransform shadowRect = shadowObj.AddComponent<RectTransform>();
        shadowRect.anchorMin = Vector2.zero;
        shadowRect.anchorMax = Vector2.one;
        shadowRect.sizeDelta = Vector2.zero;
        shadowRect.anchoredPosition = new Vector2(2f, -2f); // Offset for shadow
        
        this.shadowText = shadowObj.AddComponent<Text>();
        this.shadowText.text = string.Format(displayFormat, 0, totalBoltsInLevel);
        this.shadowText.color = Color.black;
        this.shadowText.fontSize = fontSize;
        this.shadowText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        this.shadowText.alignment = TextAnchor.UpperRight;
        this.shadowText.fontStyle = FontStyle.Bold;
        
        // Make shadow not interactable
        shadowObj.AddComponent<CanvasGroup>().interactable = false;
        
        // Position in top right corner
        counterRect = textObj.GetComponent<RectTransform>();
        if (counterRect != null)
        {
            counterRect.anchorMin = new Vector2(1f, 1f);
            counterRect.anchorMax = new Vector2(1f, 1f);
            counterRect.pivot = new Vector2(1f, 1f);
            counterRect.anchoredPosition = new Vector2(-30f, -30f); // 30px from top-right corner
            counterRect.sizeDelta = new Vector2(200f, 50f); // Ensure text has space
        }
    }
    
    /// <summary>
    /// Refresh total bolts count from GameManager
    /// </summary>
    public void RefreshTotalBolts()
    {
        // Count all lightning bolts in the scene (including inactive ones)
        LightningBolt[] allBolts = FindObjectsByType<LightningBolt>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int count = 0;
        foreach (LightningBolt bolt in allBolts)
        {
            if (bolt != null && !bolt.isCollected)
            {
                count++;
            }
        }
        totalBoltsInLevel = count;
        
        // Also update display if counter text exists
        if (counterText != null && originalTotalBolts == 0)
        {
            originalTotalBolts = totalBoltsInLevel;
            UpdateCounter(gameManager != null ? gameManager.totalBoltsCollected : 0);
        }
    }
    
    /// <summary>
    /// Update the counter display
    /// </summary>
    public void UpdateCounter(int count)
    {
        // Use original total bolts count (don't recalculate - bolts may be disabled)
        // Only refresh if we haven't stored the original yet
        if (originalTotalBolts == 0)
        {
            RefreshTotalBolts();
            originalTotalBolts = totalBoltsInLevel;
        }
        
        // Use original total, not current count (which may be lower due to disabled bolts)
        string displayText = string.Format(displayFormat, count, originalTotalBolts);
        
        if (counterText != null)
        {
            counterText.text = displayText;
        }
        
        if (shadowText != null)
        {
            shadowText.text = displayText;
        }
    }
    
    /// <summary>
    /// Reset the counter to use original total (called when level resets)
    /// </summary>
    public void ResetToOriginalTotal()
    {
        RefreshTotalBolts();
        originalTotalBolts = totalBoltsInLevel;
        UpdateCounter(0);
    }
}

