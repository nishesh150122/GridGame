//#if UNITY_EDITOR
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using Newtonsoft.Json;
//using UnityEditor;
//using UnityEngine;
//using Random = System.Random;

//public class LevelAutoGenerate : EditorWindow
//{
//    int levelWidth = 5;
//    int levelHeight = 5;
//    string jsonPath = "Assets/Resources/Player.json";
    
//    [MenuItem("/Tools/Level Auto Generate")]
//    // Start is called before the first frame update
//    public static void ShowWindow()
//    {
//        GetWindow<LevelAutoGenerate>("Level Auto Generate");
//    }

//    private void OnGUI()
//    {
//        GUILayout.Label("Level Auto Generator", EditorStyles.boldLabel);
//        GUILayout.Space(5);

//        levelWidth = EditorGUILayout.IntSlider("Inner Width", levelWidth, 3, 12);
//        levelHeight = EditorGUILayout.IntSlider("Inner Height", levelHeight, 3, 12);

//        GUILayout.Space(5);
//        jsonPath = EditorGUILayout.TextField("Player.json Path", jsonPath);

//        GUILayout.Space(10);

//        if (GUILayout.Button("Generate & Append New Level", GUILayout.Height(35)))
//        {
//            GenerateandAppend();
//        }

//        GUILayout.Space(5);
//        if (GUILayout.Button("Preview Last Generated Level in Console"))
//        {
//            PreviewLastLevel();
//        }

//    }

//    private void PreviewLastLevel()
//    {
//        throw new NotImplementedException();
//    }

//    private void GenerateandAppend()
//    {
//        PlayerLevelFile levelFile = ReadExistingFile();
//        List<List<int>> newGrid = GenerateLevel(levelWidth, levelHeight);

//        if(newGrid==null)
//        {
//            Debug.LogError("[Level Auto Genrate] Generation failed after max retries." + "Try again Later");
//            return;
//        }
//        levelFile.levels.Add(newGrid);
//        WriteFile(levelFile);
//        int newIndex = levelFile.levels.Count - 1;
//        Debug.Log($"[LevelAutoGenerator] ✅ Level {newIndex} appended to {jsonPath} " +
//                  $"| Grid: {levelWidth + 2} cols × {levelHeight + 2} rows (border included)");
//    }

//    private List<List<int>> GenerateLevel(int innerW, int innerH)
//    {
//        int maxRetries = 200;
//        for(int attempt=0;attempt<maxRetries;attempt++)
//        {
//            int totalCols = innerW + 2;
//            int totalRows=innerH + 2;

//            int[,] inner = new int[innerH, innerW];
            
//            int startRow = Random.Range(0, innerH);
//            int startCol = Random.Range(0, innerW);

//            int goalRow, goalCol;
//            do
//            {
//                goalRow = Random.Range(0, innerH);
//                goalCol = Random.Range(0, innerH);
//            }
//            while (Mathf.Abs(goalRow - startRow) + Mathf.Abs(goalCol - startCol) < 3);
//        }
//    }

//    // ─────────────────────────────────────────────────────────────
//    //  CORE ALGORITHM
//    //  Produces a 2D grid with:
//    //   - 0-border padding all around  (matches your JSON format)
//    //   - 2 (start) on the left column
//    //   - 3 (goal)  on the right column
//    //   - Guaranteed connected walkable path of 1s from 2 → 3
//    // ─────────────────────────────────────────────────────────────
//    private void PreviewLastLeve()
//    {
//        if (!FilePathAttribute.Exits(jsonPath))
//        {
//            Debug.LogError("[Level Auto Generate] File not Found");
//            return;
//        }
//        var file = JsonConvert.DeserializtionObject<PlayerLevelFile>(file.ReadAllText(jsonPath));

//        if(file==null|| file.levels.Count == 0)
//        {
//            Debug.Log("[LevelAutoGenerate] Np Levels Found in File.");
//            return ;
//        }

//        var last = file.levels[file.levels.Count - 1];
//        var sb = new System.Text.StringBuilder();
//        sb.AppendLine($"==Level {file.levels.Count - 1} (index ) Preview==");

//        foreach(var row in last)
//        {
//            foreach(var cell in row)
//            {
//                sb.Append(cell switch
//                {
//                    0 => " .",
//                    1 => " #",
//                    2 => " S",
//                    3 => " G",
//                    4 => " B",
//                    5 => " F",
//                    _ => " ?"
//                });
//            }
//            sb.AppendLine();
//        }
//    }
//    // Update is called once per frame
//    void Update()
//    {
        
//    }
//}
//#endif