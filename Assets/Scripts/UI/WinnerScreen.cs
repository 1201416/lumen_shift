using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// Winner screen UI - shows when player completes a level
/// Provides options to Retry or go to Next Level
/// </summary>
public class WinnerScreen : MonoBehaviour
{
    [Header("UI References")]
    public Canvas winnerCanvas;
    public Text winnerMessageText;
    public Button retryButton;
    public Text retryButtonText;
    public Button nextLevelButton;
    public Text nextLevelButtonText;
    
    [Header("Settings")]
    public string winnerMessage = "Level Complete!\nCongratulations!";
    public string retryButtonTextStr = "Retry";
    public string nextLevelButtonTextStr = "Next Level";
    
    [Header("Level Navigation")]
    public int currentLevelNumber = 1; // Current level (1-5)
    public string[] levelSceneNames = new string[] 
    { 
        "SampleScene", // Level 1
        "Level2Scene", // Level 2
        "Level3Scene", // Level 3
        "Level4Scene", // Level 4
        "Level5Scene"  // Level 5
    };
    
    private bool isShowing = false;
    private FinishLine finishLine;
    
    void Start()
    {
        // Try to find FinishLine to get level info
        finishLine = FindFirstObjectByType<FinishLine>();
        
        // Detect current level from scene name or level generators
        DetectCurrentLevel();
        
        // Create UI if it doesn't exist
        if (winnerCanvas == null)
        {
            CreateWinnerUI();
        }
        
        // Hide by default
        HideWinnerScreen();
    }
    
    void Update()
    {
        // Check for keyboard shortcuts (only when winner screen is showing)
        if (isShowing && Keyboard.current != null)
        {
            // R key for retry
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                RetryLevel();
            }
            // N key or Enter/Space for next level
            else if (Keyboard.current.nKey.wasPressedThisFrame ||
                     Keyboard.current.enterKey.wasPressedThisFrame || 
                     Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                GoToNextLevel();
            }
        }
    }
    
    /// <summary>
    /// Detect which level is currently being played
    /// </summary>
    void DetectCurrentLevel()
    {
        // Try to detect from LevelManager first (most reliable)
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null)
        {
            currentLevelNumber = levelManager.currentLevel;
            return;
        }
        
        // Try to detect from level generators
        if (FindFirstObjectByType<FirstLevelGenerator>() != null)
        {
            currentLevelNumber = 1;
        }
        else if (FindFirstObjectByType<Level2Generator>() != null)
        {
            currentLevelNumber = 2;
        }
        else if (FindFirstObjectByType<Level3Generator>() != null)
        {
            currentLevelNumber = 3;
        }
        else if (FindFirstObjectByType<Level4Generator>() != null)
        {
            currentLevelNumber = 4;
        }
        else if (FindFirstObjectByType<Level5Generator>() != null)
        {
            currentLevelNumber = 5;
        }
        else if (FindFirstObjectByType<Level6Generator>() != null)
        {
            currentLevelNumber = 6;
        }
        else if (FindFirstObjectByType<Level7Generator>() != null)
        {
            currentLevelNumber = 7;
        }
        else if (FindFirstObjectByType<Level8Generator>() != null)
        {
            currentLevelNumber = 8;
        }
        else if (FindFirstObjectByType<Level9Generator>() != null)
        {
            currentLevelNumber = 9;
        }
        else if (FindFirstObjectByType<Level10Generator>() != null)
        {
            currentLevelNumber = 10;
        }
        else if (FindFirstObjectByType<Level11Generator>() != null)
        {
            currentLevelNumber = 11;
        }
        else if (FindFirstObjectByType<Level12Generator>() != null)
        {
            currentLevelNumber = 12;
        }
        else if (FindFirstObjectByType<Level13Generator>() != null)
        {
            currentLevelNumber = 13;
        }
        else if (FindFirstObjectByType<Level14Generator>() != null)
        {
            currentLevelNumber = 14;
        }
        else if (FindFirstObjectByType<Level15Generator>() != null)
        {
            currentLevelNumber = 15;
        }
        else
        {
            // Try to detect from scene name
            string sceneName = SceneManager.GetActiveScene().name;
            for (int i = 0; i < levelSceneNames.Length; i++)
            {
                if (sceneName.Contains((i + 1).ToString()) || sceneName == levelSceneNames[i])
                {
                    currentLevelNumber = i + 1;
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// Create winner screen UI
    /// </summary>
    void CreateWinnerUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("WinnerCanvas");
        winnerCanvas = canvasObj.AddComponent<Canvas>();
        winnerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        winnerCanvas.sortingOrder = 200; // Above everything
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create background overlay (semi-transparent dark)
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.85f); // Dark semi-transparent
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        // Create winner message
        GameObject messageObj = new GameObject("WinnerMessage");
        messageObj.transform.SetParent(canvasObj.transform, false);
        winnerMessageText = messageObj.AddComponent<Text>();
        winnerMessageText.text = winnerMessage;
        winnerMessageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        winnerMessageText.fontSize = 56;
        winnerMessageText.color = new Color(1f, 0.84f, 0f); // Gold color
        winnerMessageText.alignment = TextAnchor.MiddleCenter;
        winnerMessageText.fontStyle = FontStyle.Bold;
        
        RectTransform msgRect = messageObj.GetComponent<RectTransform>();
        msgRect.anchorMin = new Vector2(0.5f, 0.65f);
        msgRect.anchorMax = new Vector2(0.5f, 0.65f);
        msgRect.pivot = new Vector2(0.5f, 0.5f);
        msgRect.sizeDelta = new Vector2(700f, 150f);
        msgRect.anchoredPosition = Vector2.zero;
        
        // Create Retry button
        GameObject retryButtonObj = new GameObject("RetryButton");
        retryButtonObj.transform.SetParent(canvasObj.transform, false);
        
        RectTransform retryBtnRect = retryButtonObj.AddComponent<RectTransform>();
        retryBtnRect.anchorMin = new Vector2(0.5f, 0.4f);
        retryBtnRect.anchorMax = new Vector2(0.5f, 0.4f);
        retryBtnRect.pivot = new Vector2(0.5f, 0.5f);
        retryBtnRect.sizeDelta = new Vector2(220f, 70f);
        retryBtnRect.anchoredPosition = new Vector2(-130f, 0f); // Left side
        
        Image retryBtnImage = retryButtonObj.AddComponent<Image>();
        retryBtnImage.color = new Color(0.4f, 0.4f, 0.4f, 0.95f); // Gray
        
        retryButton = retryButtonObj.AddComponent<Button>();
        retryButton.targetGraphic = retryBtnImage;
        
        ColorBlock retryColors = retryButton.colors;
        retryColors.normalColor = new Color(0.4f, 0.4f, 0.4f);
        retryColors.highlightedColor = new Color(0.6f, 0.6f, 0.6f);
        retryColors.pressedColor = new Color(0.3f, 0.3f, 0.3f);
        retryButton.colors = retryColors;
        
        retryButton.onClick.AddListener(RetryLevel);
        
        // Create retry button text
        GameObject retryBtnTextObj = new GameObject("Text");
        retryBtnTextObj.transform.SetParent(retryButtonObj.transform, false);
        
        RectTransform retryBtnTextRect = retryBtnTextObj.AddComponent<RectTransform>();
        retryBtnTextRect.anchorMin = Vector2.zero;
        retryBtnTextRect.anchorMax = Vector2.one;
        retryBtnTextRect.sizeDelta = Vector2.zero;
        
        retryButtonText = retryBtnTextObj.AddComponent<Text>();
        retryButtonText.text = "(R) " + retryButtonTextStr; // Add "(R)" prefix
        retryButtonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        retryButtonText.fontSize = 36;
        retryButtonText.color = Color.white;
        retryButtonText.alignment = TextAnchor.MiddleCenter;
        retryButtonText.fontStyle = FontStyle.Bold;
        
        // Enable retry button
        retryButton.interactable = true;
        
        // Create Next Level button
        GameObject nextLevelButtonObj = new GameObject("NextLevelButton");
        nextLevelButtonObj.transform.SetParent(canvasObj.transform, false);
        
        RectTransform nextBtnRect = nextLevelButtonObj.AddComponent<RectTransform>();
        nextBtnRect.anchorMin = new Vector2(0.5f, 0.4f);
        nextBtnRect.anchorMax = new Vector2(0.5f, 0.4f);
        nextBtnRect.pivot = new Vector2(0.5f, 0.5f);
        nextBtnRect.sizeDelta = new Vector2(220f, 70f);
        nextBtnRect.anchoredPosition = new Vector2(130f, 0f); // Right side
        
        Image nextBtnImage = nextLevelButtonObj.AddComponent<Image>();
        nextBtnImage.color = new Color(0.2f, 0.7f, 0.2f, 0.95f); // Green
        
        nextLevelButton = nextLevelButtonObj.AddComponent<Button>();
        nextLevelButton.targetGraphic = nextBtnImage;
        
        ColorBlock nextColors = nextLevelButton.colors;
        nextColors.normalColor = new Color(0.2f, 0.7f, 0.2f);
        nextColors.highlightedColor = new Color(0.3f, 0.8f, 0.3f);
        nextColors.pressedColor = new Color(0.1f, 0.6f, 0.1f);
        nextLevelButton.colors = nextColors;
        
        nextLevelButton.onClick.AddListener(GoToNextLevel);
        
        // Create next level button text
        GameObject nextBtnTextObj = new GameObject("Text");
        nextBtnTextObj.transform.SetParent(nextLevelButtonObj.transform, false);
        
        RectTransform nextBtnTextRect = nextBtnTextObj.AddComponent<RectTransform>();
        nextBtnTextRect.anchorMin = Vector2.zero;
        nextBtnTextRect.anchorMax = Vector2.one;
        nextBtnTextRect.sizeDelta = Vector2.zero;
        
        nextLevelButtonText = nextBtnTextObj.AddComponent<Text>();
        nextLevelButtonText.text = nextLevelButtonTextStr;
        nextLevelButtonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        nextLevelButtonText.fontSize = 36;
        nextLevelButtonText.color = Color.white;
        nextLevelButtonText.alignment = TextAnchor.MiddleCenter;
        nextLevelButtonText.fontStyle = FontStyle.Bold;
        
        nextBtnTextObj.AddComponent<CanvasGroup>().interactable = false;
        
        // Update button text with level number
        UpdateNextLevelButtonText();
    }
    
    /// <summary>
    /// Update the Next Level button text to show which level is next
    /// </summary>
    void UpdateNextLevelButtonText()
    {
        if (nextLevelButtonText != null)
        {
            if (HasNextLevel())
            {
                nextLevelButtonText.text = $"Level {currentLevelNumber + 1} (N)"; // Add "(N)" suffix
                // Enable button
                if (nextLevelButton != null)
                {
                    nextLevelButton.interactable = true;
                    Image btnImage = nextLevelButton.GetComponent<Image>();
                    if (btnImage != null)
                    {
                        btnImage.color = new Color(0.2f, 0.7f, 0.2f, 0.95f); // Green
                    }
                }
            }
            else
            {
                // Final level - show "Play Again" instead
                nextLevelButtonText.text = "Play Again (N)"; // Add "(N)" suffix
                // Keep button enabled but change color
                if (nextLevelButton != null)
                {
                    nextLevelButton.interactable = true;
                    Image btnImage = nextLevelButton.GetComponent<Image>();
                    if (btnImage != null)
                    {
                        btnImage.color = new Color(0.8f, 0.6f, 0.2f, 0.95f); // Gold/orange
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Check if there's a next level available
    /// </summary>
    bool HasNextLevel()
    {
        return currentLevelNumber < 15; // Max 15 levels
    }
    
    /// <summary>
    /// Show winner screen
    /// </summary>
    public void ShowWinnerScreen()
    {
        if (isShowing) return;
        isShowing = true;
        
        // Update level detection
        DetectCurrentLevel();
        UpdateNextLevelButtonText();
        
        // Update winner message with level number
        if (winnerMessageText != null)
        {
            if (currentLevelNumber == 15)
            {
                // Special message for final level
                winnerMessageText.text = "FINAL LEVEL COMPLETE!\n\nCongratulations!\nYou've mastered Lumen-Shift!";
                winnerMessageText.fontSize = 48; // Slightly smaller for longer text
            }
            else
            {
                winnerMessageText.text = $"Level {currentLevelNumber} Complete!\nCongratulations!";
                winnerMessageText.fontSize = 56; // Normal size
            }
        }
        
        if (winnerCanvas != null)
        {
            winnerCanvas.gameObject.SetActive(true);
        }
        
        // Player movement is already stopped by FinishLine after 0.5 seconds
        // Just ensure it's still disabled
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
        
        // Don't pause time - let animations continue
        // Time.timeScale = 0f; // Removed - keep time running
    }
    
    /// <summary>
    /// Hide winner screen
    /// </summary>
    public void HideWinnerScreen()
    {
        isShowing = false;
        
        if (winnerCanvas != null)
        {
            winnerCanvas.gameObject.SetActive(false);
        }
        
        // Resume time
        Time.timeScale = 1f;
    }
    
    /// <summary>
    /// Retry button action - reload current level
    /// </summary>
    public void RetryLevel()
    {
        // Resume time before loading scene
        Time.timeScale = 1f;
        HideWinnerScreen();
        
        // Try to use LevelManager first (for same-scene progression)
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.ReloadCurrentLevel();
            return;
        }
        
        // Fallback: reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    /// <summary>
    /// Next Level button action - load next level
    /// </summary>
    public void GoToNextLevel()
    {
        // Resume time before loading scene
        Time.timeScale = 1f;
        HideWinnerScreen();
        
        // Use LevelManager for same-scene progression (all levels are in one scene)
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null)
        {
            // Ensure we have the correct current level before loading next
            // Use WinnerScreen's detected level if LevelManager's is incorrect
            if (levelManager.currentLevel != currentLevelNumber)
            {
                Debug.LogWarning($"LevelManager currentLevel ({levelManager.currentLevel}) doesn't match WinnerScreen currentLevelNumber ({currentLevelNumber}). Updating LevelManager.");
                levelManager.currentLevel = currentLevelNumber;
            }
            
            Debug.Log($"WinnerScreen: Going to next level. Current: {currentLevelNumber}, Next: {currentLevelNumber + 1}");
            levelManager.LoadNextLevel();
            
            // Enable player movement after level loads (with a small delay to ensure level generation completes)
            Invoke(nameof(EnablePlayerMovement), 0.1f);
            return;
        }

        // If LevelManager doesn't exist, log error and reload current scene
        Debug.LogError("LevelManager not found! Cannot load next level. Please ensure LevelManager exists in the scene.");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    /// <summary>
    /// Enable player movement (called after level loads)
    /// </summary>
    void EnablePlayerMovement()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.enabled = true;
            }
            
            // Reset velocity
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
    
    /// <summary>
    /// Set the current level number manually
    /// </summary>
    public void SetCurrentLevel(int levelNumber)
    {
        currentLevelNumber = Mathf.Clamp(levelNumber, 1, 15);
        UpdateNextLevelButtonText();
    }
}
