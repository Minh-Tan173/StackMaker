using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    private const string MUSIC_VOLUME_KEY = "MusicVolume";

    private AudioSource audioSource;

    private float musicVolume;
    private bool isMutedMusic;

    private void Awake() {

        Instance = this;

        audioSource = GetComponent<AudioSource>();

        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
        audioSource.volume = musicVolume;

        if (Mathf.Approximately(musicVolume, 0f)) {

            isMutedMusic = true;
        }
        else {
            isMutedMusic = false;
        }
    }

    private void SetVolume(float volume) {

        this.musicVolume = volume;
        audioSource.volume = musicVolume;

        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volume);
        PlayerPrefs.Save();
    }

    public void ToggleMusicVolume() {

        isMutedMusic = !isMutedMusic;

        if (isMutedMusic) {

            SetVolume(0f);
        }
        else {
            SetVolume(1f);
        }
    }

    public bool IsMutedMusic() {
        return this.isMutedMusic;
    }
}
