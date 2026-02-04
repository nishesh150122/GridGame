using UnityEngine;
using System.Collections.Generic;

public class BridgeTrigger : MonoBehaviour
{
    public static BridgeTrigger Instance;
    GridGenerator gridGenerator;
  
    public int[,] grid;

    public GameObject bridgeTilePrefab;
    bool hasCollided;

    void Awake()
    {
        gridGenerator = GridGenerator.Instance;
        
        Instance = this;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!hasCollided)
        {
            ActivateBridge();
            hasCollided = true;
        }
       
    }

    public void ActivateBridge()
    {
        
        for (int y = 0; y < gridGenerator.rows; y++)
        {
            for (int x = 0; x < gridGenerator.columns; x++)
            {
                Debug.Log("sdf");
                var tile = (int)gridGenerator.parsedData["levels"][gridGenerator.level][y][x];
                if (tile == 5)
                {
                   // grid[y, x] = 1;
                    SpawnBridgeTile(x, y);
                }
            }
        }
    }

    void SpawnBridgeTile(int x, int y)
    {
        Vector3 position = new Vector3(
                       x * 1,
                       0,
                       -y * 1
                   );
       // Vector3 worldPos = new Vector3(x, 0, y);
        Instantiate(bridgeTilePrefab, position, Quaternion.identity);
    }
}
