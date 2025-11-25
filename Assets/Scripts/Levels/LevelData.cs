using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Defines a level layout - like a blueprint for level generation.
/// This is a ScriptableObject so you can create multiple level designs.
/// </summary>
[CreateAssetMenu(fileName = "NewLevel", menuName = "LumenShift/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public string levelName = "Level 1";
    public int levelWidth = 50; // Width in blocks
    public int levelHeight = 20; // Height in blocks
    public float blockSize = 1f; // Size of each block
    
    [Header("Level Layout")]
    [TextArea(10, 30)]
    public string levelLayout = "";
    
    [Header("Block Prefabs")]
    public GameObject floorBlockPrefab;
    public GameObject boxBlockPrefab;
    public GameObject lightningBoltPrefab;
    
    [Header("Player Start")]
    public Vector2 playerStartPosition = new Vector2(1f, 2f);
    
    [Header("Level Goals")]
    public int lightningBoltsRequired = 3;
    
    /// <summary>
    /// Parse the level layout string into a 2D grid.
    /// Characters:
    /// 'F' = Floor block
    /// 'B' = Box block
    /// 'L' = Lightning bolt
    /// 'P' = Player start (optional, can use playerStartPosition instead)
    /// ' ' = Empty space
    /// </summary>
    public char[,] ParseLayout()
    {
        char[,] grid = new char[levelHeight, levelWidth];
        
        // Initialize with empty spaces
        for (int y = 0; y < levelHeight; y++)
        {
            for (int x = 0; x < levelWidth; x++)
            {
                grid[y, x] = ' ';
            }
        }
        
        // Parse the layout string
        string[] lines = levelLayout.Split('\n');
        int lineIndex = 0;
        
        // Start from top (levelHeight - 1) and work down
        for (int y = levelHeight - 1; y >= 0 && lineIndex < lines.Length; y--)
        {
            string line = lines[lineIndex].TrimEnd();
            for (int x = 0; x < levelWidth && x < line.Length; x++)
            {
                if (x < line.Length)
                {
                    grid[y, x] = line[x];
                }
            }
            lineIndex++;
        }
        
        return grid;
    }
}



