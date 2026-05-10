using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioClipRefSO audioClipRefSO;


    private float sfxVolume;

    public void Awake() {

        Instance = this;

        this.sfxVolume = DataManager.GetSFXData().sfxVolume;

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

    public void ToggleSFXVolume() {

        bool isMutedSFX = DataManager.GetSFXData().isMutedSFX;

        if (!isMutedSFX) {
            this.sfxVolume = 0f;
        }
        else {
            this.sfxVolume = 1f;
        }

        DataManager.SetSFXData(this.sfxVolume, !isMutedSFX);
    }
}
