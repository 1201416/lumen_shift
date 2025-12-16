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
        // Check for Enter/Space key to continue to next level (only when winner screen is showing)
        if (isShowing && Keyboard.current != null)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame || 
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
        retryButtonText.text = retryButtonTextStr;
        retryButtonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        retryButtonText.fontSize = 36;
        retryButtonText.color = Color.white;
        retryButtonText.alignment = TextAnchor.MiddleCenter;
        retryButtonText.fontStyle = FontStyle.Bold;
        
        retryBtnTextObj.AddComponent<CanvasGroup>().interactable = false;
        
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
                nextLevelButtonText.text = $"Level {currentLevelNumber + 1}";
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
                nextLevelButtonText.text = "Play Again";
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
        return currentLevelNumber < 5; // Max 5 levels
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
            if (currentLevelNumber == 5)
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
        
        // Pause time (optional - can be removed if you want animations to continue)
        Time.timeScale = 0f;
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
        
        // Try to use LevelManager first (for same-scene progression)
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.LoadNextLevel();
            return;
        }
        
        // Fallback: use scene-based loading
        if (HasNextLevel())
        {
            int nextLevel = currentLevelNumber + 1;
            
            // Try to load next level scene
            if (nextLevel <= levelSceneNames.Length && !string.IsNullOrEmpty(levelSceneNames[nextLevel - 1]))
            {
                string nextSceneName = levelSceneNames[nextLevel - 1];
                Debug.Log($"Loading Level {nextLevel}: {nextSceneName}");
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                // Fallback: reload current scene if next level doesn't exist
                Debug.LogWarning($"Next level scene not found! Reloading current level.");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
        else
        {
            // Final level complete - return to first level (Level 1)
            Debug.Log("All levels complete! Returning to Level 1...");
            // Load first level scene
            if (levelSceneNames.Length > 0 && !string.IsNullOrEmpty(levelSceneNames[0]))
            {
                SceneManager.LoadScene(levelSceneNames[0]);
            }
            else
            {
                // Fallback: reload current scene
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
    
    /// <summary>
    /// Set the current level number manually
    /// </summary>
    public void SetCurrentLevel(int levelNumber)
    {
        currentLevelNumber = Mathf.Clamp(levelNumber, 1, 5);
        UpdateNextLevelButtonText();
    }
}
