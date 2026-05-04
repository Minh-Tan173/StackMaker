using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class LevelEditorUI : MonoBehaviour
{
    public static LevelEditorUI Instance { get; private set; }

    [Header("Data")]
    [SerializeField] private LevelSO levelSO;

    [Header("Button")]
    [SerializeField] private Button resetChunkButton;
    [SerializeField] private Button nextChunkButton;
    [SerializeField] private Button backChunkButton;
    [SerializeField] private Button addChunkButton;
    [SerializeField] private Button deleteChunkButton;
    [SerializeField] private Button gameModeButton;
    [SerializeField] private Button closeButton;

    [Header("Grid Data")]
    [SerializeField] private Transform chunkGrid;
    [SerializeField] private Transform nodeButtonPrefab;

    [Header("Input Field Text")]
    [SerializeField] private TMP_InputField chunkNameInputField;
    [SerializeField] private TMP_InputField widthInputField;
    [SerializeField] private TMP_InputField heightInputField;
    [SerializeField] private TMP_InputField bridgeCountInputField;
    [SerializeField] private int minValue;
    [SerializeField] private int maxValue;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI totalPathCountText;
    [SerializeField] private TextMeshProUGUI pathCountPerChunkText;


    private int currentChunkIndex = 0; // Mặc định khi bắt đầu là chunk index 0

    private List<NodeButton> nodeButtonList;

    #region Path Node Info
    private int lastCachedTotal;
    private int lastCachedPathPerChunk;
    private Dictionary<ChunkData, int> pathCountPerChunkDict;
    #endregion

    private void Awake() {

        Instance = this;

        nodeButtonList = new List<NodeButton>();

        pathCountPerChunkDict = new Dictionary<ChunkData, int>();

        #region Button Listener
        resetChunkButton.onClick.AddListener(() => {

            if (currentChunkIndex >= levelSO.chunkList.Count - 1) { return; }

            ChunkData chunkData = levelSO.chunkList[currentChunkIndex];

            chunkData.startNode.nodePos = Vector2Int.zero;
            chunkData.endNode.nodePos = Vector2Int.zero;
            chunkData.pathNodeList.Clear();

            RefreshGridVisual();

            UpdateLevelData();

            pathCountPerChunkDict[chunkData] = chunkData.pathNodeList.Count;
        });

        nextChunkButton.onClick.AddListener(() => {

            if (addChunkButton.gameObject.activeSelf) {
                // Reach last chunk had in levelSO
                return;
            }

            currentChunkIndex += 1;

            ChunkData chunk = null;

            if (currentChunkIndex <= levelSO.chunkList.Count - 1) {
                // This level having this index before
                chunk = levelSO.chunkList[currentChunkIndex];
            }

            SwitchChunk(chunk);
        });

        backChunkButton.onClick.AddListener(() => {

            if (currentChunkIndex == 0) {
                return;
            }

            currentChunkIndex -= 1;

            ChunkData chunk = levelSO.chunkList[currentChunkIndex];

            SwitchChunk(chunk);
        });

        addChunkButton.onClick.AddListener(() => {

            ChunkData newChunk = CreateNewChunk();

            levelSO.chunkList.Add(newChunk);

            pathCountPerChunkDict.Add(newChunk, newChunk.pathNodeList.Count);

            UpdateLevelData();
            UpdateInputField(newChunk);

            SwitchChunk(levelSO.chunkList[currentChunkIndex]);
        });

        deleteChunkButton.onClick.AddListener(() => {

            if (levelSO.chunkList.Count == 0) return;

            ChunkData chunkToDelete = levelSO.chunkList[currentChunkIndex];
            if (pathCountPerChunkDict.ContainsKey(chunkToDelete)) {
                pathCountPerChunkDict.Remove(chunkToDelete);
            }

            levelSO.chunkList.RemoveAt(currentChunkIndex);

            if (levelSO.chunkList.Count == 0) {

                currentChunkIndex = 0;
                SwitchChunk(null);
            }
            else {

                if (currentChunkIndex >= levelSO.chunkList.Count) {
                    currentChunkIndex = levelSO.chunkList.Count - 1;
                }

                SwitchChunk(levelSO.chunkList[currentChunkIndex]);
            }

            UpdateLevelData();
        });

        gameModeButton.onClick.AddListener(() => {

            LevelManager.Instance.LoadLevel(levelSO);
            LevelManager.Instance.OnInit();
            LevelManager.Instance.OnPlay();

            Hide();
        });

        closeButton.onClick.AddListener(() => {

        });
        #endregion

        #region Input Field Listener
        chunkNameInputField.onEndEdit.AddListener(delegate {

            string chunkName = chunkNameInputField.text;

            if (currentChunkIndex > levelSO.chunkList.Count - 1) { return; }

            levelSO.chunkList[currentChunkIndex].chunkName = chunkName;

            UpdateLevelData();
        });

        widthInputField.onEndEdit.AddListener(delegate {OnSizeChunkChanged(); });
        heightInputField.onEndEdit.AddListener(delegate {OnSizeChunkChanged(); });

        bridgeCountInputField.onEndEdit.AddListener(delegate {

            if (bridgeCountInputField.text == "") { return; }

            ChunkData currentChunk = levelSO.chunkList[currentChunkIndex];
            int totalPathInChunk = pathCountPerChunkDict[currentChunk];

            int bridgeCount = int.Parse(bridgeCountInputField.text);
            //int clampCount = Mathf.Clamp(bridgeCount, totalPathInChunk, this.lastCachedTotal);
            levelSO.chunkList[currentChunkIndex].bridgeCount = bridgeCount;

            UpdateInputField(currentChunk);
            UpdateLevelData();
        });
        #endregion
    }

    private void Start() {

        OnInit();
    }

    private void Update() {
        
        if (levelSO == null) { return; }
        if (levelSO.chunkList.Count <= 0) { return; }


        // Total Path
        int currentTotal = GetTotalPathNodesInLevel();

        if (currentTotal != lastCachedTotal) {

            lastCachedTotal = currentTotal;
            totalPathCountText.text = $"Total Path: {lastCachedTotal}";
        }

        // Path per Chunk
        if (currentChunkIndex >= levelSO.chunkList.Count) { return; }

        ChunkData currentChunk = levelSO.chunkList[currentChunkIndex];
        int currentPathCount = pathCountPerChunkDict[currentChunk];

        if (currentPathCount != lastCachedPathPerChunk) {

            lastCachedPathPerChunk = currentPathCount;
            pathCountPerChunkText.text = $"Path in Chunk: {lastCachedPathPerChunk} node";
        }
    }

    private void OnInit() {

        if (levelSO.chunkList.Count == 0) {
            // If chunk is empty
            return;
        }

        // Init Dict to counting pathNode count per chunk
        pathCountPerChunkDict.Clear();
        foreach (ChunkData chunk in levelSO.chunkList) {
            if (!pathCountPerChunkDict.ContainsKey(chunk)) {

                pathCountPerChunkDict.Add(chunk, chunk.pathNodeList.Count);
            }
        }

        ChunkData chunk0 = levelSO.chunkList[currentChunkIndex];

        SwitchChunk(chunk0);
    }

    private void ClearChunkGrid() {

        foreach (NodeButton nodeButton in nodeButtonList) {

            Destroy(nodeButton.transform.gameObject);
        }

        nodeButtonList.Clear();
    }
    
    private void ClearInputField() {

        bridgeCountInputField.text = "";
        heightInputField.text = "";
        widthInputField.text = "";
        chunkNameInputField.text = "";

    }

    private void InitializeChunkGrid(ChunkData chunk) {

        // Setup ChunkGrid size
        GridLayoutGroup gridLayoutGroup = chunkGrid.GetComponent<GridLayoutGroup>();

        int gridWidth = chunk.chunkWidth;
        int gridHeight = chunk.chunkHeight;

        float cellSize = 100f;

        if (gridWidth <= 11 && gridHeight <= 8) {
            // Small chunk

            cellSize = 100f;
        }
        else if (gridWidth <= 15 && gridHeight <= 10) {
            // Medium chunk

            cellSize = 80f;
        }
        else if (gridWidth <= 20 && gridHeight <= 16) {
            // Big chunk
            cellSize = 55f;
        }
        else {
            // Huge chunk
            cellSize = 45f;
        }

        gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);

        chunkGrid.GetComponent<RectTransform>().sizeDelta = new Vector2(cellSize * gridWidth, cellSize * gridHeight);

        // Init grid
        for (int x = 0; x < gridWidth; x++) {   
            for (int y = 0; y < gridHeight; y++) {

                Vector2Int nodePos = new Vector2Int(x, y);
                NodeButton nodeButton = NodeButton.SpawnNodeButton(nodeButtonPrefab, chunkGrid, nodePos, this);

                nodeButton.gameObject.name = $"Node_{x}_{y}";

                nodeButtonList.Add(nodeButton);
            }
        }

        RefreshGridVisual();
    }

    public void HandleNodeInteraction(Vector2Int nodePos) {

        var keyboard = Keyboard.current;

        bool is1 = keyboard.digit1Key.isPressed;
        bool is2 = keyboard.digit2Key.isPressed;
        bool is3 = keyboard.digit3Key.isPressed;
        bool is4 = keyboard.digit4Key.isPressed;
        bool isLeftShift = keyboard.leftShiftKey.isPressed;

        ChunkData chunk = levelSO.chunkList[currentChunkIndex];
        bool hasChanged = false;

        PathNode currentNode = GetPathNodeAt(chunk, nodePos);

        if (is1) {
            // ---- SET PATH ----

            if (currentNode == null) {
                // If this node is not Path

                PathNode newNode = new PathNode { nodePos = nodePos, hasCornerOn = false };
                chunk.pathNodeList.Add(newNode);
                hasChanged = true;

                // Update pathNode count
                pathCountPerChunkDict[chunk] = chunk.pathNodeList.Count;
            }
        }
        else if (is2) {
            // ---- SET/UNSET CORNER ----

            if (currentNode != null) {
                // If this node is Path before

                currentNode.hasCornerOn = !currentNode.hasCornerOn;
                hasChanged = true;
            }

        }
        else if (is3) {
            // ---- SET/UNSET STARTNODE ----

            if (currentNode != null) {
                // If this node is Path before

                chunk.startNode.nodePos = nodePos;
                hasChanged = true;
            }
        }
        else if (is4) {
            // ---- SET/UNSET ENDNODE ----

            if (currentNode != null) {
                // If this node is Path before

                chunk.endNode.nodePos = nodePos;
                hasChanged = true;
            }
        }
        else if (isLeftShift) {
            // ---- REMOVE PATH NODE ----

            if (currentNode != null) {

                chunk.pathNodeList.Remove(currentNode);

                // Update pathNode count
                pathCountPerChunkDict[chunk] = chunk.pathNodeList.Count;

                if (chunk.startNode.nodePos == nodePos) {
                    // If this node is startNode --> Reset startNode

                    chunk.startNode.nodePos = Vector2Int.zero;
                }
                
                if (chunk.endNode.nodePos == nodePos) {
                    // Is this node is endNode ---> Reset endNode

                    chunk.endNode.nodePos = Vector2Int.zero;
                }

                hasChanged = true;
            }
        }

        if (hasChanged) {

            RefreshGridVisual();

            UpdateLevelData();
        }
    }

    public void RefreshGridVisual() {

        ChunkData chunk = levelSO.chunkList[currentChunkIndex];

        foreach (NodeButton nodeButton in nodeButtonList) {

            nodeButton.UpdateVisual(chunk, GetPathNodeAt(chunk, nodeButton.GetNodePos()));
        }
    }

    private PathNode GetPathNodeAt(ChunkData chunk, Vector2Int pos) {

        foreach (PathNode node in chunk.pathNodeList) {
            if (node.nodePos == pos) return node;
        }
        return null;
    }

    private void OnSizeChunkChanged() {

        if (levelSO.chunkList.Count == 0) { return; }

        if (widthInputField.text == "") { return; }
        if (heightInputField.text == "") { return; }

        ChunkData currentChunk = levelSO.chunkList[currentChunkIndex];

        int width = ValidateInput(int.Parse(widthInputField.text));
        int height = ValidateInput(int.Parse(heightInputField.text));

        // Update size in LevelSO
        currentChunk.chunkWidth = width;
        currentChunk.chunkHeight = height;

        // Update PathNodeDict
        pathCountPerChunkDict[currentChunk] = currentChunk.pathNodeList.Count;

        // Update size in Editor Scene
        SwitchChunk(currentChunk);
    }

    private void SwitchChunk(ChunkData chunk = null) {

        if (chunk != null) {

            HideAddChunk();

            ClearChunkGrid();
            InitializeChunkGrid(chunk);

            UpdateInputField(chunk);
        }
        else {

            ClearChunkGrid();
            ClearInputField();

            ShowAddChunk();
        }
    }

    private int ValidateInput(int input) {

        return Mathf.Clamp(input, minValue, maxValue);
    }

    private void UpdateInputField(ChunkData chunk) {

        // Update Chunk Name Text
        if (chunk.chunkName != "") {

            chunkNameInputField.text = $"{chunk.chunkName}";
        }
        else {
            chunkNameInputField.text = "";
            chunkNameInputField.placeholder.GetComponent<TextMeshProUGUI>().text = "Enter Name...";
        }        

        // Update Width Text
        if (chunk.chunkWidth != 0) {

            widthInputField.text = ValidateInput(chunk.chunkWidth).ToString();
        }
        else {
            widthInputField.text = "";
            widthInputField.placeholder.GetComponent<TextMeshProUGUI>().text = "Enter Width...";
        }

        // Update Height Text
        if (chunk.chunkHeight != 0) {

            heightInputField.text = chunk.chunkHeight.ToString();
        }
        else {
            heightInputField.text = "";
            heightInputField.placeholder.GetComponent<TextMeshProUGUI>().text = "Enter Height...";
        }
        
        // Update Bridge Count Text
        if (chunk.bridgeCount != 0) {

            bridgeCountInputField.text = $"{chunk.bridgeCount}";
        }
        else {
            bridgeCountInputField.text = "";
            bridgeCountInputField.placeholder.GetComponent<TextMeshProUGUI>().text = "Enter...";
        }
    }

    private int GetTotalPathNodesInLevel() {
        int total = 0;

        foreach (ChunkData chunk in levelSO.chunkList) {

            if (chunk.pathNodeList != null) {

                total += chunk.pathNodeList.Count;
            }
        }

        return total;
    }

    private ChunkData CreateNewChunk() {

        ChunkData newChunk = new ChunkData();

        newChunk.chunkName = "";
        newChunk.chunkWidth = 0;
        newChunk.chunkHeight = 0;
        newChunk.pathNodeList = new List<PathNode>();
        newChunk.startNode = new PathNode { nodePos = Vector2Int.zero, hasCornerOn = false };
        newChunk.endNode = new PathNode { nodePos = Vector2Int.zero, hasCornerOn = false };
        newChunk.bridgeCount = 0;

        return newChunk;
    }

    private void Show() {
        this.gameObject.SetActive(true);
    }

    private void Hide() {
        this.gameObject.SetActive(false);
    }

    private void ShowAddChunk() {
        
        addChunkButton.gameObject.SetActive(true);
        deleteChunkButton.gameObject.SetActive(false);
    }

    private void HideAddChunk() {

        addChunkButton.gameObject.SetActive(false);
        deleteChunkButton.gameObject.SetActive(true);
    }

    private void UpdateLevelData() {

    #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(levelSO);
        //UnityEditor.AssetDatabase.SaveAssets(); 
    #endif
    }

    public bool IsShow() {
        return this.gameObject.activeSelf;
    }
}
