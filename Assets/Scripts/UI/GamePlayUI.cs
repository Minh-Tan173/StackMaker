using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Button")]
    [SerializeField] private Button settingButton;

    [Header("UI")]
    [SerializeField] private SettingUI settingUI;

    private void Awake() {

        settingButton.onClick.AddListener(() => {

            if (settingUI.IsRunningCoroutine()) { return; }

            LevelManager.Instance.ToggleGameSetting();
        });
    }

    private void Start() {
        LevelManager.Instance.InitObjectData += LevelManager_InitObjectData;
    }

    private void OnDestroy() {

        LevelManager.Instance.InitObjectData -= LevelManager_InitObjectData;
    }

    private void LevelManager_InitObjectData(object sender, System.EventArgs e) {

        UpdateVisual();
    }

    private void UpdateVisual() {

        int currentLevel = LevelManager.Instance.GetCurrentLevelIndex() + 1;
        levelText.text = $"Level {currentLevel}";
    }
}
