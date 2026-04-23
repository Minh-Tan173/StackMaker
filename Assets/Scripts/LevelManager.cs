using System;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public enum LevelState {
        StartGame,
        GameRunning,
        WinGame
    }

    public event EventHandler OnWinState;

    private LevelState currentLevelState;

    private void Awake() {

        Instance = this;
    }

    private void ChangeLevelStateTo(LevelState levelState) {
        this.currentLevelState = levelState;
    }

    private void LoadLevel(int levelIndex) {

    }

    private void OnInit() {

    }

    private void OnPlay() {

    } 

    public void OnWin() {

        ChangeLevelStateTo(LevelState.WinGame);

        OnWinState?.Invoke(this, EventArgs.Empty);
    }

    public LevelState GetCurrentLevelState() {
        return this.currentLevelState;
    }
}
