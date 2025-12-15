using UnityEngine;

/// <summary>
/// Helper component to handle player collection of lightning bolts
/// Attached to the trigger collider child object
/// </summary>
public class LightningBoltCollector : MonoBehaviour
{
    public LightningBolt bolt;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (bolt == null) return;
        
        // Check if player collected it
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            bolt.CollectBolt();
        }
    }
}



