using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public enum LevelState {
        StartGame,
        GameRunning,
        WinGame
    }

    public static LevelManager Instance { get; private set; }

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
    }

    public LevelState GetCurrentLevelState() {
        return this.currentLevelState;
    }
}
