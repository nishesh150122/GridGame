using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;
using System;
using DG.Tweening;

public class GridGenerator : MonoBehaviour
{
    public static GridGenerator Instance;
    private string fileName = "Player";
    private string filePath;
   // LevelRoot levelData;
    [SerializeField] int tileSize = 1;
    [SerializeField] GameObject floorPrefab, switchPrefab, goalPrefab,bridgePrefab;
    public int level;
    [SerializeField] private GameObject player;
    public int rows,columns;
   public JObject parsedData;
    public int tile;

    private void Awake()
    {
        Instance = this;
       // levelData = new LevelRoot();
        filePath = Path.Combine(Application.persistentDataPath,fileName);
        //ResetGame();
       //  InitializeSaveFile();
        LoadGame();




    }
    void InitializeSaveFile()
    {
        if(!File.Exists(filePath))
        {
            Debug.Log("Save file not found. Creating new one...");
            TextAsset playerJson = Resources.Load<TextAsset>("Player");
            if(playerJson != null)
            {
                File.WriteAllText(filePath, playerJson.text);
            }
            else
            {
              
            }
        }

    }
  
    void LoadGame()
    {
        GameObject switchObject = null;
     //   if(File.Exists(filePath))
     {
         var filePath = Resources.Load<TextAsset>("Player");
           // string json = File.ReadAllText(filePath.ToString());
     
            parsedData = JObject.Parse(filePath.ToString());
             rows = ((JArray)parsedData["levels"][level]).Count;
             columns = ((JArray)parsedData["levels"][level][0]).Count;
           // Debug.Log(rows + " " + columns);

            
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                     tile = (int) parsedData["levels"][level][y][x];
                    
                    Vector3 position = new Vector3(
                        x * tileSize,
                        0,
                        -y * tileSize
                    );

                    switch (tile)
                    {
                        case 1:
                           var floor = Instantiate(floorPrefab, position, Quaternion.identity);
                            floor.transform.DOScale(new Vector3(1, 0.1f, 1), 1)
                                .SetEase(Ease.InSine);
                               
                            break;

                        case 2:
                           switchObject =  Instantiate(switchPrefab, position, Quaternion.identity);
                            switchObject.transform.DOScale(new Vector3(1, 0.1f, 1), 1)
                               .SetEase(Ease.InSine);
                            // spawn player
                            
                          
                          
                            break;

                        case 3:
                           var goalObject = Instantiate(goalPrefab, position, Quaternion.identity);
                            goalObject.transform.DOScale(new Vector3(1, 0.1f, 1),1 )
                               .SetEase(Ease.InSine);
                            break;
                        case 4:
                            var bridgeObject = Instantiate(bridgePrefab, position, Quaternion.identity);
                            bridgeObject.transform.DOScale(new Vector3(1, 0.1f, 1), 1)
                               .SetEase(Ease.InSine);
                            break;
                    }
                }
            }
        }
        Instantiate(player,switchObject.transform.position + new Vector3(0f,1f,0f),Quaternion.identity);
    }
    
    void ResetGame()
    {
        if(File.Exists(filePath))
        {
            File.Delete(filePath);
        }
       // InitializeSaveFile();
        //LoadGame();
    }
    
}

[Serializable]
public class LevelRoot
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

    public int width { get; set; }
    public int height { get; set; }
    public List<List<int>> tiles { get; set; }


}