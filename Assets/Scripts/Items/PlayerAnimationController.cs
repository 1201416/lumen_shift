using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

/// <summary>
/// Sets up player animations using all Virtual Guy sprites
/// Creates Animator Controller and animation clips automatically
/// </summary>
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Animation Settings")]
    public float idleFrameRate = 8f;
    public float runFrameRate = 12f;
    public float jumpFrameRate = 10f;
    public float fallFrameRate = 10f;
    public float hitFrameRate = 10f;
    
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerController playerController;
    
    // Sprite collections for each animation
    private Sprite[] idleSprites;
    private Sprite[] runSprites;
    private Sprite[] jumpSprites;
    private Sprite[] fallSprites;
    private Sprite[] hitSprites;
    
    // Animation timing
    private float animationTimer = 0f;
    private int currentFrame = 0;
    private string currentAnimation = "Idle";
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerController = GetComponent<PlayerController>();
        
        // CRITICAL: Ensure rotation is correct (facing right, upright, NOT sideways)
        transform.rotation = Quaternion.identity;
        transform.localScale = new Vector3(1f, 1f, 1f); // Ensure no weird scaling
        
        // Load all sprites
        LoadAllSprites();
        
        // Setup animator (try to use Unity Animator, fallback to manual sprite switching)
        SetupAnimator();
    }
    
    void Update()
    {
        if (playerController == null) return;
        
        // Always ensure rotation is correct
        if (transform.rotation != Quaternion.identity)
        {
            transform.rotation = Quaternion.identity;
        }
        
        // Update animation based on player state
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            UpdateAnimatorParameters();
        }
        else
        {
            // Fallback: Manual sprite animation
            UpdateManualAnimation();
        }
    }
    
    void LoadAllSprites()
    {
        // Load sprites by finding them in the project
        idleSprites = LoadSpritesByName("Idle");
        runSprites = LoadSpritesByName("Run");
        jumpSprites = LoadSpritesByName("Jump");
        fallSprites = LoadSpritesByName("Fall");
        hitSprites = LoadSpritesByName("Hit");
        
        Debug.Log($"Loaded sprites - Idle: {idleSprites?.Length ?? 0}, Run: {runSprites?.Length ?? 0}, Jump: {jumpSprites?.Length ?? 0}, Fall: {fallSprites?.Length ?? 0}, Hit: {hitSprites?.Length ?? 0}");
    }
    
    Sprite[] LoadSpritesByName(string spriteName)
    {
        List<Sprite> sprites = new List<Sprite>();
        
        #if UNITY_EDITOR
        // Use AssetDatabase in editor
        string[] guids = AssetDatabase.FindAssets(spriteName + " t:Sprite", new[] { "Assets/PixelAdventure/Main Characters/Virtual Guy" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("Virtual Guy") && path.Contains(spriteName))
            {
                // Try loading as sprite
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null)
                {
                    sprites.Add(sprite);
                }
                else
                {
                    // Try loading all sprites from texture (sprite sheet)
                    Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
                    foreach (Object asset in assets)
                    {
                        if (asset is Sprite s && s.name.Contains(spriteName))
                        {
                            sprites.Add(s);
                        }
                    }
                }
            }
        }
        #else
        // At runtime, find by name from all loaded sprites
        Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
        foreach (Sprite s in allSprites)
        {
            if (s.name.Contains(spriteName) && s.name.Contains("Virtual Guy"))
            {
                sprites.Add(s);
            }
        }
        #endif
        
        // Sort sprites by name to ensure correct order
        sprites = sprites.OrderBy(s => s.name).ToList();
        
        return sprites.Count > 0 ? sprites.ToArray() : null;
    }
    
    void SetupAnimator()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = gameObject.AddComponent<Animator>();
        }
        
        // Try to load or create animator controller
        RuntimeAnimatorController controller = LoadOrCreateAnimatorController();
        if (controller != null)
        {
            animator.runtimeAnimatorController = controller;
        }
    }
    
    RuntimeAnimatorController LoadOrCreateAnimatorController()
    {
        #if UNITY_EDITOR
        // Try to find existing controller
        string controllerPath = "Assets/Animations/PlayerAnimator.controller";
        RuntimeAnimatorController existingController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
        if (existingController != null)
        {
            return existingController;
        }
        
        // Create new animator controller
        if (!AssetDatabase.IsValidFolder("Assets/Animations"))
        {
            AssetDatabase.CreateFolder("Assets", "Animations");
        }
        
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        
        // Create animation clips from sprites
        if (idleSprites != null && idleSprites.Length > 0)
        {
            AnimationClip idleClip = CreateAnimationClip("Idle", idleSprites, idleFrameRate);
            AnimatorState idleState = controller.AddMotion(idleClip);
            controller.layers[0].stateMachine.defaultState = idleState;
        }
        
        if (runSprites != null && runSprites.Length > 0)
        {
            AnimationClip runClip = CreateAnimationClip("Run", runSprites, runFrameRate);
            controller.AddMotion(runClip);
        }
        
        if (jumpSprites != null && jumpSprites.Length > 0)
        {
            AnimationClip jumpClip = CreateAnimationClip("Jump", jumpSprites, jumpFrameRate);
            controller.AddMotion(jumpClip);
        }
        
        if (fallSprites != null && fallSprites.Length > 0)
        {
            AnimationClip fallClip = CreateAnimationClip("Fall", fallSprites, fallFrameRate);
            controller.AddMotion(fallClip);
        }
        
        if (hitSprites != null && hitSprites.Length > 0)
        {
            AnimationClip hitClip = CreateAnimationClip("Hit", hitSprites, hitFrameRate);
            controller.AddMotion(hitClip);
        }
        
        // Create transitions between states
        CreateTransitions(controller);
        
        AssetDatabase.SaveAssets();
        return controller;
        #else
        // At runtime, try to load from Resources
        return Resources.Load<RuntimeAnimatorController>("PlayerAnimator");
        #endif
    }
    
    #if UNITY_EDITOR
    AnimationClip CreateAnimationClip(string clipName, Sprite[] sprites, float frameRate)
    {
        AnimationClip clip = new AnimationClip();
        clip.name = clipName;
        clip.frameRate = frameRate;
        
        EditorCurveBinding spriteBinding = new EditorCurveBinding();
        spriteBinding.type = typeof(SpriteRenderer);
        spriteBinding.path = "";
        spriteBinding.propertyName = "m_Sprite";
        
        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe();
            keyframes[i].time = i / frameRate;
            keyframes[i].value = sprites[i];
        }
        
        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);
        
        // Save clip
        string clipPath = $"Assets/Animations/{clipName}.anim";
        AssetDatabase.CreateAsset(clip, clipPath);
        
        return clip;
    }
    
    void CreateTransitions(AnimatorController controller)
    {
        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        
        // Create parameters
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
        controller.AddParameter("VelocityY", AnimatorControllerParameterType.Float);
        
        // Get states
        AnimatorState idleState = null;
        AnimatorState runState = null;
        AnimatorState jumpState = null;
        AnimatorState fallState = null;
        
        foreach (ChildAnimatorState state in stateMachine.states)
        {
            if (state.state.name == "Idle") idleState = state.state;
            else if (state.state.name == "Run") runState = state.state;
            else if (state.state.name == "Jump") jumpState = state.state;
            else if (state.state.name == "Fall") fallState = state.state;
        }
        
        // Create transitions
        if (idleState != null && runState != null)
        {
            AnimatorStateTransition transition = idleState.AddTransition(runState);
            transition.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            transition.duration = 0.1f;
        }
        
        if (runState != null && idleState != null)
        {
            AnimatorStateTransition transition = runState.AddTransition(idleState);
            transition.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            transition.duration = 0.1f;
        }
        
        if (idleState != null && jumpState != null)
        {
            AnimatorStateTransition transition = idleState.AddTransition(jumpState);
            transition.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");
            transition.duration = 0f;
        }
        
        if (runState != null && jumpState != null)
        {
            AnimatorStateTransition transition = runState.AddTransition(jumpState);
            transition.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");
            transition.duration = 0f;
        }
        
        if (jumpState != null && fallState != null)
        {
            AnimatorStateTransition transition = jumpState.AddTransition(fallState);
            transition.AddCondition(AnimatorConditionMode.Less, -0.1f, "VelocityY");
            transition.duration = 0f;
        }
        
        if (fallState != null && idleState != null)
        {
            AnimatorStateTransition transition = fallState.AddTransition(idleState);
            transition.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");
            transition.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            transition.duration = 0.1f;
        }
        
        if (fallState != null && runState != null)
        {
            AnimatorStateTransition transition = fallState.AddTransition(runState);
            transition.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");
            transition.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            transition.duration = 0.1f;
        }
    }
    #endif
    
    void UpdateAnimatorParameters()
    {
        if (animator == null || playerController == null) return;
        
        float speed = Mathf.Abs(playerController.GetHorizontalVelocity());
        bool grounded = playerController.IsGrounded();
        float velocityY = playerController.GetVerticalVelocity();
        
        animator.SetFloat("Speed", speed);
        animator.SetBool("IsGrounded", grounded);
        animator.SetFloat("VelocityY", velocityY);
    }
    
    void UpdateManualAnimation()
    {
        if (spriteRenderer == null || playerController == null) return;
        
        // Determine current animation state
        string newAnimation = DetermineAnimationState();
        
        // Switch animation if changed
        if (newAnimation != currentAnimation)
        {
            currentAnimation = newAnimation;
            currentFrame = 0;
            animationTimer = 0f;
        }
        
        // Get sprites for current animation
        Sprite[] currentSprites = GetSpritesForAnimation(currentAnimation);
        if (currentSprites == null || currentSprites.Length == 0) return;
        
        // Animate
        float frameRate = GetFrameRateForAnimation(currentAnimation);
        animationTimer += Time.deltaTime;
        
        if (animationTimer >= 1f / frameRate)
        {
            animationTimer = 0f;
            currentFrame = (currentFrame + 1) % currentSprites.Length;
            spriteRenderer.sprite = currentSprites[currentFrame];
        }
    }
    
    string DetermineAnimationState()
    {
        if (playerController == null) return "Idle";
        
        bool grounded = playerController.IsGrounded();
        float velocityY = playerController.GetVerticalVelocity();
        float speed = Mathf.Abs(playerController.GetHorizontalVelocity());
        
        if (!grounded)
        {
            if (velocityY > 0.1f) return "Jump";
            else return "Fall";
        }
        else if (speed > 0.1f)
        {
            return "Run";
        }
        else
        {
            return "Idle";
        }
    }
    
    Sprite[] GetSpritesForAnimation(string animationName)
    {
        switch (animationName)
        {
            case "Idle": return idleSprites;
            case "Run": return runSprites;
            case "Jump": return jumpSprites;
            case "Fall": return fallSprites;
            case "Hit": return hitSprites;
            default: return idleSprites;
        }
    }
    
    float GetFrameRateForAnimation(string animationName)
    {
        switch (animationName)
        {
            case "Idle": return idleFrameRate;
            case "Run": return runFrameRate;
            case "Jump": return jumpFrameRate;
            case "Fall": return fallFrameRate;
            case "Hit": return hitFrameRate;
            default: return idleFrameRate;
        }
    }
}

