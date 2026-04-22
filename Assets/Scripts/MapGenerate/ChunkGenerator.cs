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
    private List<Vector3> lastBrideNodePosPlus;

    private void Awake() {

        Instance = this;

        chunkList = new List<ChunkInstance>();

        lastBrideNodePosPlus = new List<Vector3>();

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

            // Spawn Bridge
            InitializeBridge(newChunk, chunkData);
        }

        // After spawn Path/Bridge --> Set chunkPos base on bridge
        Vector3 currentAnchor = Vector3.zero;

        for (int i = 0; i < chunkList.Count; i++) {

            Transform chunkTransform = chunkList[i].chunkTransform;
            ChunkData chunkData = levelSO.chunkList[i];

            if (i == 0) {
                chunkTransform.localPosition = Vector3.zero;
            }
            else {
                Vector2Int entryNodeLocal = chunkData.pathNodeList[0].nodePos;
                Vector3 entryOffset = new Vector3(entryNodeLocal.x, 0f, entryNodeLocal.y);

                chunkTransform.localPosition = currentAnchor - entryOffset;
            }


            currentAnchor = chunkTransform.localPosition + lastBrideNodePosPlus[i];
        }

        // After Spawn chunk done --> Spawn WinPos
        InitializeWinPos(currentAnchor);

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
                chunkInstance.gridMaps[keyPos.x, keyPos.y] = GridNode.NodeID.Wall;
            }

        }

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

        Vector3 nextChunkPos = spawnPos + spawnPosPlus;
        lastBrideNodePosPlus.Add(nextChunkPos);
    }

    private void InitializeWinPos(Vector3 finalAnchorPos) {

        if (chunkList.Count == 0 || levelSO.winPosPrefab == null) return;

        // Spawn
        Transform winPosTransform = Instantiate(levelSO.winPosPrefab, this.transform);

        // Gắn với điểm neo cuối cùng (Local Space)
        winPosTransform.localPosition = finalAnchorPos;
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
    }

    public bool IsValidNode(Vector2Int pos) {
        return pos.x >= 0 && pos.x < chunkData.chunkWidth && pos.y >= 0 && pos.y < chunkData.chunkHeight;
    }

    public bool IsPathNode(Vector2Int pos) {
        return IsValidNode(pos) && gridMaps[pos.x, pos.y] == GridNode.NodeID.Path;
    }
}
