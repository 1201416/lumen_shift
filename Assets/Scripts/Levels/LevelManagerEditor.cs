#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor script to ensure LevelManager has all generators assigned
/// This runs in edit mode to ensure generators persist after Unity crashes
/// </summary>
[CustomEditor(typeof(LevelManager))]
public class LevelManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        LevelManager levelManager = (LevelManager)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "If generators are missing after Unity crashes, click 'Auto-Create Missing Generators' below.",
            MessageType.Info
        );
        
        if (GUILayout.Button("Auto-Create Missing Generators"))
        {
            CreateMissingGenerators(levelManager);
        }
    }
    
    void CreateMissingGenerators(LevelManager levelManager)
    {
        bool sceneDirty = false;
        
        // Create Level 1 generator if missing
        if (levelManager.level1Generator == null)
        {
            GameObject go = GameObject.Find("FirstLevelGenerator");
            if (go == null)
            {
                go = new GameObject("FirstLevelGenerator");
                levelManager.level1Generator = go.AddComponent<FirstLevelGenerator>();
                sceneDirty = true;
            }
            else
            {
                levelManager.level1Generator = go.GetComponent<FirstLevelGenerator>();
                if (levelManager.level1Generator == null)
                {
                    levelManager.level1Generator = go.AddComponent<FirstLevelGenerator>();
                    sceneDirty = true;
                }
            }
        }
        
        // Create Level 2-15 generators if missing
        if (levelManager.level2Generator == null) { levelManager.level2Generator = CreateGenerator<Level2Generator>("Level2Generator"); sceneDirty = true; }
        if (levelManager.level3Generator == null) { levelManager.level3Generator = CreateGenerator<Level3Generator>("Level3Generator"); sceneDirty = true; }
        if (levelManager.level4Generator == null) { levelManager.level4Generator = CreateGenerator<Level4Generator>("Level4Generator"); sceneDirty = true; }
        if (levelManager.level5Generator == null) { levelManager.level5Generator = CreateGenerator<Level5Generator>("Level5Generator"); sceneDirty = true; }
        if (levelManager.level6Generator == null) { levelManager.level6Generator = CreateGenerator<Level6Generator>("Level6Generator"); sceneDirty = true; }
        if (levelManager.level7Generator == null) { levelManager.level7Generator = CreateGenerator<Level7Generator>("Level7Generator"); sceneDirty = true; }
        if (levelManager.level8Generator == null) { levelManager.level8Generator = CreateGenerator<Level8Generator>("Level8Generator"); sceneDirty = true; }
        if (levelManager.level9Generator == null) { levelManager.level9Generator = CreateGenerator<Level9Generator>("Level9Generator"); sceneDirty = true; }
        if (levelManager.level10Generator == null) { levelManager.level10Generator = CreateGenerator<Level10Generator>("Level10Generator"); sceneDirty = true; }
        if (levelManager.level11Generator == null) { levelManager.level11Generator = CreateGenerator<Level11Generator>("Level11Generator"); sceneDirty = true; }
        if (levelManager.level12Generator == null) { levelManager.level12Generator = CreateGenerator<Level12Generator>("Level12Generator"); sceneDirty = true; }
        if (levelManager.level13Generator == null) { levelManager.level13Generator = CreateGenerator<Level13Generator>("Level13Generator"); sceneDirty = true; }
        if (levelManager.level14Generator == null) { levelManager.level14Generator = CreateGenerator<Level14Generator>("Level14Generator"); sceneDirty = true; }
        if (levelManager.level15Generator == null) { levelManager.level15Generator = CreateGenerator<Level15Generator>("Level15Generator"); sceneDirty = true; }
        
        if (sceneDirty)
        {
            EditorUtility.SetDirty(levelManager);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("Created missing level generators. Please save the scene to persist them.");
        }
        else
        {
            Debug.Log("All level generators are already assigned.");
        }
    }
    
    T CreateGenerator<T>(string gameObjectName) where T : MonoBehaviour
    {
        GameObject go = GameObject.Find(gameObjectName);
        if (go == null)
        {
            go = new GameObject(gameObjectName);
            return go.AddComponent<T>();
        }
        else
        {
            T component = go.GetComponent<T>();
            if (component == null)
            {
                component = go.AddComponent<T>();
            }
            return component;
        }
    }
}
#endif
