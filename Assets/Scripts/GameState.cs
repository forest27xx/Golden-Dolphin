using UnityEngine;

public class GameState : MonoBehaviour
{
    private const string SaveExistsKey = "SaveExists";
    private const string CurrentLevelIndexKey = "CurrentLevelIndex";
    private const string UnlockedLevelIndexKey = "UnlockedLevelIndex";
    private const string TotalFragmentsKey = "TotalFragments";
    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";
    private const string ScreenShakeEnabledKey = "ScreenShakeEnabled";
    private const string SkipReadStoryEnabledKey = "SkipReadStoryEnabled";
    private const string IsFullscreenKey = "IsFullscreen";

    [Header("Progress")]
    public int currentLevelIndex = 1;
    public int unlockedLevelIndex = 1;
    public int totalFragments;

    [Header("Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    public bool screenShakeEnabled = true;
    public bool skipReadStoryEnabled;
    public bool isFullscreen = true;

    private void Awake()
    {
        LoadLocalState();
    }

    public bool HasSaveData()
    {
        return PlayerPrefs.GetInt(SaveExistsKey, 0) == 1;
    }

    public void NewGame()
    {
        currentLevelIndex = 1;
        unlockedLevelIndex = 1;
        totalFragments = 0;
        SaveLocalState();
    }

    public void ContinueGame()
    {
        LoadLocalState();
    }

    public void ClearProgress()
    {
        PlayerPrefs.DeleteKey(SaveExistsKey);
        PlayerPrefs.DeleteKey(CurrentLevelIndexKey);
        PlayerPrefs.DeleteKey(UnlockedLevelIndexKey);
        PlayerPrefs.DeleteKey(TotalFragmentsKey);
        PlayerPrefs.Save();

        currentLevelIndex = 1;
        unlockedLevelIndex = 1;
        totalFragments = 0;
    }

    public void LoadLocalState()
    {
        currentLevelIndex = PlayerPrefs.GetInt(CurrentLevelIndexKey, 1);
        unlockedLevelIndex = PlayerPrefs.GetInt(UnlockedLevelIndexKey, 1);
        totalFragments = PlayerPrefs.GetInt(TotalFragmentsKey, 0);

        masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
        screenShakeEnabled = PlayerPrefs.GetInt(ScreenShakeEnabledKey, 1) == 1;
        skipReadStoryEnabled = PlayerPrefs.GetInt(SkipReadStoryEnabledKey, 0) == 1;
        isFullscreen = PlayerPrefs.GetInt(IsFullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
    }

    public void SaveLocalState()
    {
        PlayerPrefs.SetInt(SaveExistsKey, 1);
        PlayerPrefs.SetInt(CurrentLevelIndexKey, currentLevelIndex);
        PlayerPrefs.SetInt(UnlockedLevelIndexKey, unlockedLevelIndex);
        PlayerPrefs.SetInt(TotalFragmentsKey, totalFragments);

        PlayerPrefs.SetFloat(MasterVolumeKey, masterVolume);
        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
        PlayerPrefs.SetInt(ScreenShakeEnabledKey, screenShakeEnabled ? 1 : 0);
        PlayerPrefs.SetInt(SkipReadStoryEnabledKey, skipReadStoryEnabled ? 1 : 0);
        PlayerPrefs.SetInt(IsFullscreenKey, isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }
}
