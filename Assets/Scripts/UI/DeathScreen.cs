using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// Death screen UI - shows when player dies with respawn button
/// </summary>
public class DeathScreen : MonoBehaviour
{
    [Header("UI References")]
    public Canvas deathCanvas;
    public Text deathMessageText;
    public Button respawnButton;
    public Text respawnButtonText;
    
    [Header("Settings")]
    public string deathMessage = "You died!";
    public string respawnButtonTextStr = "Respawn (R)";
    [Header("Audio")]
    public AudioClip deathSound;
    
    private GameManager gameManager;
    private Vector3 playerSpawnPosition;
    public bool isShowing { get; private set; } = false;
    
    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        
        // Find player spawn position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerSpawnPosition = player.transform.position;
        }
        else
        {
            // Default spawn position
            playerSpawnPosition = new Vector3(1f, 1.5f, 0f);
        }
        
        // Create UI if it doesn't exist
        if (deathCanvas == null)
        {
            CreateDeathUI();
        }
        
        // Hide by default
        HideDeathScreen();

        // Try load default death sound from Resources if none assigned
        if (deathSound == null)
        {
            deathSound = Resources.Load<AudioClip>("Audio/death");
            if (deathSound == null) deathSound = Resources.Load<AudioClip>("death");
            if (deathSound != null)
            {
                Debug.Log("DeathScreen: loaded deathSound from Resources");
            }
        }
    }
    
    void Update()
    {
        // Check for R key to respawn (only when death screen is showing)
        if (isShowing && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RespawnPlayer();
        }
    }
    
    /// <summary>
    /// Create death screen UI
    /// </summary>
    void CreateDeathUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("DeathCanvas");
        deathCanvas = canvasObj.AddComponent<Canvas>();
        deathCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        deathCanvas.sortingOrder = 100; // Above everything
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // No background overlay - keep normal background
        
        // Create death message
        GameObject messageObj = new GameObject("DeathMessage");
        messageObj.transform.SetParent(canvasObj.transform, false);
        deathMessageText = messageObj.AddComponent<Text>();
        deathMessageText.text = deathMessage;
        deathMessageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        deathMessageText.fontSize = 48;
        deathMessageText.color = Color.white;
        deathMessageText.alignment = TextAnchor.MiddleCenter;
        deathMessageText.fontStyle = FontStyle.Bold;
        
        RectTransform msgRect = messageObj.GetComponent<RectTransform>();
        msgRect.anchorMin = new Vector2(0.5f, 0.6f);
        msgRect.anchorMax = new Vector2(0.5f, 0.6f);
        msgRect.pivot = new Vector2(0.5f, 0.5f);
        msgRect.sizeDelta = new Vector2(400f, 80f);
        msgRect.anchoredPosition = Vector2.zero;
        
        // Create respawn button
        GameObject buttonObj = new GameObject("RespawnButton");
        buttonObj.transform.SetParent(canvasObj.transform, false);
        
        RectTransform btnRect = buttonObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.4f);
        btnRect.anchorMax = new Vector2(0.5f, 0.4f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.sizeDelta = new Vector2(200f, 60f);
        btnRect.anchoredPosition = Vector2.zero;
        
        Image btnImage = buttonObj.AddComponent<Image>();
        btnImage.color = new Color(0.3f, 0.3f, 0.3f, 0.9f);
        
        respawnButton = buttonObj.AddComponent<Button>();
        respawnButton.targetGraphic = btnImage;
        
        ColorBlock colors = respawnButton.colors;
        colors.normalColor = new Color(0.3f, 0.3f, 0.3f);
        colors.highlightedColor = new Color(0.5f, 0.5f, 0.5f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.2f);
        respawnButton.colors = colors;
        
        respawnButton.onClick.AddListener(RespawnPlayer);
        
        // Create button text
        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.sizeDelta = Vector2.zero;
        
        respawnButtonText = btnTextObj.AddComponent<Text>();
        respawnButtonText.text = respawnButtonTextStr;
        respawnButtonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        respawnButtonText.fontSize = 32;
        respawnButtonText.color = Color.white;
        respawnButtonText.alignment = TextAnchor.MiddleCenter;
        respawnButtonText.fontStyle = FontStyle.Bold;
        
        btnTextObj.AddComponent<CanvasGroup>().interactable = false;
    }
    
    /// <summary>
    /// Show death screen
    /// </summary>
    public void ShowDeathScreen()
    {
        if (isShowing) return;
        isShowing = true;
        
        if (deathCanvas != null)
        {
            deathCanvas.gameObject.SetActive(true);
        }
        
        // Background remains unchanged - keep normal day/night background
        
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

        // Play death sound if available
        if (deathSound != null)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(deathSound, 1f);
            }
            else
            {
                AudioSource.PlayClipAtPoint(deathSound, Camera.main != null ? Camera.main.transform.position : transform.position);
            }
        }
    }
    
    /// <summary>
    /// Hide death screen
    /// </summary>
    public void HideDeathScreen()
    {
        isShowing = false;
        
        if (deathCanvas != null)
        {
            deathCanvas.gameObject.SetActive(false);
        }
        
        // Background remains unchanged - no need to restore anything
    }
    
    /// <summary>
    /// Respawn the player at spawn position
    /// </summary>
    public void RespawnPlayer()
    {
        HideDeathScreen();
        
        // Reset all monsters' death state
        Monster[] allMonsters = FindObjectsByType<Monster>(FindObjectsSortMode.None);
        foreach (var monster in allMonsters)
        {
            if (monster != null)
            {
                monster.ResetDeathState();
            }
        }
        
        // Reset lightning bolts and counter
        ResetLightningBolts();
        
        // Find or get player spawn position
        FirstLevelGenerator levelGen = FindFirstObjectByType<FirstLevelGenerator>();
        if (levelGen != null)
        {
            // Get spawn position from level generator
            playerSpawnPosition = new Vector3(1f * levelGen.blockSize, 1.5f, 0f);
        }
        
        // Reset player position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = playerSpawnPosition;
            
            // Re-enable player movement
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
        
        Debug.Log("Player respawned! Lightning bolts reset.");
    }
    
    /// <summary>
    /// Reset all lightning bolts to uncollected state and reset counter
    /// </summary>
    void ResetLightningBolts()
    {
        // Reset GameManager bolt count
        if (gameManager != null)
        {
            gameManager.totalBoltsCollected = 0;
            gameManager.RefreshAllObjects(); // Refresh bolt list
        }
        
        // Reset all lightning bolts in the scene (including inactive ones)
        LightningBolt[] allBolts = FindObjectsByType<LightningBolt>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var bolt in allBolts)
        {
            if (bolt != null)
            {
                bolt.ResetBolt();
            }
        }
        
        // Update counter display and reset to original total
        LightningBoltCounter counter = FindFirstObjectByType<LightningBoltCounter>();
        if (counter != null)
        {
            counter.ResetToOriginalTotal();
        }
        
        Debug.Log($"Reset {allBolts.Length} lightning bolts");
    }
}

