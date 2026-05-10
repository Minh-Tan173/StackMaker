using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    private AudioSource audioSource;

    private float musicVolume;

    private void Awake() {

        Instance = this;

        audioSource = GetComponent<AudioSource>();

        SetSFXVolume(DataManager.GetMusicData().musicVolume);

    }

    private void SetSFXVolume(float volume) {

        this.musicVolume = volume;
        audioSource.volume = this.musicVolume;
    }

    public void ToggleMusicVolume() {

        bool isMutedMusic = DataManager.GetMusicData().isMutedMusic;

        if (!isMutedMusic) {

            SetSFXVolume(0f);
        }
        else {

            SetSFXVolume(1f);
        }

        DataManager.SetMusicData(this.musicVolume, !isMutedMusic);
    }
}
