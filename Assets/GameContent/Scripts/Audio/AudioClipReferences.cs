using UnityEngine;

[CreateAssetMenu(fileName = "AudioClipReferences", menuName = "CoffinHill/Audio Clip References")]
public class AudioClipReferences : ScriptableObject
{
    [Header("Music")]
    public AudioClip overworldMusic;
    public AudioClip battleMusic;
    public AudioClip victoryJingle;
    public AudioClip encounterStinger;

    [Header("SFX")]
    public AudioClip confirmSFX;
    public AudioClip denySFX;
    public AudioClip hitSFX;
    public AudioClip critSFX;
    public AudioClip levelUpSFX;
    public AudioClip caughtSFX;
}
