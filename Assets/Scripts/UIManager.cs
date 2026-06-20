using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private GameState gameState;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private LevelManager levelManager;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button collectionButton;

    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject newGameConfirmPanel;

    [Header("Settings Controls")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle screenShakeToggle;
    [SerializeField] private Toggle skipReadStoryToggle;
    [SerializeField] private Toggle fullscreenToggle;

    private void Start()
    {
        RefreshMenuState();
        RefreshSettingsControls();
        HidePanel(settingsPanel);
        HidePanel(creditsPanel);
        HidePanel(newGameConfirmPanel);
    }

    public void OnNewGameClicked()
    {
        PlayButtonClick();

        if (gameState != null && gameState.HasSaveData())
        {
            ShowPanel(newGameConfirmPanel);
            return;
        }

        StartNewGame();
    }

    public void OnContinueClicked()
    {
        PlayButtonClick();

        if (gameState == null || !gameState.HasSaveData())
        {
            RefreshMenuState();
            return;
        }

        gameState.ContinueGame();
        if (levelManager != null)
        {
            levelManager.LoadCurrentLevel();
        }
    }

    public void OnSettingsClicked()
    {
        PlayButtonClick();
        ShowPanel(settingsPanel);
        RefreshSettingsControls();
    }

    public void OnCreditsClicked()
    {
        PlayButtonClick();
        ShowPanel(creditsPanel);
    }

    public void OnQuitClicked()
    {
        PlayButtonClick();

#if UNITY_EDITOR
        Debug.Log("Quit requested. Application.Quit is skipped in the Unity Editor.");
#else
        Application.Quit();
#endif
    }

    public void OnCloseSettingsClicked()
    {
        PlayButtonClick();
        HidePanel(settingsPanel);
    }

    public void OnCloseCreditsClicked()
    {
        PlayButtonClick();
        HidePanel(creditsPanel);
    }

    public void OnConfirmNewGameClicked()
    {
        PlayButtonClick();
        HidePanel(newGameConfirmPanel);
        StartNewGame();
    }

    public void OnCancelNewGameClicked()
    {
        PlayButtonClick();
        HidePanel(newGameConfirmPanel);
    }

    public void OnMasterVolumeChanged(float value)
    {
        if (audioManager != null)
        {
            audioManager.SetMasterVolume(value);
        }
    }

    public void OnMusicVolumeChanged(float value)
    {
        if (audioManager != null)
        {
            audioManager.SetMusicVolume(value);
        }
    }

    public void OnSfxVolumeChanged(float value)
    {
        if (audioManager != null)
        {
            audioManager.SetSfxVolume(value);
        }
    }

    public void OnScreenShakeChanged(bool isEnabled)
    {
        if (gameState == null)
        {
            return;
        }

        gameState.screenShakeEnabled = isEnabled;
        gameState.SaveLocalState();
    }

    public void OnSkipReadStoryChanged(bool isEnabled)
    {
        if (gameState == null)
        {
            return;
        }

        gameState.skipReadStoryEnabled = isEnabled;
        gameState.SaveLocalState();
    }

    public void OnFullscreenChanged(bool isFullscreen)
    {
        if (gameState == null)
        {
            return;
        }

        gameState.isFullscreen = isFullscreen;
        Screen.fullScreen = isFullscreen;
        gameState.SaveLocalState();
    }

    private void StartNewGame()
    {
        if (gameState != null)
        {
            gameState.ClearProgress();
            gameState.NewGame();
        }

        RefreshMenuState();

        if (levelManager != null)
        {
            levelManager.LoadFirstLevel();
        }
    }

    private void RefreshMenuState()
    {
        if (continueButton != null)
        {
            continueButton.interactable = gameState != null && gameState.HasSaveData();
        }

        if (collectionButton != null)
        {
            collectionButton.interactable = false;
        }
    }

    private void RefreshSettingsControls()
    {
        if (gameState == null)
        {
            return;
        }

        SetSliderValueWithoutNotify(masterVolumeSlider, gameState.masterVolume);
        SetSliderValueWithoutNotify(musicVolumeSlider, gameState.musicVolume);
        SetSliderValueWithoutNotify(sfxVolumeSlider, gameState.sfxVolume);
        SetToggleValueWithoutNotify(screenShakeToggle, gameState.screenShakeEnabled);
        SetToggleValueWithoutNotify(skipReadStoryToggle, gameState.skipReadStoryEnabled);
        SetToggleValueWithoutNotify(fullscreenToggle, gameState.isFullscreen);
    }

    private void PlayButtonClick()
    {
        if (audioManager != null)
        {
            audioManager.PlayButtonClick();
        }
    }

    private static void ShowPanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    private static void HidePanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private static void SetSliderValueWithoutNotify(Slider slider, float value)
    {
        if (slider != null)
        {
            slider.SetValueWithoutNotify(value);
        }
    }

    private static void SetToggleValueWithoutNotify(Toggle toggle, bool value)
    {
        if (toggle != null)
        {
            toggle.SetIsOnWithoutNotify(value);
        }
    }
}
