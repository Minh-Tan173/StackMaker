using UnityEngine;

public static class DataManager
{

    public const string GAME_SAVED_DATA_KEY = "GameSavedData";

    private static GameDataSaved gameData;

    #region Level
    public static void SaveLevel(int levelIndex) {

        gameData = GetGameDataSaved();
        gameData.currentLevelIndex = levelIndex;

        SaveDataToPrefs();
    }

    public static int GetSavedLevel() {

        return GetGameDataSaved().currentLevelIndex;
    }
    #endregion

    #region SFX
    public static void SetSFXData(float sfxVolume, bool isMuted) {

        gameData = GetGameDataSaved();

        gameData.sfxDataSaved.sfxVolume = sfxVolume;
        gameData.sfxDataSaved.isMutedSFX = isMuted;

        SaveDataToPrefs();
    }

    public static SFXDataSaved GetSFXData() {

        return GetGameDataSaved().sfxDataSaved;
    }
    #endregion

    #region Music
    public static void SetMusicData(float musicVolume, bool isMuted) {

        gameData = GetGameDataSaved();

        gameData.musicDataSaved.musicVolume = musicVolume;
        gameData.musicDataSaved.isMutedMusic = isMuted;

        SaveDataToPrefs();
    }

    public static MusicDataSaved GetMusicData() {

        return GetGameDataSaved().musicDataSaved;
    }
    #endregion


    private static GameDataSaved GetGameDataSaved() {

        if (gameData != null) { return gameData; }

        string jsonText = PlayerPrefs.GetString(GAME_SAVED_DATA_KEY, "");

        gameData = new GameDataSaved { currentLevelIndex = 0, sfxDataSaved = new SFXDataSaved(1f, false), musicDataSaved = new MusicDataSaved(1f, false) };

        if (!string.IsNullOrEmpty(jsonText)) {
            // Game Data is not empty

            JsonUtility.FromJsonOverwrite(jsonText, gameData);
        }

        return gameData;
    }

    private static void SaveDataToPrefs() {

        if (gameData == null) { return; }

        string jsonText = JsonUtility.ToJson(gameData);
        PlayerPrefs.SetString(GAME_SAVED_DATA_KEY, jsonText);
        PlayerPrefs.Save();
    }
}

[System.Serializable]
public class SFXDataSaved {

    public float sfxVolume;
    public bool isMutedSFX;

    public SFXDataSaved(float sfxVolume, bool isMutedSFX) {
        this.sfxVolume = sfxVolume;
        this.isMutedSFX = isMutedSFX;
    }
}

[System.Serializable]
public class MusicDataSaved {

    public float musicVolume;
    public bool isMutedMusic;

    public MusicDataSaved(float musicVolume, bool isMutedMusic) {

        this.musicVolume = musicVolume;
        this.isMutedMusic = isMutedMusic;
    }
}

[System.Serializable]
public class GameDataSaved {

    // Game
    public int currentLevelIndex;

    // Sound
    public SFXDataSaved sfxDataSaved;
    public MusicDataSaved musicDataSaved;

}
