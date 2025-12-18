using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// BootScreen - shows a single splash image before the first level loads.
/// Place a sprite or texture at Assets/Resources/Logo (png/jpg).
/// </summary>
public static class BootScreen
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ShowBootScreen()
    {
        // Create root object so FirstLevelGenerator can wait for it by name
        GameObject go = new GameObject("BootScreen");
        Object.DontDestroyOnLoad(go);
        BootScreenController controller = go.AddComponent<BootScreenController>();
        controller.displaySeconds = 5f;
        controller.fadeSeconds = 0.5f;
        controller.Show();
    }
}

public class BootScreenController : MonoBehaviour
{
    public float displaySeconds = 2.5f;
    public float fadeSeconds = 0.5f;
    // Seconds to show credits.png (after logo)
    public float creditsDisplaySeconds = 3f;

    Canvas canvas;
    CanvasGroup canvasGroup;
    bool hiding = false;
    bool active = false;
    float elapsed = 0f;
    float fadeElapsed = 0f;
    float totalElapsed = 0f;
    const float safetyBuffer = 5f;
    // UI elements and state for second splash
    Image logoImage;
    Text logoTextFallback;
    Sprite creditsSprite;
    bool showingCredits = false;

    public void Show()
    {
        CreateUI();
        active = true;
        hiding = false;
        elapsed = 0f;
        fadeElapsed = 0f;
        totalElapsed = 0f;
        Debug.Log("BootScreen: shown");
    }

    void CreateUI()
    {
        GameObject canvasGO = new GameObject("BootCanvas");
        canvasGO.transform.SetParent(this.transform, false);
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(canvasGO.transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = Color.black;

        // logo
        GameObject logoGO = new GameObject("Logo");
        logoGO.transform.SetParent(canvasGO.transform, false);
        logoImage = logoGO.AddComponent<Image>();
        RectTransform logoRect = logoGO.GetComponent<RectTransform>();
        logoRect.anchorMin = new Vector2(0.5f, 0.5f);
        logoRect.anchorMax = new Vector2(0.5f, 0.5f);
        logoRect.sizeDelta = new Vector2(600f, 300f);
        logoRect.anchoredPosition = Vector2.zero;

        // Try load Sprite first, then Texture2D
        Sprite logo = Resources.Load<Sprite>("Logo");
        if (logo == null)
        {
            Texture2D tex = Resources.Load<Texture2D>("Logo");
            if (tex != null)
            {
                logo = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            }
        }

        if (logo != null)
        {
            logoImage.sprite = logo;
            logoImage.preserveAspect = true;
        }
        else
        {
            // fallback text
            GameObject textGO = new GameObject("LogoText");
            textGO.transform.SetParent(canvasGO.transform, false);
            logoTextFallback = textGO.AddComponent<Text>();
            logoTextFallback.text = "Lumen-Shift";
            logoTextFallback.alignment = TextAnchor.MiddleCenter;
            logoTextFallback.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            logoTextFallback.fontSize = 48;
            RectTransform tr = textGO.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0.5f, 0.5f);
            tr.anchorMax = new Vector2(0.5f, 0.5f);
            tr.anchoredPosition = Vector2.zero;
        }

        // Try to load credits sprite (credits.png) - try common name variants
        creditsSprite = Resources.Load<Sprite>("Credits");
        if (creditsSprite == null) creditsSprite = Resources.Load<Sprite>("credits");
        if (creditsSprite == null)
        {
            Texture2D ctex = Resources.Load<Texture2D>("Credits");
            if (ctex != null) creditsSprite = Sprite.Create(ctex, new Rect(0, 0, ctex.width, ctex.height), new Vector2(0.5f, 0.5f), 100f);
            else
            {
                ctex = Resources.Load<Texture2D>("credits");
                if (ctex != null) creditsSprite = Sprite.Create(ctex, new Rect(0, 0, ctex.width, ctex.height), new Vector2(0.5f, 0.5f), 100f);
            }
        }
        if (creditsSprite != null)
        {
            Debug.Log("BootScreen: credits sprite loaded from Resources");
        }

        canvasGroup = canvasGO.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
    }

    void Update()
    {
        if (!active) return;


        if (!hiding)
        {
            elapsed += Time.unscaledDeltaTime;
            totalElapsed += Time.unscaledDeltaTime;
            // If we haven't switched to credits yet, do so after displaySeconds
            if (!showingCredits)
            {
                if (elapsed >= displaySeconds)
                {
                    // If we have a credits sprite or fallback text, switch to credits
                    if (creditsSprite != null || logoTextFallback != null)
                    {
                        showingCredits = true;
                        elapsed = 0f;
                        Debug.Log("BootScreen: switching to credits");
                        if (logoImage != null && creditsSprite != null)
                        {
                            logoImage.sprite = creditsSprite;
                            logoImage.preserveAspect = true;
                        }
                        else if (logoTextFallback != null)
                        {
                            logoTextFallback.text = "Credits";
                        }
                    }
                    else
                    {
                        // No credits available, start hiding
                        hiding = true;
                        Debug.Log("BootScreen: time elapsed, start hiding");
                    }
                }
            }
            else
            {
                // Already showing credits, wait creditsDisplaySeconds then hide
                if (elapsed >= creditsDisplaySeconds)
                {
                    hiding = true;
                    Debug.Log("BootScreen: credits elapsed, start hiding");
                }
            }
        }

        if (hiding)
        {
            fadeElapsed += Time.unscaledDeltaTime;
            totalElapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1f - (fadeElapsed / Mathf.Max(0.0001f, fadeSeconds)));
            if (fadeElapsed >= fadeSeconds)
            {
                // finalize
                canvasGroup.alpha = 0f;
                if (canvas != null) canvas.enabled = false;
                active = false;
                Debug.Log("BootScreen: hidden, destroying");
                Destroy(this.gameObject);
            }
        }

        // safety (include credits display time)
        float maxLifetime = displaySeconds + creditsDisplaySeconds + fadeSeconds + safetyBuffer;
        if (totalElapsed >= maxLifetime && active)
        {
            Debug.LogWarning($"BootScreen: safety timeout reached ({totalElapsed:F1}s), forcing removal");
            if (canvas != null) canvas.enabled = false;
            active = false;
            try { DestroyImmediate(this.gameObject); } catch { Destroy(this.gameObject); }
        }
    }
}
