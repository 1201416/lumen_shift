using UnityEngine;

[CreateAssetMenu(fileName = "NewScriptableObjectScript", menuName = "Scriptable Objects/NewScriptableObjectScript")]
public class BlockType : ScriptableObject
{
    public string displayName;
    public Sprite sprite;
    public bool hasCollider;
    public bool reactsToTimeOfDay;
    public Color dayTint = Color.white;
    public Color nightTint = Color.white;
}
