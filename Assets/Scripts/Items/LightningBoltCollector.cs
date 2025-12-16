using UnityEngine;

/// <summary>
/// Helper component to handle player collection of lightning bolts
/// Attached to the trigger collider child object
/// </summary>
public class LightningBoltCollector : MonoBehaviour
{
    public LightningBolt bolt;
    private bool hasCollected = false;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (bolt == null) return;
        if (hasCollected) return; // Prevent double collection
        if (bolt.isCollected) return; // Already collected
        
        // Check if player collected it
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            hasCollected = true;
            bolt.CollectBolt();
        }
    }
    
    /// <summary>
    /// Reset collector state (called when bolt is reset)
    /// </summary>
    public void ResetCollector()
    {
        hasCollected = false;
    }
}



