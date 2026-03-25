using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager GetInstanceST() => _instance;

    [SerializeField] private AudioClipReferences clips;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    private const float DefaultFadeDuration = 1f;

    private void Awake()
    {
        _instance = this;
        GameManager.RegisterAudioManagerST(this);
    }

    private void Start()
    {
        GameManager.OnStateChanged += OnStateChanged;
    }

    private void OnDestroy()
    {
        GameManager.OnStateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameState state)
    {
        if (clips == null) return;
        switch (state)
        {
            case GameState.Overworld:
                PlayMusicST(clips.overworldMusic, true);
                break;
            case GameState.Battle:
                PlayMusicST(clips.battleMusic, false);
                break;
            case GameState.MainMenu:
                StopMusicST(true);
                break;
        }
    }

    public void PlayMusicST(AudioClip clip, bool fadeIn)
    {
        if (clip == null || musicSource == null) return;
        StartCoroutine(PlayMusicRoutine(clip, fadeIn));
    }

    public void PlaySFXST(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void StopMusicST(bool fadeOut)
    {
        if (musicSource == null) return;
        if (fadeOut)
            StartCoroutine(FadeMusicRoutine(musicSource.volume, 0f, DefaultFadeDuration, true));
        else
            musicSource.Stop();
    }

    public void SetMusicVolumeST(float volume)
    {
        if (musicSource) musicSource.volume = volume;
    }

    public void SetSFXVolumeST(float volume)
    {
        if (sfxSource) sfxSource.volume = volume;
    }

    public SettingsSaveData GetSettingsSaveDataST()
    {
        return new SettingsSaveData
        {
            musicVolume = musicSource != null ? musicSource.volume : 1f,
            sfxVolume   = sfxSource   != null ? sfxSource.volume   : 1f
        };
    }

    public void ApplySettingsSaveDataST(SettingsSaveData settings)
    {
        if (settings == null) return;
        SetMusicVolumeST(settings.musicVolume);
        SetSFXVolumeST(settings.sfxVolume);
    }

    private IEnumerator PlayMusicRoutine(AudioClip clip, bool fadeIn)
    {
        float targetVolume = musicSource.volume > 0 ? musicSource.volume : 1f;
        if (musicSource.isPlaying)
            yield return FadeMusicRoutine(musicSource.volume, 0f, DefaultFadeDuration * 0.5f, false);

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.volume = fadeIn ? 0f : targetVolume;
        musicSource.Play();

        if (fadeIn)
            yield return FadeMusicRoutine(0f, targetVolume, DefaultFadeDuration, false);
    }

    private IEnumerator FadeMusicRoutine(float from, float to, float duration, bool stopAfter)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        musicSource.volume = to;
        if (stopAfter) musicSource.Stop();
    }
}
