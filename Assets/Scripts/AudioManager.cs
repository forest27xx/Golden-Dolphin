using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private GameState gameState;
    [SerializeField] private AudioSource buttonClickSource;

    private void Start()
    {
        if (gameState != null)
        {
            ApplyMasterVolume(gameState.masterVolume);
        }
    }

    public void SetMasterVolume(float value)
    {
        if (gameState != null)
        {
            gameState.masterVolume = Mathf.Clamp01(value);
            gameState.SaveLocalState();
            ApplyMasterVolume(gameState.masterVolume);
            return;
        }

        ApplyMasterVolume(value);
    }

    public void SetMusicVolume(float value)
    {
        if (gameState == null)
        {
            return;
        }

        gameState.musicVolume = Mathf.Clamp01(value);
        gameState.SaveLocalState();
    }

    public void SetSfxVolume(float value)
    {
        if (gameState == null)
        {
            return;
        }

        gameState.sfxVolume = Mathf.Clamp01(value);
        gameState.SaveLocalState();
    }

    public void PlayButtonClick()
    {
        if (buttonClickSource == null || buttonClickSource.clip == null)
        {
            return;
        }

        buttonClickSource.PlayOneShot(buttonClickSource.clip);
    }

    private static void ApplyMasterVolume(float value)
    {
        AudioListener.volume = Mathf.Clamp01(value);
    }
}
