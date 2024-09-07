using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    /*
    NOTE: 自分の下のFloorが親コンポーネントになっている
    */

    [SerializeField] int health = 10;
    [SerializeField] int power = 4;
    [SerializeField] float alertRange; // 追いかけるようになるレンジ
    [Tooltip("1マスは1.1fが安全")]
    [SerializeField] float attackRange = 1.1f;
    [Tooltip("1マスは1.1fが安全")]
    [SerializeField] float takeDamageRange = 1.1f;

    Player player;
    EnemyXp enemyXp;
    Vector2 curPos;
    LayerMask obstacleMask, unwalkableMask;
    List<Vector2> availableMovementList = new List<Vector2>();
    List<Node> nodesList = new List<Node>();
    bool isMoving;
    int currentHealth = 0;
    int attackPercentage = 70;

    void Start()
    {
        player = FindObjectOfType<Player>();
        enemyXp = GetComponent<EnemyXp>();
        obstacleMask = LayerMask.GetMask("Wall", "Enemy", "Player");
        unwalkableMask = LayerMask.GetMask("Wall", "Enemy");
        curPos = transform.position;
        currentHealth = health;
    }

    /// <summary>
    /// 自由に歩く関数
    /// </summary>
    void Patrol()
    {
        // 進行可能マスの生成
        availableMovementList.Clear();
        Vector2 hitSize = Vector2.one * .8f;
        if (!IsHit("up", curPos, hitSize, obstacleMask)) availableMovementList.Add(Vector2.up);
        if (!IsHit("right", curPos, hitSize, obstacleMask)) availableMovementList.Add(Vector2.right);
        if (!IsHit("down", curPos, hitSize, obstacleMask)) availableMovementList.Add(Vector2.down);
        if (!IsHit("left", curPos, hitSize, obstacleMask)) availableMovementList.Add(Vector2.left);

        // 進行方向を進行可能なマスからランダムに1マス選ぶ
        if (availableMovementList.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableMovementList.Count);
            curPos += availableMovementList[randomIndex];
        }

        StartCoroutine(SmoothMove());
    }

    /// <summary>
    /// 確認対象のマス目がunwalableMaskではないか確認し、そうではない場合nodeListに追加する
    /// </summary>
    /// <param name="checkPoint"></param>
    /// <param name="parent"></param>
    void CheckNode(Vector2 checkPoint, Vector2 parent)
    {
        Vector2 hitSize = Vector2.one * .5f;
        Collider2D hit = Physics2D.OverlapBox(checkPoint, hitSize, 0, unwalkableMask);
        if (!hit) nodesList.Add(new Node(checkPoint, parent));
    }

    Vector2 FindNextStep(Vector2 startPos, Vector2 targetPos, float distanceToPlayer)
    {
        Vector2 myPos = startPos;
        nodesList.Clear();

        void _CheckSurroundingNodes()
        {
            // up, right, down, leftの移動可能なマスを追加
            CheckNode(myPos + Vector2.up, myPos);
            CheckNode(myPos + Vector2.right, myPos);
            CheckNode(myPos + Vector2.down, myPos);
            CheckNode(myPos + Vector2.left, myPos);
        }

        // もし追いかけてくる範囲外なら周り4マスをランダムに選択して終了
        if (distanceToPlayer > alertRange)
        {
            _CheckSurroundingNodes();
            if (nodesList.Count > 0) myPos = nodesList[UnityEngine.Random.Range(0, nodesList.Count)].position;
            return myPos;
        }

        // マス計算
        int listIndex = 0;
        nodesList.Add(new Node(startPos, startPos));
        while (myPos != targetPos && listIndex < 1000 && nodesList.Count > 0) // TODO: listIndex < 1000は維持的、ダンジョンの大きさで最大値異なる
        {
            _CheckSurroundingNodes();

            listIndex++;
            if (listIndex < nodesList.Count)
            {
                myPos = nodesList[listIndex].position;
            }
        }

        // targetPosまでたどり着いた時、リストを逆にして次のマス目を選択する
        if (myPos == targetPos)
        {
            nodesList.Reverse();
            for (int i = 0; i < nodesList.Count; i++)
            {
                if (myPos == nodesList[i].position)
                {
                    Vector2 reserved = DungeonManager.instance.AllEnemiesTargetPos.Find(targetPos => Vector2.Equals(targetPos, myPos));
                    if (!Vector2.Equals(reserved, myPos) && nodesList[i].parent == startPos)
                    {
                        DungeonManager.instance.SetAllEnemiesTargetPos(myPos);
                        return myPos; // 次のマス目
                    }
                    myPos = nodesList[i].parent;
                }
            }
        }
        return startPos;
    }

    IEnumerator SmoothMove()
    {
        isMoving = true;
        while (GameManager.instance.CurrentGameState != GameState.FloorChange && Vector2.Distance(transform.position, curPos) > .01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, curPos, 5f * Time.deltaTime);
            yield return null; // 1フレーム待つ
        }
        transform.position = curPos;

        // 移動するごとに下のFloorを親にする
        GameObject floor = DungeonManager.instance.GetFloorByPos(curPos);
        if (floor != null) transform.SetParent(floor.transform);

        isMoving = false;
    }

    /// <summary>
    /// プレイヤーとの距離によって放浪するかプレイヤーを追いかけるかプレイヤーを攻撃する関数
    /// </summary>
    /// <returns></returns>
    public IEnumerator Movement()
    {
        // TODO: パフォーマンス改善
        // TODO: 1体1マス以内に入ると他の敵が1マス以内に来ない
        yield return new WaitForSeconds(GameManager.instance.TurnDelay);

        if (!isMoving && GameManager.instance.CurrentGameState == GameState.EnemyTurn)
        {
            // プレイヤーとの距離を取得
            float distToPlayer = GetDistanceFromPlayer();

            // 範囲外は放浪
            if (distToPlayer > alertRange)
            {
                // Debug.Log("範囲外放浪");
                Patrol();
            }
            else
            {
                // Debug.Log("範囲内 " + distToPlayer);

                // 攻撃
                if (distToPlayer <= attackRange)
                {
                    // Debug.Log("普通に攻撃");
                    Attack();
                }
                // 追いかける
                else
                {
                    // Debug.Log("追いかける");

                    Vector2 targetPos = player.TargetPos;
                    Vector2 newPos = FindNextStep(transform.position, targetPos, distToPlayer);
                    if (newPos != curPos)
                    {
                        // 移動先がPlayerと同じ場合その場で攻撃する
                        if (newPos == targetPos)
                        {
                            // Debug.Log("その場で攻撃");
                            Attack();
                        }
                        else
                        {
                            curPos = newPos;
                            StartCoroutine(SmoothMove());
                        }
                    }
                    else
                    {
                        // Debug.Log("パトロール");
                        Patrol();
                    }
                }
            }
        }
    }

    public void Attack()
    {
        int roll = UnityEngine.Random.Range(0, 100);
        if (roll <= attackPercentage)
        {
            player.TakeDamage(power);
        }
    }

    public void TakeDamage(int damageToTake)
    {
        float dist = GetDistanceFromPlayer();
        if (dist > takeDamageRange) return;

        currentHealth -= damageToTake;
        if (currentHealth <= 0) Die();
    }

    /// <summary>
    /// 敵を破壊する時にDungeonManagerのEnemeisリストからも削除する
    /// </summary>
    void Die()
    {
        player.GainXp(enemyXp.MaxXp);
        DungeonManager.instance.RemoveEnemy(this);
        Destroy(gameObject);
    }

    // private void OnMouseDown()
    // {
    //     // 敵をタップでも攻撃可能
    //     player.IncreaseHealthByPlayerMovement();
    //     TakeDamage(player.CurrentPower);
    // }

    bool IsHit(string direction, Vector2 myPos, Vector2 hitSize, LayerMask targetMask)
    {
        // TODO: 引数をup right down leftに指定したい
        if (direction == "up") return Physics2D.OverlapBox(myPos + Vector2.up, hitSize, 0, targetMask);
        else if (direction == "right") return Physics2D.OverlapBox(myPos + Vector2.right, hitSize, 0, targetMask);
        else if (direction == "down") return Physics2D.OverlapBox(myPos + Vector2.down, hitSize, 0, targetMask);
        else if (direction == "left") return Physics2D.OverlapBox(myPos + Vector2.left, hitSize, 0, targetMask);

        return false;
    }

    float GetDistanceFromPlayer()
    {
        // TODO: 処理が重い時などまだenemyのcoroutine途中でDestroyされるとnullになる
        try
        {
            return Vector2.Distance(transform.position, player.transform.position);
        }
        catch (Exception e)
        {
            Debug.Log("Enemy ERROR: " + e.Message);
            return 0f;
        }
    }
}
