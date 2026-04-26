using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NodeButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nodeText;

    private LevelEditorUI levelEditorUI;
    private Vector2Int nodePos;
    private ChunkData chunk;
    private Button button;
    private Image image;

    private void Awake() {

        button = GetComponent<Button>();
        image = GetComponent<Image>();

        button.onClick.AddListener(() => {
            levelEditorUI.HandleNodeInteraction(nodePos);
        });

    }

    public void UpdateVisual(ChunkData chunk, PathNode pathNode) {

        if (chunk.startNode.nodePos == nodePos) {

            SetVisual(Color.green, "S");
        }
        else if (chunk.endNode.nodePos == nodePos) {

            SetVisual(Color.red, "E");
        }
        else if (pathNode != null) {

            if (pathNode.hasCornerOn) {

                SetVisual(Color.yellow, "C");
            }
            else {
                SetVisual(Color.cyan, "");
            }
        }
        else {

            SetVisual(Color.gray, "");
        }
    }

    private void SetVisual(Color c, string t) {
        image.color = c;
        nodeText.text = t;
    }

    public Vector2Int GetNodePos() {
        return this.nodePos;
    }

    public static NodeButton SpawnNodeButton(Transform buttonPrefab, Transform parent, Vector2Int nodePos, LevelEditorUI levelEditorUI) {

        Transform nodeButtonTransform = Instantiate(buttonPrefab, parent);

        NodeButton nodeButton = nodeButtonTransform.GetComponent<NodeButton>();

        // Setup field
        nodeButton.nodePos = nodePos;
        nodeButton.levelEditorUI = levelEditorUI;

        return nodeButton;
    }
}
