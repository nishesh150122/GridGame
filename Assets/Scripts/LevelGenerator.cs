using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    public int width = 5;
    public int height = 5;

 public int[,] grid;

    private Vector2Int startPos;
    private Vector2Int goalPos;

    void Start()
    {
        GenerateLevel();
        PrintLevel();
    }

    void GenerateLevel()
    {
        grid = new int[height, width];

        // 1?? Fill with empty
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                grid[y, x] = 0;

        // 2?? Choose start & goal
        startPos = new Vector2Int(1, 1);
        goalPos = new Vector2Int(width - 2, height - 2);

        // 3?? Generate guaranteed path
        List<Vector2Int> path = GeneratePath(startPos, goalPos);

        foreach (var p in path)
            grid[p.y, p.x] = 1;

        grid[startPos.y, startPos.x] = 2;
        grid[goalPos.y, goalPos.x] = 3;

        // 4?? Create gap(s)
        Vector2Int gapPos = path[path.Count / 2];
        grid[gapPos.y, gapPos.x] = 5;

        // 5?? Place bridge trigger (off main path)
        Vector2Int triggerPos = FindTriggerPosition(path);
        grid[triggerPos.y, triggerPos.x] = 4;
    }

    // --------------------------------------------------------

    List<Vector2Int> GeneratePath(Vector2Int start, Vector2Int goal)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = start;

        path.Add(current);

        while (current != goal)
        {
            if (Random.value > 0.5f && current.x != goal.x)
                current.x += (goal.x > current.x) ? 1 : -1;
            else if (current.y != goal.y)
                current.y += (goal.y > current.y) ? 1 : -1;

            if (!path.Contains(current))
                path.Add(current);
        }

        return path;
    }

    Vector2Int FindTriggerPosition(List<Vector2Int> path)
    {
        foreach (var p in path)
        {
            Vector2Int candidate = p + Vector2Int.left;
            if (IsInside(candidate) && grid[candidate.y, candidate.x] == 0)
                return candidate;
        }

        // fallback
        return path[1] + Vector2Int.up;
    }

    bool IsInside(Vector2Int pos)
    {
        return pos.x > 0 && pos.x < width - 1 &&
               pos.y > 0 && pos.y < height - 1;
    }

    // --------------------------------------------------------

    void PrintLevel()
    {
        Debug.Log("Generated Level:");
        for (int y = 0; y < height; y++)
        {
            string row = "";
            for (int x = 0; x < width; x++)
                row += grid[y, x] + " ";
            Debug.Log(row);
        }
    }
}
