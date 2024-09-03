using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] GameObject gameOverPopup;

    bool isGamePaused = false;
    public bool IsGamePaused { get { return isGamePaused; } }

    void Awake()
    {
        if (instance == null) instance = this;
        if (gameOverPopup != null) gameOverPopup.SetActive(false);
    }

    public void ShowGameOverPopup()
    {
        PauseGame();
        gameOverPopup.SetActive(true);
    }

    public void HideGameOverPopup()
    {
        ResumeGame();
        gameOverPopup.SetActive(false);
    }

    public void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1;
    }

    public void RestartGame()
    {
        HideGameOverPopup();
        ResumeGame();
        ReloadScene();
    }

    void ReloadScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}
