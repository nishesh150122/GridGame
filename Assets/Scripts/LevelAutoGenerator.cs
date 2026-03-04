#if UNITY_EDITOR
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Unity Editor Window tool.
/// Open via:  Tools → Level Auto Generator
///
/// What it does:
///   1. Reads existing Player.json  (never destroys existing levels)
///   2. Algorithmically generates a new playable level grid
///   3. Validates the level is solvable with BFS
///   4. Appends the new level and writes the file back
///
/// Tile legend:
///   0 = empty/void
///   1 = walkable floor
///   2 = start position
///   3 = goal hole
/// </summary>
public class LevelAutoGenerator : EditorWindow
{
    // ── Editor Window Fields ──────────────────────────────────────
    private int levelWidth = 5;  // inner playable width  (border NOT included)
    private int levelHeight = 5;  // inner playable height (border NOT included)
    private string jsonPath = "Assets/Resources/Player.json";

    [MenuItem("Tools/Level Auto Generator")]
    public static void ShowWindow()
    {
        GetWindow<LevelAutoGenerator>("Level Auto Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Bloxorz Level Auto Generator", EditorStyles.boldLabel);
        GUILayout.Space(5);

        levelWidth = EditorGUILayout.IntSlider("Inner Width", levelWidth, 3, 12);
        levelHeight = EditorGUILayout.IntSlider("Inner Height", levelHeight, 3, 12);

        GUILayout.Space(5);
        jsonPath = EditorGUILayout.TextField("Player.json Path", jsonPath);

        GUILayout.Space(10);

        if (GUILayout.Button("Generate & Append New Level", GUILayout.Height(35)))
        {
            GenerateAndAppend();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Preview Last Generated Level in Console"))
        {
            PreviewLastLevel();
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  MAIN ENTRY POINT
    // ─────────────────────────────────────────────────────────────
    private void GenerateAndAppend()
    {
        // Step 1: Read existing JSON — preserve ALL existing levels
        PlayerLevelFile levelFile = ReadExistingFile();

        // Step 2: Generate a new valid level grid
        List<List<int>> newGrid = GenerateLevel(levelWidth, levelHeight);

        if (newGrid == null)
        {
            Debug.LogError("[LevelAutoGenerator] Generation failed after max retries. " +
                           "Try a larger width/height.");
            return;
        }

        // Step 3: Append ONLY — never touch existing levels
        levelFile.levels.Add(newGrid);

        // Step 4: Write back to the same file
        WriteFile(levelFile);

        int newIndex = levelFile.levels.Count - 1;
        Debug.Log($"[LevelAutoGenerator] ✅ Level {newIndex} appended to {jsonPath} " +
                  $"| Grid: {levelWidth + 2} cols × {levelHeight + 2} rows (border included)");
    }

    // ─────────────────────────────────────────────────────────────
    //  CORE ALGORITHM
    //  Produces a 2D grid with:
    //   - 0-border padding all around  (matches your JSON format)
    //   - 2 (start) on the left column
    //   - 3 (goal)  on the right column
    //   - Guaranteed connected walkable path of 1s from 2 → 3
    // ─────────────────────────────────────────────────────────────
    private List<List<int>> GenerateLevel(int innerW, int innerH)
    {
        int maxRetries = 200;

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            int totalCols = innerW + 2;  // includes left+right border
            int totalRows = innerH + 2;  // includes top+bottom border

            // Work in inner space — indices [0..innerH-1][0..innerW-1]
            int[,] inner = new int[innerH, innerW]; // all 0

            // ── Place START (2) on left column, random row ──
            // Place START (2) fully random anywhere in the inner grid
            int startRow = Random.Range(0, innerH);
            int startCol = Random.Range(0, innerW);

            // Place GOAL (3) fully random — keep re-picking until far enough from start
            int goalRow, goalCol;
            do
            {
                goalRow = Random.Range(0, innerH);
                goalCol = Random.Range(0, innerW);
            }
            while (Mathf.Abs(goalRow - startRow) + Mathf.Abs(goalCol - startCol) < 3);
            // The < 3 check ensures start and goal aren't the same cell or right next to each other

            inner[startRow, startCol] = 2;
            inner[goalRow, goalCol] = 3;

            // ── Carve a guaranteed connected path via BFS ──
            bool pathOk = CarvePathBFS(inner, startRow, startCol,
                                             goalRow, goalCol,
                                             innerH, innerW);
            if (!pathOk) continue;

            // ── Scatter extra tiles for visual variety ──
            AddRandomTiles(inner, innerH, innerW);

            // ── Validate: BFS must still confirm solvable ──
            if (!IsGoalReachable(inner, startRow, startCol, goalRow, goalCol, innerH, innerW))
                continue;

            // ── Wrap with 0-border and return ──
            return WrapWithBorder(inner, innerH, innerW, totalRows, totalCols);
        }

        return null; // exhausted retries
    }

    // ─────────────────────────────────────────────────────────────
    //  BFS PATH CARVER
    //  Uses BFS to find ANY path from start → goal across the
    //  empty grid, then marks every cell on that path as 1.
    //  Because BFS explores randomly shuffled directions,
    //  the carved path varies each generation.
    // ─────────────────────────────────────────────────────────────
    private bool CarvePathBFS(int[,] inner, int sr, int sc,
                               int gr, int gc, int h, int w)
    {
        int[,] parentRow = new int[h, w];
        int[,] parentCol = new int[h, w];

        for (int r = 0; r < h; r++)
            for (int c = 0; c < w; c++)
            {
                parentRow[r, c] = -1;
                parentCol[r, c] = -1;
            }

        bool[,] visited = new bool[h, w];
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(sr, sc));
        visited[sr, sc] = true;

        int[] dr = { -1, 1, 0, 0 };
        int[] dc = { 0, 0, -1, 1 };

        bool found = false;

        while (queue.Count > 0)
        {
            Vector2Int cur = queue.Dequeue();

            if (cur.x == gr && cur.y == gc)
            {
                found = true;
                break;
            }

            ShuffleDirections(dr, dc); // randomise direction order

            for (int d = 0; d < 4; d++)
            {
                int nr = cur.x + dr[d];
                int nc = cur.y + dc[d];

                if (nr >= 0 && nr < h && nc >= 0 && nc < w && !visited[nr, nc])
                {
                    visited[nr, nc] = true;
                    parentRow[nr, nc] = cur.x;
                    parentCol[nr, nc] = cur.y;
                    queue.Enqueue(new Vector2Int(nr, nc));
                }
            }
        }

        if (!found) return false;

        // Trace back from goal → start and carve 1s
        int row = gr, col = gc;
        while (!(row == sr && col == sc))
        {
            if (inner[row, col] == 0)  // don't overwrite 2 or 3
                inner[row, col] = 1;

            int pr = parentRow[row, col];
            int pc = parentCol[row, col];
            row = pr;
            col = pc;
        }

        return true;
    }

    // ─────────────────────────────────────────────────────────────
    //  RANDOM EXTRA TILES
    //  Scatters 20–40% additional floor tiles so levels don't
    //  look like thin corridors.
    // ─────────────────────────────────────────────────────────────
    private void AddRandomTiles(int[,] inner, int h, int w)
    {
        int extras = Random.Range(
            Mathf.CeilToInt(h * w * 0.2f),
            Mathf.CeilToInt(h * w * 0.4f));

        for (int i = 0; i < extras; i++)
        {
            int r = Random.Range(0, h);
            int c = Random.Range(0, w);
            if (inner[r, c] == 0)
                inner[r, c] = 1;
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  BFS VALIDATOR
    //  Walks from start position.
    //  Any non-zero tile is treated as walkable.
    //  Returns true only if goal cell is reached.
    // ─────────────────────────────────────────────────────────────
    private bool IsGoalReachable(int[,] inner, int sr, int sc,
                                  int gr, int gc, int h, int w)
    {
        bool[,] visited = new bool[h, w];
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(sr, sc));
        visited[sr, sc] = true;

        int[] dr = { -1, 1, 0, 0 };
        int[] dc = { 0, 0, -1, 1 };

        while (queue.Count > 0)
        {
            Vector2Int cur = queue.Dequeue();
            if (cur.x == gr && cur.y == gc) return true;

            for (int d = 0; d < 4; d++)
            {
                int nr = cur.x + dr[d];
                int nc = cur.y + dc[d];

                if (nr >= 0 && nr < h && nc >= 0 && nc < w
                    && !visited[nr, nc]
                    && inner[nr, nc] != 0)  // 0 = void, impassable
                {
                    visited[nr, nc] = true;
                    queue.Enqueue(new Vector2Int(nr, nc));
                }
            }
        }

        return false;
    }

    // ─────────────────────────────────────────────────────────────
    //  WRAP WITH BORDER
    //  Adds the one-cell ring of 0s around the inner grid,
    //  exactly matching the format of your Player.json levels.
    // ─────────────────────────────────────────────────────────────
    private List<List<int>> WrapWithBorder(int[,] inner, int innerH, int innerW,
                                            int totalRows, int totalCols)
    {
        var grid = new List<List<int>>();

        for (int r = 0; r < totalRows; r++)
        {
            var row = new List<int>();
            for (int c = 0; c < totalCols; c++)
            {
                bool isBorder = (r == 0 || r == totalRows - 1 ||
                                 c == 0 || c == totalCols - 1);

                row.Add(isBorder ? 0 : inner[r - 1, c - 1]);
            }
            grid.Add(row);
        }

        return grid;
    }

    // ─────────────────────────────────────────────────────────────
    //  FILE I/O
    // ─────────────────────────────────────────────────────────────
    private PlayerLevelFile ReadExistingFile()
    {
        if (!File.Exists(jsonPath))
        {
            Debug.LogWarning($"[LevelAutoGenerator] {jsonPath} not found. A new file will be created.");
            return new PlayerLevelFile { width = levelWidth, height = levelHeight };
        }

        string json = File.ReadAllText(jsonPath);
        var file = JsonConvert.DeserializeObject<PlayerLevelFile>(json);

        if (file == null)
        {
            Debug.LogWarning("[LevelAutoGenerator] Failed to parse existing JSON. Starting fresh.");
            return new PlayerLevelFile { width = levelWidth, height = levelHeight };
        }

        return file;
    }

    private void WriteFile(PlayerLevelFile file)
    {
        string json = JsonConvert.SerializeObject(file, Formatting.Indented);
        File.WriteAllText(jsonPath, json);
        AssetDatabase.Refresh();
    }

    // ─────────────────────────────────────────────────────────────
    //  UTILITY — Fisher-Yates shuffle for BFS direction arrays
    // ─────────────────────────────────────────────────────────────
    private void ShuffleDirections(int[] dr, int[] dc)
    {
        for (int i = dr.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (dr[i], dr[j]) = (dr[j], dr[i]);
            (dc[i], dc[j]) = (dc[j], dc[i]);
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  DEBUG PREVIEW — prints last level as ASCII to console
    //  .  =  0  empty
    //  #  =  1  floor
    //  S  =  2  start
    //  G  =  3  goal
    // ─────────────────────────────────────────────────────────────
    private void PreviewLastLevel()
    {
        if (!File.Exists(jsonPath))
        {
            Debug.LogError("[LevelAutoGenerator] File not found.");
            return;
        }

        var file = JsonConvert.DeserializeObject<PlayerLevelFile>(File.ReadAllText(jsonPath));

        if (file == null || file.levels.Count == 0)
        {
            Debug.Log("[LevelAutoGenerator] No levels found in file.");
            return;
        }

        var last = file.levels[file.levels.Count - 1];
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"=== Level {file.levels.Count - 1} (index) Preview ===");

        foreach (var row in last)
        {
            foreach (var cell in row)
            {
                sb.Append(cell switch
                {
                    0 => " .",
                    1 => " #",
                    2 => " S",
                    3 => " G",
                    4 => " B",
                    5 => " F",
                    _ => " ?"
                });
            }
            sb.AppendLine();
        }

        Debug.Log(sb.ToString());
    }
}
#endif