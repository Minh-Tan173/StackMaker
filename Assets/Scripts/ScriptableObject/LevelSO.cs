using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class LevelSO : ScriptableObject
{
    [Header("Chunk")]
    public Transform winPosPrefab;
    public List<ChunkData> chunkList;

}

[System.Serializable]
public class ChunkData {

    [Header("Name")]
    public string chunkName;

    [Header("Size")]
    public int chunkWidth;
    public int chunkHeight;

    [Header("Path Data")]
    public List<PathNode> pathNodeList;

    [Header("Bridge Data")]
    public int bridgeCount;

}

[System.Serializable]
public class PathNode {

    public Vector2Int nodePos;
    public bool hasCornerOn;
}
