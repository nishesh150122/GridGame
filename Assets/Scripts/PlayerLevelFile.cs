using System;
using System.Collections.Generic;

[Serializable]
public class PlayerLevelFile
{
    public int width;
    public int height;
    public List<List<List<int>>> levels = new List<List<List<int>>>();
}