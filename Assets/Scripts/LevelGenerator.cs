using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Prefab References")]
    [SerializeField] private GameObject outsideCornerPrefab;
    [SerializeField] private GameObject outsideWallPrefab;
    [SerializeField] private GameObject insideCornerPrefab;
    [SerializeField] private GameObject insideWallPrefab;
    [SerializeField] private GameObject pelletPrefab;
    [SerializeField] private GameObject powerPelletPrefab;
    [SerializeField] private GameObject tJunctionPrefab;
    [SerializeField] private GameObject ghostExitWallPrefab;
    
    [Header("Camera Reference")]
    [SerializeField] private Camera gameCamera;
    
    // Original level map (top-left quadrant only)
    private int[,] levelMap = new int[,]
    {
        {1,2,2,2,2,2,2,7,2,2,2,2,2,1},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,2},
        {2,6,3,4,4,3,5,3,4,4,4,3,6,2},
        {2,5,4,0,0,4,5,4,0,0,0,4,5,2},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,2},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,2},
        {2,5,3,4,4,3,5,3,3,5,3,4,4,2},
        {2,5,4,0,0,4,5,4,4,5,4,0,0,2},
        {2,5,5,5,5,5,5,4,4,5,5,5,5,2},
        {2,5,3,4,4,3,5,4,3,4,4,3,5,2},
        {2,5,4,0,0,4,5,4,4,0,0,4,5,2},
        {2,5,3,4,4,3,5,4,4,0,0,0,5,2},
        {2,5,5,5,5,5,5,3,3,0,3,4,5,8},
        {2,5,5,5,5,5,5,0,0,0,4,0,5,2},
        {1,2,2,2,2,2,2,2,2,2,2,2,2,1}
    };
    
    private int mapWidth;
    private int mapHeight;
    private GameObject levelParent;
    
    void Start()
    {
        // Delete existing manual level
        DeleteExistingLevel();
        
        // Generate the procedural level
        GenerateLevel();
        
        // Adjust camera to show full level
        AdjustCamera();
    }
    
    void DeleteExistingLevel()
    {
        // Find and delete the manually created level
        GameObject existingLevel = GameObject.Find("Level Layout");
        if (existingLevel != null)
        {
            DestroyImmediate(existingLevel);
        }
        
        // Also clean up any loose sprites
        GameObject[] allSprites = GameObject.FindGameObjectsWithTag("Untagged");
        foreach (GameObject sprite in allSprites)
        {
            if (sprite.GetComponent<SpriteRenderer>() != null && sprite != gameObject)
            {
                DestroyImmediate(sprite);
            }
        }
    }
    
    void GenerateLevel()
    {
        // Create parent object for organization
        levelParent = new GameObject("Generated Level");
        
        mapHeight = levelMap.GetLength(0);
        mapWidth = levelMap.GetLength(1);
        
        // Generate full level with mirroring
        int fullWidth = mapWidth * 2 - 1;  // Remove duplicate center column
        int fullHeight = mapHeight * 2 - 1; // Remove duplicate center row
        
        for (int row = 0; row < fullHeight; row++)
        {
            for (int col = 0; col < fullWidth; col++)
            {
                // Determine which quadrant we're in and map coordinates
                int sourceRow, sourceCol;
                bool flipH = false, flipV = false;
                
                if (row < mapHeight && col < mapWidth)
                {
                    // Top-left quadrant (original)
                    sourceRow = row;
                    sourceCol = col;
                }
                else if (row < mapHeight && col >= mapWidth)
                {
                    // Top-right quadrant (horizontal flip)
                    sourceRow = row;
                    sourceCol = mapWidth - 1 - (col - mapWidth + 1);
                    flipH = true;
                }
                else if (row >= mapHeight && col < mapWidth)
                {
                    // Bottom-left quadrant (vertical flip)
                    sourceRow = mapHeight - 1 - (row - mapHeight + 1);
                    sourceCol = col;
                    flipV = true;
                }
                else
                {
                    // Bottom-right quadrant (both flips)
                    sourceRow = mapHeight - 1 - (row - mapHeight + 1);
                    sourceCol = mapWidth - 1 - (col - mapWidth + 1);
                    flipH = true;
                    flipV = true;
                }
                
                // Skip if we're on the duplicate center lines
                if ((row == mapHeight - 1 && row >= mapHeight) || 
                    (col == mapWidth - 1 && col >= mapWidth))
                    continue;
                
                int tileType = levelMap[sourceRow, sourceCol];
                if (tileType != 0) // Skip empty spaces
                {
                    Vector3 position = new Vector3(col, -row, 0);
                    float rotation = CalculateRotation(tileType, sourceRow, sourceCol, flipH, flipV);
                    
                    InstantiateTile(tileType, position, rotation);
                }
            }
        }
    }
    
    float CalculateRotation(int tileType, int row, int col, bool flipH, bool flipV)
    {
        float baseRotation = 0f;
        
        switch (tileType)
        {
            case 1: // Outside corner
                baseRotation = CalculateCornerRotation(row, col, true);
                break;
                
            case 2: // Outside wall
                baseRotation = CalculateWallRotation(row, col, true);
                break;
                
            case 3: // Inside corner
                baseRotation = CalculateCornerRotation(row, col, false);
                break;
                
            case 4: // Inside wall
                baseRotation = CalculateWallRotation(row, col, false);
                break;
                
            case 7: // T-junction
                baseRotation = CalculateTJunctionRotation(row, col);
                break;
        }
        
        // Apply mirroring adjustments
        if (flipH)
        {
            if (tileType == 1 || tileType == 3) // Corners
            {
                baseRotation = GetHorizontalFlippedCornerRotation(baseRotation);
            }
            else if (tileType == 7) // T-junction
            {
                baseRotation = GetHorizontalFlippedTRotation(baseRotation);
            }
        }
        
        if (flipV)
        {
            if (tileType == 1 || tileType == 3) // Corners
            {
                baseRotation = GetVerticalFlippedCornerRotation(baseRotation);
            }
            else if (tileType == 7) // T-junction
            {
                baseRotation = GetVerticalFlippedTRotation(baseRotation);
            }
        }
        
        return baseRotation;
    }
    
    float CalculateCornerRotation(int row, int col, bool isOutside)
    {
        // Check adjacent tiles to determine corner orientation
        bool hasUp = (row > 0 && levelMap[row - 1, col] != 0 && levelMap[row - 1, col] != 5 && levelMap[row - 1, col] != 6);
        bool hasDown = (row < mapHeight - 1 && levelMap[row + 1, col] != 0 && levelMap[row + 1, col] != 5 && levelMap[row + 1, col] != 6);
        bool hasLeft = (col > 0 && levelMap[row, col - 1] != 0 && levelMap[row, col - 1] != 5 && levelMap[row, col - 1] != 6);
        bool hasRight = (col < mapWidth - 1 && levelMap[row, col + 1] != 0 && levelMap[row, col + 1] != 5 && levelMap[row, col + 1] != 6);
        
        // Determine corner type based on connections
        if (hasRight && hasDown) return 0f;   // Top-left corner
        if (hasLeft && hasDown) return 90f;   // Top-right corner  
        if (hasLeft && hasUp) return 180f;    // Bottom-right corner
        if (hasRight && hasUp) return 270f;   // Bottom-left corner
        
        // Default based on position if unclear
        if (row == 0 && col == 0) return 0f;
        if (row == 0 && col == mapWidth - 1) return 90f;
        if (row == mapHeight - 1 && col == mapWidth - 1) return 180f;
        if (row == mapHeight - 1 && col == 0) return 270f;
        
        return 0f;
    }
    
    float CalculateWallRotation(int row, int col, bool isOutside)
    {
        // Check if we have connections to left/right or up/down
        bool hasHorizontalConnection = (col > 0 && IsWallType(levelMap[row, col - 1])) || 
                                     (col < mapWidth - 1 && IsWallType(levelMap[row, col + 1]));
        bool hasVerticalConnection = (row > 0 && IsWallType(levelMap[row - 1, col])) || 
                                   (row < mapHeight - 1 && IsWallType(levelMap[row + 1, col]));
        
        if (hasVerticalConnection && !hasHorizontalConnection) return 90f; // Vertical wall
        return 0f; // Horizontal wall
    }
    
    float CalculateTJunctionRotation(int row, int col)
    {
        // T-junction at position [0,7] should point up (270°)
        if (row == 0) return 270f; // T pointing up
        return 0f; // Default T pointing right
    }
    
    bool IsWallType(int tileType)
    {
        return tileType == 1 || tileType == 2 || tileType == 3 || tileType == 4 || tileType == 7 || tileType == 8;
    }
    
    float GetHorizontalFlippedCornerRotation(float originalRotation)
    {
        switch ((int)originalRotation)
        {
            case 0: return 90f;   // Top-left → Top-right
            case 90: return 0f;   // Top-right → Top-left  
            case 180: return 270f; // Bottom-right → Bottom-left
            case 270: return 180f; // Bottom-left → Bottom-right
            default: return originalRotation;
        }
    }
    
    float GetVerticalFlippedCornerRotation(float originalRotation)
    {
        switch ((int)originalRotation)
        {
            case 0: return 270f;   // Top-left → Bottom-left
            case 90: return 180f;  // Top-right → Bottom-right
            case 180: return 90f;  // Bottom-right → Top-right
            case 270: return 0f;   // Bottom-left → Top-left
            default: return originalRotation;
        }
    }
    
    float GetHorizontalFlippedTRotation(float originalRotation)
    {
        switch ((int)originalRotation)
        {
            case 0: return 180f;   // Right → Left
            case 90: return 90f;   // Down → Down
            case 180: return 0f;   // Left → Right
            case 270: return 270f; // Up → Up
            default: return originalRotation;
        }
    }
    
    float GetVerticalFlippedTRotation(float originalRotation)
    {
        switch ((int)originalRotation)
        {
            case 0: return 0f;     // Right → Right
            case 90: return 270f;  // Down → Up
            case 180: return 180f; // Left → Left
            case 270: return 90f;  // Up → Down
            default: return originalRotation;
        }
    }
    
    void InstantiateTile(int tileType, Vector3 position, float rotationZ)
    {
        GameObject prefab = GetPrefabForTileType(tileType);
        if (prefab == null) return;
        
        GameObject tile = Instantiate(prefab, position, Quaternion.Euler(0, 0, rotationZ));
        tile.transform.parent = levelParent.transform;
        
        // Name the tile for debugging
        tile.name = $"{GetTileTypeName(tileType)}_{position.x}_{-position.y}";
    }
    
    GameObject GetPrefabForTileType(int tileType)
    {
        switch (tileType)
        {
            case 1: return outsideCornerPrefab;
            case 2: return outsideWallPrefab;
            case 3: return insideCornerPrefab;
            case 4: return insideWallPrefab;
            case 5: return pelletPrefab;
            case 6: return powerPelletPrefab;
            case 7: return tJunctionPrefab;
            case 8: return ghostExitWallPrefab;
            default: return null;
        }
    }
    
    string GetTileTypeName(int tileType)
    {
        switch (tileType)
        {
            case 1: return "OutsideCorner";
            case 2: return "OutsideWall";
            case 3: return "InsideCorner";
            case 4: return "InsideWall";
            case 5: return "Pellet";
            case 6: return "PowerPellet";
            case 7: return "TJunction";
            case 8: return "GhostExit";
            default: return "Unknown";
        }
    }
    
    void AdjustCamera()
    {
        if (gameCamera == null) return;
        
        // Calculate the size needed to show the full level
        int fullWidth = mapWidth * 2 - 1;
        int fullHeight = mapHeight * 2 - 1;
        
        // Position camera at center of level
        gameCamera.transform.position = new Vector3(fullWidth / 2f, -fullHeight / 2f, gameCamera.transform.position.z);
        
        // Adjust orthographic size to fit the level
        float aspectRatio = (float)Screen.width / Screen.height;
        float verticalSize = fullHeight / 2f + 1f; // Add padding
        float horizontalSize = fullWidth / (2f * aspectRatio) + 1f;
        
        gameCamera.orthographicSize = Mathf.Max(verticalSize, horizontalSize);
    }
}