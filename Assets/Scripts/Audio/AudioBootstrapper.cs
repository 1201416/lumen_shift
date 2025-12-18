using UnityEngine;

public static class AudioBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureAudioManagerExists()
    {
        // If an AudioManager already exists, do nothing
        var existing = Object.FindObjectOfType<AudioManager>();
        if (existing != null)
        {
            Debug.Log("AudioBootstrapper: AudioManager already present.");
            return;
        }

        // Create a new GameObject with AudioManager
        GameObject go = new GameObject("AudioManager");
        Object.DontDestroyOnLoad(go);
        var am = go.AddComponent<AudioManager>();
        Debug.Log("AudioBootstrapper: Created AudioManager at BeforeSceneLoad.");
    }
}
