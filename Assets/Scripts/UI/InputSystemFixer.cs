using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Ensures all EventSystems use InputSystemUIInputModule instead of StandaloneInputModule
/// This fixes the Input System compatibility issue
/// </summary>
public class InputSystemFixer : MonoBehaviour
{
    void Start()
    {
        FixAllEventSystems();
    }
    
    void FixAllEventSystems()
    {
        // Find all EventSystems in the scene
        EventSystem[] allEventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        
        foreach (EventSystem eventSystem in allEventSystems)
        {
            // Check if it has the old StandaloneInputModule
            var oldModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (oldModule != null)
            {
                // Remove the old module
                Destroy(oldModule);
                Debug.Log($"Removed StandaloneInputModule from {eventSystem.name}");
            }
            
            // Check if it has InputSystemUIInputModule
            var newModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (newModule == null)
            {
                // Add the new module
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                Debug.Log($"Added InputSystemUIInputModule to {eventSystem.name}");
            }
        }
    }
}

