using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
    public static ChunkGenerator Instance { get; private set; }

    [Header("Path Data")]
    [SerializeField] private LevelSO levelSO;

    [Header("Prefab")]
    [SerializeField] private Transform platformPrefab;
    [SerializeField] private Transform bridgePrefab;

    private List<ChunkInstance> chunkList;

    private void Awake() {

        Instance = this;

        chunkList = new List<ChunkInstance>();

        InitializeChunk();

    }

    private void InitializeChunk() {

        int chunkCount = levelSO.chunkList.Count;

        for (int i = 0; i < chunkCount; i++) {

            ChunkData chunkData = levelSO.chunkList[i];

            ChunkInstance newChunk = new ChunkInstance(i, chunkData, this.transform);
            chunkList.Add(newChunk);

            // Spawn Platform
            for (int x = 0; x < chunkData.chunkWidth; x++) {

                for (int y = 0; y < chunkData.chunkHeight; y++) {

                    Vector2Int nodePos = new Vector2Int(x, y);
                    Platform platformVisual = SpawnPlatform(x, y, newChunk.chunkTransform);

                    newChunk.gridNodeDict.Add(nodePos, platformVisual);
                    newChunk.gridMaps[x, y] = GridNode.NodeID.Empty;
                }
            }

            // Spawn Path
            InitializePathByChunk(newChunk, chunkData);
        }

        // After Spawn chunk done --> Spawn WinPos
        InitializeWinPos();


    }

    private Platform SpawnPlatform(int x, int y, Transform parent) {

        Transform platformTransform = Instantiate(platformPrefab, parent);

        platformTransform.localPosition = new Vector3(x, 0, y);
        platformTransform.gameObject.name = $"Platform_{x}_{y}";

        return platformTransform.GetComponent<Platform>();
    }

    private void InitializePathByChunk(ChunkInstance chunkInstance, ChunkData chunkData) {

        List<PathNode> pathNodeList = chunkData.pathNodeList;

        // Spawn path
        foreach (PathNode pathNode in pathNodeList) {

            Vector2Int nodePos = pathNode.nodePos;

            chunkInstance.gridNodeDict[nodePos].ShowStack();
            chunkInstance.gridMaps[nodePos.x, nodePos.y] = GridNode.NodeID.Path;
        }

        // Spawn Corner
        foreach (PathNode pathNode in pathNodeList) {

            if (!pathNode.hasCornerOn) { continue; }

            ApplyCornerRotation(chunkInstance, pathNode.nodePos);
        }

        // Spawn floor
        foreach (Vector2Int keyPos in chunkInstance.gridNodeDict.Keys) {

            if (chunkInstance.gridMaps[keyPos.x, keyPos.y] != GridNode.NodeID.Path) {

                chunkInstance.gridNodeDict[keyPos].ShowFloor();
                chunkInstance.gridMaps[keyPos.x, keyPos.y] = GridNode.NodeID.Floor;
            }

        }

        InitializeBridge(chunkInstance, chunkData);
    }

    private void InitializeBridge(ChunkInstance chunkInstance, ChunkData chunk) {

        PathNode endNode = chunk.pathNodeList[chunk.pathNodeList.Count - 1];

        Vector3 angle = Vector3.zero;
        Vector3 spawnPos = new Vector3(endNode.nodePos.x, 0f, endNode.nodePos.y);
        Vector3 spawnPosPlus = Vector3.zero;

        // 1. Kiểm tra biên endNode
        if (endNode.nodePos.x == chunk.chunkWidth - 1) {
            // Biên phải

            angle = Vector3.zero;
            spawnPosPlus = new Vector3(1f, 0f, 0f);

        }
        else if (endNode.nodePos.y == 0) {
            // Biên dưới

            angle = new Vector3(0, 90, 0);
            spawnPosPlus = new Vector3(0f, 0f, -1f);
        }
        else if (endNode.nodePos.x == 0) {
            // Biên trái

        }
        else if (endNode.nodePos.y == chunk.chunkHeight - 1) {
            // Biên trên
        }
        else {
            Debug.LogError("Nothing dir true");
        }

            // 2. Spawn Bridge
        int bridgeCount = chunk.bridgeCount;
        for (int i = 0; i < bridgeCount; i++) {

            spawnPos += spawnPosPlus;

            Bridge.SpawnBridge(bridgePrefab, chunkInstance.chunkTransform, spawnPos, angle);
        }
        
    }

    private void InitializeWinPos() {
        if (chunkList.Count == 0) return;

        ChunkInstance lastChunkInstance = chunkList[chunkList.Count - 1];
        ChunkData lastChunkData = levelSO.chunkList[levelSO.chunkList.Count - 1];

        PathNode lastNode = lastChunkData.pathNodeList[lastChunkData.pathNodeList.Count - 1];

        float offsetX = lastNode.nodePos.x + lastChunkData.bridgeCount + 1;

        Vector3 localWinPos = new Vector3(offsetX, 0, lastNode.nodePos.y);


        Vector3 worldWinPos = lastChunkInstance.chunkTransform.TransformPoint(localWinPos);

        // Spawn
        Transform winPosTransform = Instantiate(levelSO.winPosPrefab, this.transform);
        winPosTransform.position = worldWinPos;
    }

    private void ApplyCornerRotation(ChunkInstance chunkInstance, Vector2Int nodePos) {

        Vector2Int upNode = nodePos + Vector2Int.up;
        Vector2Int rightNode = nodePos + Vector2Int.right;
        Vector2Int downNode = nodePos + Vector2Int.down;
        Vector2Int leftNode = nodePos + Vector2Int.left;

        bool hasUpPath = chunkInstance.IsPathNode(upNode);
        bool hasRightPath = chunkInstance.IsPathNode(rightNode);
        bool hasDownPath = chunkInstance.IsPathNode(downNode);
        bool hasLeftPath = chunkInstance.IsPathNode(leftNode);

        if (hasRightPath && hasDownPath) {
            chunkInstance.gridNodeDict[nodePos].ShowCorner(Corner.CornerType.RightDown);
        }
        else if (hasLeftPath && hasDownPath) {
            chunkInstance.gridNodeDict[nodePos].ShowCorner(Corner.CornerType.LeftDown);
        }
        else if (hasRightPath && hasUpPath) {
            chunkInstance.gridNodeDict[nodePos].ShowCorner(Corner.CornerType.RightUp);
        }
        else if (hasLeftPath && hasUpPath) {
            chunkInstance.gridNodeDict[nodePos].ShowCorner(Corner.CornerType.LeftUp);
        }
        else {
            Debug.LogError("Nothing dir true");
        }
    }
    

    public LevelSO GetLevelSO() {
        return this.levelSO;
    }

    public List<ChunkInstance> GetChunkList() {
        return this.chunkList;
    }

}

public class ChunkInstance {
    public Transform chunkTransform { get; private set; }
    public GridNode.NodeID[,] gridMaps { get; private set; }
    public Dictionary<Vector2Int, Platform> gridNodeDict { get; private set; }

    private ChunkData chunkData;

    public ChunkInstance(int index, ChunkData chunkData, Transform parent) {

        this.chunkData = chunkData;
        this.gridNodeDict = new Dictionary<Vector2Int, Platform>();
        this.gridMaps = new GridNode.NodeID[chunkData.chunkWidth, chunkData.chunkHeight];

        // Setup chunk
        GameObject chunkObj = new GameObject($"Chunk_{index}");
        chunkTransform = chunkObj.transform;
        chunkTransform.SetParent(parent);
        chunkTransform.localPosition = chunkData.chunkPos;
        chunkTransform.localRotation = Quaternion.Euler(chunkData.chunkRotation);
    }



    public bool IsValidNode(Vector2Int pos) {
        return pos.x >= 0 && pos.x < chunkData.chunkWidth && pos.y >= 0 && pos.y < chunkData.chunkHeight;
    }

    public bool IsPathNode(Vector2Int pos) {
        return IsValidNode(pos) && gridMaps[pos.x, pos.y] == GridNode.NodeID.Path;
    }
}
