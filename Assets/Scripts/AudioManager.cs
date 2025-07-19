using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : SingletonBehaviour<AudioManager>
{
    private const string EFFECTS_VOLUME = "EffectsVolume";
    private const string MUSIC_VOLUME = "MusicVolume";

    [SerializeField] private AudioSource _mainMenuMusicAudioSource;
    [SerializeField] private AudioSource _ingameMusicAudioSource;
    [SerializeField] private AudioSource _levelSuccessAudioSource;
    [SerializeField] private AudioMixer _audioMixer;

    private InGameMenuUI _inGameMenuUI;
    private float _musicDB, _effectDB;

    protected override void Initialize()
    {
        dontDestroyOnload = true;
    }

    private void Start()
    {
        _inGameMenuUI = InGameMenuUI.Instance;
        SceneManager.sceneLoaded += OnSceneLoaded;
        AssignCallbacks();
        CheckScene();
        UpdateLevelsOnStart();
    }

    private void OnDestroy()
    {
        RemoveCallbacks();
    }

    private void AssignCallbacks()
    {
        _inGameMenuUI.MusicVolumeChanged += OnMusicLevelChange;
        _inGameMenuUI.EffectsSoundVolumeChanged += OnEffetsLevelChange;
    }

    private void RemoveCallbacks()
    {
        if (_inGameMenuUI != null)
        {
            _inGameMenuUI.MusicVolumeChanged -= OnMusicLevelChange;
            _inGameMenuUI.EffectsSoundVolumeChanged -= OnEffetsLevelChange;
        }
    }

    private void CheckScene()
    {
        if (_inGameMenuUI.OnMainMenu)
        {
            _mainMenuMusicAudioSource.Play();
            _ingameMusicAudioSource.Stop();
        }
        else
        {
            _mainMenuMusicAudioSource.Stop();
            _ingameMusicAudioSource.Play();
        }
    }

    private void UpdateLevelsOnStart()
    {
        OnMusicLevelChange(_inGameMenuUI.MusicVolume);
        OnEffetsLevelChange(_inGameMenuUI.EffectsSoundVolume);
    }

    private void OnMusicLevelChange(float level)
    {
        _musicDB = level > 0.0001f ? 20f * Mathf.Log10(level) : -80f;
        _audioMixer.SetFloat(MUSIC_VOLUME, _musicDB);
    }

    private void OnEffetsLevelChange(float level)
    {
        _effectDB = level > 0.0001f ? 20f * Mathf.Log10(level) : -80f;
        _audioMixer.SetFloat(EFFECTS_VOLUME, _effectDB);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckScene();
    }

    public void PlaySuccessSound()
    {
        _levelSuccessAudioSource.Play();
    }
}
