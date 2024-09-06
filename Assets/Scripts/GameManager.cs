using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// 基本はWaiting
/// Playerの行動はWaiting時のみ行う
/// 終わる際にCurrentGameStateでPlayerTurnに変更
/// Enemyの行動はこのスクリプトから呼び出す
/// 終わる際にWaitingに変更
/// </summary>
public enum GameState
{
    Waiting,
    PlayerTurn,
    EnemyTurn
};

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] GameObject gameOverPopup;

    GameState currentGameState; // 現在のゲーム状態
    float turnDelay = .1f; // 移動ごとの間隔

    bool isGamePaused = false;

    public GameState CurrentGameState { get { return currentGameState; } }
    public float TurnDelay { get { return turnDelay; } }
    public bool IsGamePaused { get { return isGamePaused; } }

    void Awake()
    {
        if (instance == null) instance = this;
        if (gameOverPopup != null) gameOverPopup.SetActive(false);
        currentGameState = GameState.Waiting;
    }

    public void SetCurrentState(GameState state)
    {
        currentGameState = state;
        OnGameStateChanged(state);
    }

    void OnGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Waiting:
                DungeonManager.instance.ClearAllEnemiesTargetPos();
                break;

            case GameState.PlayerTurn:
                StartCoroutine(PlayerTurn());
                break;

            case GameState.EnemyTurn:
                StartCoroutine(EnemyTurn());
                break;
        }
    }

    IEnumerator PlayerTurn()
    {
        yield return new WaitForSeconds(turnDelay);
        SetCurrentState(GameState.EnemyTurn);
    }

    IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(turnDelay);

        foreach (Enemy enemy in DungeonManager.instance.Enemies)
        {
            StartCoroutine(enemy.Movement());
        }

        SetCurrentState(GameState.Waiting);
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
