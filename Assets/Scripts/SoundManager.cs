using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioClipRefSO audioClipRefSO;

    private const string SFX_VOLUME_KEY = "SfxVolume";

    private float sfxVolume;
    private bool isMutedSFX = false;

    public void Awake() {

        Instance = this;

        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);

        if (Mathf.Approximately(sfxVolume, 0f)) {
            isMutedSFX = true;
        }
        else {
            isMutedSFX = false;
        }
    }

    private void Start() {

        Player.OnAddBrickSFX += Player_OnAddBrickSFX;
        Player.OnRemoveBrickSFX += Player_OnRemoveBrickSFX;
        WinPos.OnWinSFX += WinPos_OnWinSFX;
    }

    private void OnDestroy() {

        Player.OnAddBrickSFX -= Player_OnAddBrickSFX;
        Player.OnRemoveBrickSFX -= Player_OnRemoveBrickSFX;
        WinPos.OnWinSFX -= WinPos_OnWinSFX;
    }

    private void WinPos_OnWinSFX(object sender, System.EventArgs e) {

        WinPos winPos = sender as WinPos;
        PlaySound(winPos.transform.position, audioClipRefSO.winSFX);
    }

    private void Player_OnRemoveBrickSFX(object sender, System.EventArgs e) {

        Player player = sender as Player;
        PlaySound(player.transform.position, audioClipRefSO.removeBrickSFX);
    }

    private void Player_OnAddBrickSFX(object sender, System.EventArgs e) {
        
        Player player = sender as Player;
        PlaySound(player.transform.position, audioClipRefSO.addBrickSFX);
    }

    private void PlaySound(Vector3 position, AudioClip audioClip) {

        AudioSource.PlayClipAtPoint(audioClip, position, this.sfxVolume);
    }

    private void SaveVolume(float volume) {

        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
        PlayerPrefs.Save();
    }

    public void ToggleSFXVolume() {

        isMutedSFX = !isMutedSFX;

        if (isMutedSFX) {

            this.sfxVolume = 0f;
            SaveVolume(sfxVolume);
        }
        else {
            this.sfxVolume = 1f;
            SaveVolume(sfxVolume);
        }
    }

    public bool IsMutedSFX() {
        return this.isMutedSFX;
    }
}
