using UnityEngine;

[CreateAssetMenu()]
public class AudioClipRefSO : ScriptableObject
{
    [Header("SFX")]
    public AudioClip addBrickSFX;
    public AudioClip removeBrickSFX;
    public AudioClip winSFX;
}
