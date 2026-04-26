using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class LevelEditorUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private LevelSO levelSO;

    [Header("Button")]
    [SerializeField] private Button resetChunkButton;
    [SerializeField] private Button nextChunkButton;
    [SerializeField] private Button backChunkButton;

    [Header("Grid Data")]
    [SerializeField] private Transform chunkGrid;
    [SerializeField] private Transform nodeButtonPrefab;

    [Header("Input Field Text")]
    [SerializeField] private TMP_InputField widthInputField;
    [SerializeField] private TMP_InputField heightInputField;
    [SerializeField] private int minValue;
    [SerializeField] private int maxValue;

    private int currentChunkIndex = 0; // Mặc định khi bắt đầu là chunk index 0

    private List<NodeButton> nodeButtonList;

    private void Awake() {

        nodeButtonList = new List<NodeButton>();

        OnInit();

        // Button
        resetChunkButton.onClick.AddListener(() => {

            ChunkData chunkData = levelSO.chunkList[currentChunkIndex];

            chunkData.startNode.nodePos = Vector2Int.zero;
            chunkData.endNode.nodePos = Vector2Int.zero;
            chunkData.pathNodeList.Clear();

            RefreshGridVisual();

        #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(levelSO);
            UnityEditor.AssetDatabase.SaveAssets();
        #endif
        });

        nextChunkButton.onClick.AddListener(() => {

        });

        backChunkButton.onClick.AddListener(() => {

        });

        // Input Field
        widthInputField.onEndEdit.AddListener(delegate {

            OnSizeChunkChanged();
        });

        heightInputField.onEndEdit.AddListener(delegate {

            OnSizeChunkChanged();
        });
    }

    private void OnInit() {

        if (levelSO.chunkList.Count == 0) {
            // If chunk is empty
            return;
        }

        ChunkData chunk0 = levelSO.chunkList[currentChunkIndex];

        ClearChunkGrid(chunk0);
        InitializeChunkGrid(chunk0);

        UpdateInputField(chunk0);
    }

    private void ClearChunkGrid(ChunkData chunk) {

        foreach (NodeButton nodeButton in nodeButtonList) {

            Destroy(nodeButton.transform.gameObject);
        }

        nodeButtonList.Clear();
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
        else {
            // Big chunk
            cellSize = 55f;
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
            // SET PATH

            if (currentNode == null) {
                // If this node is not Path

                PathNode newNode = new PathNode { nodePos = nodePos, hasCornerOn = false };
                chunk.pathNodeList.Add(newNode);
                hasChanged = true;
            }
        }
        else if (is2) {
            // SET/UNSET CORNER

            if (currentNode != null) {
                // If this node is Path before

                currentNode.hasCornerOn = !currentNode.hasCornerOn;
                hasChanged = true;
            }
        }
        else if (is3) {
            // SET/UNSET STARTNODE

            if (currentNode != null) {
                // If this node is Path before

                chunk.startNode.nodePos = nodePos;
                hasChanged = true;
            }
        }
        else if (is4) {
            // SET/UNSET ENDNODE

            if (currentNode != null) {
                // If this node is Path before

                chunk.endNode.nodePos = nodePos;
                hasChanged = true;
            }
        }
        else if (isLeftShift) {
            // REMOVE PATH NODE

            if (currentNode != null) {

                chunk.pathNodeList.Remove(currentNode);
                
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

        #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(levelSO);
            UnityEditor.AssetDatabase.SaveAssets();
        #endif
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

        // Update size in Editor Scene
        ClearChunkGrid(currentChunk);
        InitializeChunkGrid(currentChunk);

        UpdateInputField(currentChunk);
    }

    private int ValidateInput(int input) {

        return Mathf.Clamp(input, minValue, maxValue);
    }

    private void UpdateInputField(ChunkData chunk) {

        // Update Width Text
        if (chunk.chunkWidth != 0) {

            widthInputField.text = ValidateInput(chunk.chunkWidth).ToString();
        }
        else {
            widthInputField.placeholder.GetComponent<TextMeshProUGUI>().text = "Enter Width...";
        }

        // Update Height Text
        if (chunk.chunkHeight != 0) {

            heightInputField.text = chunk.chunkHeight.ToString();
        }
        else {
            heightInputField.placeholder.GetComponent<TextMeshProUGUI>().text = "Enter Height...";
        }
        
    }

}
