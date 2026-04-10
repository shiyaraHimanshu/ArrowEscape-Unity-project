
using System.Collections.Generic;
using UnityEngine;

namespace ArrowGameLevelEditor
{    
    [System.Serializable]
    public class LevelData
    {
        public int GridXSize;
        public int GridYSize;
        public List<ArrowData> Arrows = new();
    }
    [System.Serializable]
    public class ArrowData
    {
        public List<int> Indices = new();
        public int ColorIndex=0;
    }    
}

