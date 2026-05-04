using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
    public static ChunkGenerator Instance { get; private set; }

    [Header("Prefab")]
    [SerializeField] private Transform platformPrefab;
    [SerializeField] private Transform bridgePrefab;
    [SerializeField] private Transform winPosPrefab;

    #region Load Chunk
    private LevelSO levelSO;
    private List<ChunkInstance> chunkList;
    private List<Vector3> nextAnchorPointList;
    #endregion

    #region Pooling Map
    private List<Platform> platformPool = new List<Platform>();
    private List<Bridge> bridgePool = new List<Bridge>();
    private List<Platform> activePlatformList = new List<Platform>();
    private List<Bridge> activeBridgeList = new List<Bridge>();
    #endregion

    private WinPos winPos;

    private void Awake() {

        Instance = this;

        chunkList = new List<ChunkInstance>();

        nextAnchorPointList = new List<Vector3>();
    }

    private void Start() {
        LevelManager.Instance.LoadNewMap += LevelManager_LoadNewMap;
    }

    private void OnDestroy() {

        LevelManager.Instance.LoadNewMap -= LevelManager_LoadNewMap;
    }

    private void LevelManager_LoadNewMap(object sender, LevelManager.LoadNewMapEventArgs e) {

        this.levelSO = e.levelSO;

        ClearLevelData();

        OnInit();
    }

    private void OnInit() {

        // Reset old data
        activePlatformList.Clear();
        activeBridgeList.Clear();

        chunkList.Clear();
        nextAnchorPointList.Clear();

        // Init chunk
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
                Vector2Int entryNodeLocal = chunkData.startNode.nodePos;
                Vector3 entryOffset = new Vector3(entryNodeLocal.x, 0f, entryNodeLocal.y);

                chunkTransform.localPosition = currentAnchor - entryOffset;
            }


            currentAnchor = chunkTransform.localPosition + nextAnchorPointList[i];
        }

        // After Spawn chunk done --> Spawn WinPos
        InitializeWinPos(currentAnchor);

    }

    private Platform SpawnPlatform(int x, int y, Transform parent) {

        Vector3 localPos = new Vector3(x, 0f, y);
        Platform platform = GetPlatformFromPool(parent, localPos);
        platform.gameObject.name = $"Platform_{x}_{y}";

        platform.ResetToDefault(); // Reset platform visual

        return platform;
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

        PathNode endNode = chunk.endNode;

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

            GetBridgeFromPool(chunkInstance.chunkTransform, spawnPos, angle);
            //Bridge.SpawnBridge(bridgePrefab, chunkInstance.chunkTransform, spawnPos, angle);
        }

        Vector3 nextChunkPos = spawnPos + spawnPosPlus;
        nextAnchorPointList.Add(nextChunkPos);
    }

    private void InitializeWinPos(Vector3 finalAnchorPos) {

        if (chunkList.Count == 0 || winPosPrefab == null) return;

        // Spawn
        if (winPos == null) {
            // Nếu chưa có winPos thì mới spawn lại
            Transform winPosTransform = Instantiate(winPosPrefab, this.transform);
            winPos = winPosTransform.GetComponent<WinPos>();
        }
        else {
            winPos.gameObject.SetActive(true);
        }


        // Gắn với điểm neo cuối cùng (Local Space)
        winPos.transform.localPosition = finalAnchorPos;
    }

    private void ClearLevelData() {

        // Clear Platform
        foreach (Platform platform in activePlatformList) {

            platform.gameObject.SetActive(false);
            platform.transform.SetParent(this.transform);

            platformPool.Add(platform);
        }
        activePlatformList.Clear();

        // Clear Bridge
        foreach (Bridge bridge in activeBridgeList) {

            bridge.gameObject.SetActive(false);
            bridge.transform.SetParent(this.transform);

            bridgePool.Add(bridge); 
        }
        activeBridgeList.Clear();

        // Delete game object Chunk
        foreach (var chunk in chunkList) {

            if (chunk.chunkTransform != null) {

                Destroy(chunk.chunkTransform.gameObject);
            }
        }

        chunkList.Clear();
        nextAnchorPointList.Clear();

        // Clear WinPos
        if (winPos != null) {

            winPos.gameObject.SetActive(false);     
        }
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
        //else {

        //    Platform platform = this.GetComponentInParent<Platform>();

        //    Debug.LogError($"Error happen at platform: {platform.gameObject.name}");
        //}
    }

    private Platform GetPlatformFromPool(Transform parent, Vector3 localPos) {

        Platform platform;

        if (platformPool.Count > 0) {
            // If there is an available platform in the pool

            platform = platformPool[platformPool.Count - 1];
            platformPool.RemoveAt(platformPool.Count - 1);

            platform.transform.SetParent(parent);
            platform.gameObject.SetActive(true);
        }
        else {
            // If pool is empty
            platform = Instantiate(platformPrefab, parent).GetComponent<Platform>();
        }

        platform.transform.localPosition = localPos;

        activePlatformList.Add(platform);

        return platform;
    }

    private Bridge GetBridgeFromPool(Transform parent, Vector3 localPos, Vector3 angleRotation) {

        Bridge bridge;

        if (bridgePool.Count > 0) {
            // If there is an available bridge in the pool

            bridge = bridgePool[bridgePool.Count - 1];
            bridgePool.RemoveAt(bridgePool.Count - 1);
            bridge.gameObject.SetActive(true);

            bridge.transform.SetParent(parent);
            bridge.transform.localPosition = localPos;
            bridge.transform.localRotation = Quaternion.Euler(angleRotation);
        }
        else {
            // If pool is empty

            bridge = Bridge.SpawnBridge(bridgePrefab, parent, localPos, angleRotation);
        }

        bridge.HideStack();

        activeBridgeList.Add(bridge);

        return bridge;
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
