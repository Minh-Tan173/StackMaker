using TMPro;
using UnityEngine;

public class GamePlayUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI levelText;

    private void Start() {
        LevelManager.Instance.InitObjectData += LevelManager_InitObjectData;
    }

    private void LevelManager_InitObjectData(object sender, System.EventArgs e) {

        UpdateVisual();
    }

    private void UpdateVisual() {

        int currentLevel = LevelManager.Instance.GetCurrentLevelIndex() + 1;
        levelText.text = $"Level {currentLevel}";
    }
}
