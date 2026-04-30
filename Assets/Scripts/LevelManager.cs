using System;
using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public enum SceneType {
        GameScene,
        EditorScene
    };

    public enum LevelState {
        StartGame,
        GameRunning,
        WinGame
    }

    public event EventHandler OnWinState;
    public event EventHandler<LoadNewMapEventArgs> LoadNewMap;
    public class LoadNewMapEventArgs : EventArgs {
        public LevelSO levelSO;
    }
    public event EventHandler InitObjectData;
    public event EventHandler ClearObjectData;
    public event EventHandler OnGameSetting;
    public event EventHandler OffGameSetting;

    [Header("Scene Type")]
    [SerializeField] private SceneType sceneType;

    [Header("Level Data")]
    [SerializeField] private LevelManagerSO levelManagerSO;

    private const string CURRENT_LEVEL_KEY = "Current_Level";

    private int currentLevelIndex;
    private LevelState currentLevelState;

    private bool isToggleSettingUI = false;

    private void Awake() {

        Instance = this;

        this.currentLevelIndex = PlayerPrefs.GetInt(CURRENT_LEVEL_KEY, 0);
    }

    private void Start() {

        LoadLevel(this.currentLevelIndex);
        OnInit();
    }

    private IEnumerator WinningCoroutine() {

        ChangeLevelStateTo(LevelState.WinGame);

        OnWinState?.Invoke(this, EventArgs.Empty);

        float nextLevelTimer = 4f;
        yield return new WaitForSeconds(nextLevelTimer);

        // Start CloseTransition
        Transition.Instance.CloseTransition();

        while (Transition.Instance.IsRunningCoroutine()) {
            yield return null;
        }

        // After CloseTranstion - Reset Data and Prepared Data for next level

        this.currentLevelIndex += 1;
        PlayerPrefs.SetInt(CURRENT_LEVEL_KEY, currentLevelIndex);
        PlayerPrefs.Save();

        LoadLevel(currentLevelIndex);
        OnInit();

        yield return new WaitForSeconds(0.3f);

        // Start OpenTransition
        Transition.Instance.OpenTransition();

        while (Transition.Instance.IsRunningCoroutine()) {
            yield return null;
        }

        OnPlay();
    }

    private IEnumerator RetryCoroutine(Action actionCallBack) {

        OnDespawn();

        // Start CloseTransition
        Transition.Instance.CloseTransition();

        while (Transition.Instance.IsRunningCoroutine()) {
            yield return null;
        }

        // After CloseTransition
        LoadLevel(this.currentLevelIndex);
        OnInit();

        yield return new WaitForSeconds(0.3f);

        // Start OpenTransition
        Transition.Instance.OpenTransition();

        while (Transition.Instance.IsRunningCoroutine()) {
            yield return null;
        }

        actionCallBack?.Invoke();
    }

    private void ChangeLevelStateTo(LevelState levelState) {
        this.currentLevelState = levelState;
    }

    public void OnInit() {

        ChangeLevelStateTo(LevelState.StartGame);

        InitObjectData?.Invoke(this, EventArgs.Empty);

    }

    private void OnDespawn() {

        ClearObjectData?.Invoke(this, EventArgs.Empty);
    }

    public void LoadLevel(int levelIndex) {

        this.currentLevelIndex = levelIndex;

        LoadNewMap?.Invoke(this, new LoadNewMapEventArgs { levelSO = levelManagerSO.levelSOList[this.currentLevelIndex] });
    }

    public void LoadLevel(LevelSO levelSO) {

        LoadNewMap?.Invoke(this, new LoadNewMapEventArgs { levelSO = levelSO });
    }

    public void OnPlay() {

        ChangeLevelStateTo(LevelState.GameRunning);
    }

    public void OnWin() {

        // Happen in Editor Scene
        if (IsEditorScene()) {

            ChangeLevelStateTo(LevelState.WinGame);
            OnWinState?.Invoke(this, EventArgs.Empty);

            return;
        }

        if (currentLevelIndex >= levelManagerSO.levelSOList.Count - 1) {
            // Reached last Index

            // Todo: Show WinUI
        }
        else {

            StartCoroutine(WinningCoroutine());
        }
    }

    public void OnRetry(Action actionCallBack) {

        StartCoroutine(RetryCoroutine(actionCallBack));
    }

    public LevelState GetCurrentLevelState() {
        return this.currentLevelState;
    }

    public bool IsEditorScene() {
        return this.sceneType == SceneType.EditorScene;
    }

    public int GetCurrentLevelIndex() {
        return this.currentLevelIndex;
    }

    public void ToggleGameSetting() {

        isToggleSettingUI = !isToggleSettingUI;

        if (isToggleSettingUI) {

            Time.timeScale = 0f;
            OnGameSetting?.Invoke(this, EventArgs.Empty);
        }
        else {

            Time.timeScale = 1f;
            OffGameSetting?.Invoke(this, EventArgs.Empty);
        }
    }
}
