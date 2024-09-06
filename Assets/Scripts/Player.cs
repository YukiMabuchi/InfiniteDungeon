using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player : MonoBehaviour
{
    /*
    NOTE: 行動系 (eg 移動、攻撃)はGameState.Waitingの状態で行い、終了時にSetCurrentState(GameState.PlayerTurn)をする
    */

    public static Player instance;

    [SerializeField] int maxHealth = 12;
    [SerializeField] int power = 4;
    // [SerializeField] int stamina; // TODO
    [SerializeField] float speed;
    [SerializeField] Slider healthBar;
    [SerializeField] TextMeshProUGUI healthCount;
    LayerMask obstacleMask, enemyMask;
    Vector2 targetPos;
    Transform GFX;

    int currentHealth = 0;
    int currentPower = 0;
    string currentDirection = "down";
    float flipx;
    bool isMoving;

    /// <summary>
    /// HP回復用Playerの動きの回数。回復するたびに0にする。
    /// </summary>
    int countForHealth = 0;

    public int CurrentPower { get { return currentPower; } }
    public Vector2 TargetPos { get { return targetPos; } }

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        currentHealth = maxHealth;
        currentPower = power;
        healthBar.maxValue = currentHealth;
        healthBar.value = currentHealth;
        UpdateHealthCount();
        obstacleMask = LayerMask.GetMask("Wall", "Enemy");
        enemyMask = LayerMask.GetMask("Enemy");
        GFX = GetComponentInChildren<SpriteRenderer>().transform;
        flipx = GFX.localScale.x;
    }

    // 移動
    public void Move(string direction)
    {
        if (GameManager.instance.CurrentGameState == GameState.Waiting)
        {
            currentDirection = direction;

            // キャラの向き
            if (currentDirection == "left" || currentDirection == "right") GFX.localScale = new Vector2(flipx * GetDirection(currentDirection), GFX.localScale.y);

            // キャラの進行
            if (!isMoving && !IsHit(currentDirection, obstacleMask)) StartCoroutine(SmoothMove());
            else return; // 壁などで移動できていない時GameStateを変更せずreturn

            GameManager.instance.SetCurrentState(GameState.PlayerTurn);
        }
    }

    IEnumerator SmoothMove()
    {
        isMoving = true;
        while (Vector2.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        // HP回復
        IncreaseHealthByPlayerMovement();

        isMoving = false;
    }

    /// <summary>
    /// 指定方向で指定マスクに当たるかの判定
    /// </summary>
    bool IsHit(string direction, LayerMask targetMask)
    {
        int directionNum = GetDirection(direction);

        // 進行先の設定 (1マス先)
        if (direction == "left" || direction == "right")
        {
            targetPos = new Vector2(transform.position.x + directionNum, transform.position.y);
        }
        else if (currentDirection == "up" || currentDirection == "down")
        {
            targetPos = new Vector2(transform.position.x, transform.position.y + directionNum);
        }

        // check for collisions
        Vector2 hitSize = Vector2.one * 0.8f;
        return Physics2D.OverlapBox(targetPos, hitSize, 0, targetMask);
    }

    int GetDirection(string direction)
    {
        Dictionary<string, int> directions = new Dictionary<string, int>
        {
            {"up", 1},
            {"right", 1},
            {"down", -1},
            {"left", -1},
        };
        return directions[direction];
    }

    // 攻撃
    public void Attack()
    {
        if (GameManager.instance.CurrentGameState == GameState.Waiting)
        {
            // HP回復
            IncreaseHealthByPlayerMovement();

            // 攻撃判定
            if (IsHit(currentDirection, enemyMask))
            {
                GameObject floor = DungeonManager.instance.GetFloorByPos(targetPos);
                if (floor == null) return;

                Enemy enemy = floor.GetComponentInChildren<Enemy>();
                if (enemy != null) enemy.TakeDamage(power);
            }
            GameManager.instance.SetCurrentState(GameState.PlayerTurn);
        }
    }

    // ダメージ
    public void TakeDamage(int damageToTake)
    {
        DecreaseHealth(damageToTake);
        if (currentHealth <= 0) GameManager.instance.ShowGameOverPopup();
    }

    void IncreaseHealth(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        healthBar.value = currentHealth;
        UpdateHealthCount();
    }

    void DecreaseHealth(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;
        healthBar.value = currentHealth;
        UpdateHealthCount();
    }

    void UpdateHealthCount()
    {
        healthCount.text = maxHealth + " / " + currentHealth;
    }

    /// <summary>
    /// Playerの行動によりHPを回復する
    /// </summary>
    public void IncreaseHealthByPlayerMovement()
    {
        countForHealth++;
        if (countForHealth == 2)
        {
            IncreaseHealth(1); // TODO: 体力回復の係数調整
            countForHealth = 0;
        }
    }

    // リスタート
    public void RelocatePlayer()
    {
        targetPos = new Vector2(0, 0);
        transform.position = targetPos;
    }
}
