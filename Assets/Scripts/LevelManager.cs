using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameState gameState;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string firstLevelSceneName = "Level_01";

    public void LoadFirstLevel()
    {
        LoadScene(firstLevelSceneName);
    }

    public void LoadCurrentLevel()
    {
        if (gameState == null)
        {
            LoadFirstLevel();
            return;
        }

        string sceneName = "Level_" + gameState.currentLevelIndex.ToString("00");
        LoadScene(sceneName);
    }

    public void LoadMainMenu()
    {
        LoadScene(mainMenuSceneName);
    }

    private static void LoadScene(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        Debug.LogWarning($"Scene '{sceneName}' is not available. Add it to Build Settings after creating it.");
    }
}
