using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(LevelSO))]
public class LevelSOEditor : Editor {

    public override void OnInspectorGUI() {

        base.OnInspectorGUI();

        LevelSO levelSO = (LevelSO)target;
        if (levelSO.chunkList == null || levelSO.chunkList.Count == 0) return;

        GUILayout.Space(20);

        // Label 
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel) {
            fontSize = 14,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.yellow }
        };

        GUILayout.Label("--- GRID LEVEL EDITOR TOOL ---", headerStyle);

        GUILayout.Space(10);

        // Chunk grid visulizes
        for (int i = 0; i < levelSO.chunkList.Count; i++) {

            ChunkData chunk = levelSO.chunkList[i];
            EditorGUILayout.BeginVertical("helpbox");

            GUILayout.Label($"Index {i}: {levelSO.chunkList[i].chunkName}", EditorStyles.boldLabel);

            DrawChunkGrid(chunk);

            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
        }

        if (GUI.changed) {

            EditorUtility.SetDirty(levelSO);
        }
    }

    private void DrawChunkGrid(ChunkData chunk) {

        if (chunk.chunkWidth <= 0 || chunk.chunkHeight <= 0) {

            EditorGUILayout.HelpBox("Please Enter chunkWidth and chunkHeight", MessageType.Warning);
            return;
        }

        // Creating Grid Map
        Dictionary<Vector2Int, PathNode> nodeMap = new Dictionary<Vector2Int, PathNode>();
        if (chunk.pathNodeList == null) {

            chunk.pathNodeList = new List<PathNode>();
        }

        foreach (var node in chunk.pathNodeList) {

            if (!nodeMap.ContainsKey(node.nodePos))
                nodeMap.Add(node.nodePos, node);
        }

        for (int y = chunk.chunkHeight - 1; y >= 0; y--) {

            EditorGUILayout.BeginHorizontal();

            for (int x = 0; x < chunk.chunkWidth; x++) {

                Vector2Int pos = new Vector2Int(x, y);
                bool isPath = nodeMap.ContainsKey(pos);
                bool isStart = (chunk.startNode != null && chunk.startNode.nodePos == pos);
                bool isEnd = (chunk.endNode != null && chunk.endNode.nodePos == pos);
                bool isCorner = isPath && nodeMap[pos].hasCornerOn;


                string label = "";

                if (isStart) {

                    GUI.color = Color.green;
                    label = "S";
                }
                else if (isEnd) {

                    GUI.color = Color.red;
                    label = "E";
                }
                else if (isCorner) {

                    GUI.color = Color.blue;
                    label = "C";
                }
                else if (isPath) {

                    GUI.color = Color.cyan;
                    label = "";
                }
                else {
                    GUI.color = Color.white;
                    label = "";
                }

                if (GUILayout.Button(label, GUILayout.Width(30), GUILayout.Height(30))) {

                    HandleGridInteraction(chunk, pos, isPath);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        GUI.color = Color.white;

        // Chú thích hướng dẫn
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Guide:", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField("- Click: Add new Path node (Color: Cyan)", EditorStyles.miniLabel);
        EditorGUILayout.LabelField("- Alt + Click: Set hasCorner ON or Off (Color: Blue for On and Default for Off)", EditorStyles.miniLabel);
        EditorGUILayout.LabelField("- Shift + Click: Set StartNode (Color: Green)", EditorStyles.miniLabel);
        EditorGUILayout.LabelField("- Ctrl + Click: Set EndNode (Color: Red)", EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();

        GUILayout.Space(5f);

        if (GUILayout.Button("Reset Chunk Data")) {

            chunk.startNode.nodePos = Vector2Int.zero;
            chunk.endNode.nodePos = Vector2Int.zero;
            chunk.pathNodeList.Clear();
        }
    }

    private void HandleGridInteraction(ChunkData chunk, Vector2Int pos, bool isPath) {

        Event e = Event.current;

        if (e.shift) {
            chunk.startNode = new PathNode { nodePos = pos, hasCornerOn = false };
        }
        else if (e.control || e.command) {
            chunk.endNode = new PathNode { nodePos = pos, hasCornerOn = false };
        }
        else if (e.alt) {
            if (isPath) {
                var node = chunk.pathNodeList.Find(n => n.nodePos == pos);
                node.hasCornerOn = !node.hasCornerOn;
            }
        }
        else {
            if (isPath)
                chunk.pathNodeList.RemoveAll(n => n.nodePos == pos);
            else
                chunk.pathNodeList.Add(new PathNode { nodePos = pos, hasCornerOn = false });
        }
    }
}