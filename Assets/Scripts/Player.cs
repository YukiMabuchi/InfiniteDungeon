using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    /*
    NOTE: 行動系 (eg 移動、攻撃)はGameState.Waitingの状態で行い、終了時にSetCurrentState(GameState.PlayerTurn)をする
    */

    public static Player instance;

    [SerializeField] int power = 4;
    // [SerializeField] int stamina; // TODO
    [SerializeField] float speed;
    LayerMask obstacleMask, enemyMask;
    Vector2 targetPos;
    Vector2 attackTargetPos;
    Transform GFX;
    PlayerLevel playerLevel;
    PlayerHealth playerHealth;

    int currentPower = 0;
    string currentDirection = "down";
    float flipx;
    bool isMoving;

    public int CurrentPower { get { return currentPower; } }
    public Vector2 TargetPos { get { return targetPos; } }

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        currentPower = power;
        obstacleMask = LayerMask.GetMask("Wall", "Enemy");
        enemyMask = LayerMask.GetMask("Enemy");
        GFX = GetComponentInChildren<SpriteRenderer>().transform;
        flipx = GFX.localScale.x;
        playerLevel = GetComponent<PlayerLevel>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    // 移動
    public void Move(string direction)
    {
        if (!isMoving && GameManager.instance.CurrentGameState == GameState.Waiting)
        {
            currentDirection = direction;

            // キャラの向き
            if (currentDirection == "left" || currentDirection == "right") GFX.localScale = new Vector2(flipx * GetDirection(currentDirection), GFX.localScale.y);

            // キャラの進行
            if (!isMoving && !IsHit(currentDirection, obstacleMask))
            {
                targetPos = GenerateTargetPos(currentDirection); // NOTE: 代入必要
                StartCoroutine(SmoothMove(targetPos));
            }
            else return; // 壁などで移動できていない時GameStateを変更せずreturn

            GameManager.instance.SetCurrentState(GameState.PlayerTurn);
        }
    }

    IEnumerator SmoothMove(Vector2 posToMove)
    {
        isMoving = true;
        while (Vector2.Distance(transform.position, posToMove) > 0.01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, posToMove, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = posToMove;

        // HP回復
        playerHealth.IncreaseHealthByPlayerMovement();

        isMoving = false;
    }

    Vector2 GenerateTargetPos(string direction)
    {
        if (direction == "relocate") return new Vector2(0, 0);

        Vector2 tempTargetPos = new Vector2(0, 0);
        int directionNum = GetDirection(direction);

        // 進行先の設定 (1マス先)
        if (direction == "left" || direction == "right")
        {
            tempTargetPos = new Vector2(transform.position.x + directionNum, transform.position.y);
        }
        else if (currentDirection == "up" || currentDirection == "down")
        {
            tempTargetPos = new Vector2(transform.position.x, transform.position.y + directionNum);
        }

        return tempTargetPos;
    }

    /// <summary>
    /// 指定方向で指定マスクに当たるかの判定
    /// </summary>
    bool IsHit(string direction, LayerMask targetMask)
    {
        Vector2 tempTargetPos = GenerateTargetPos(direction);

        // check for collisions
        Vector2 hitSize = Vector2.one * 0.8f;
        return Physics2D.OverlapBox(tempTargetPos, hitSize, 0, targetMask);
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
            playerHealth.IncreaseHealthByPlayerMovement();

            // 攻撃判定
            if (IsHit(currentDirection, enemyMask))
            {
                attackTargetPos = GenerateTargetPos(currentDirection); // NOTE: 代入必要
                GameObject floor = DungeonManager.instance.GetFloorByPos(attackTargetPos);
                if (floor == null) return;

                Enemy enemy = floor.GetComponentInChildren<Enemy>();
                if (enemy != null) enemy.TakeDamage(power);
            }
            GameManager.instance.SetCurrentState(GameState.PlayerTurn);
        }
    }

    public void GainXp(int xp)
    {
        playerLevel.AccumulateXpAndCheckLevelUp(xp);
    }

    // ダメージ
    public void TakeDamage(int damageToTake)
    {
        playerHealth.DecreaseHealth(damageToTake);
        if (playerHealth.CurrentHealth <= 0) GameManager.instance.ShowGameOverPopup();
    }

    // リスタート
    public void RelocatePlayer()
    {
        targetPos = GenerateTargetPos("relocate");
        transform.position = targetPos;
    }
}
