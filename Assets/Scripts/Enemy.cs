using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    [SerializeField] int health = 10;
    [SerializeField] int power = 4;
    [SerializeField] float alertRange;
    [SerializeField] Vector2 patrolInterval;
    [SerializeField] float chaseSpeed;
    [Tooltip("1マスは1.1fが安全")]
    [SerializeField] float attackRange = 1.1f;
    [Tooltip("1マスは1.1fが安全")]
    [SerializeField] float takeDamageRange = 1.1f;

    Player player;
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
        obstacleMask = LayerMask.GetMask("Wall", "Enemy", "Player");
        unwalkableMask = LayerMask.GetMask("Wall", "Enemy");
        curPos = transform.position;
        currentHealth = health;
        StartCoroutine(Movement());
    }

    /// <summary>
    /// 自由に歩く関数
    /// </summary>
    void Patrol()
    {
        // 進行可能マスの生成
        availableMovementList.Clear();
        Vector2 hitSize = Vector2.one * .8f;
        if (!isHit("up", curPos, hitSize, obstacleMask)) availableMovementList.Add(Vector2.up);
        if (!isHit("right", curPos, hitSize, obstacleMask)) availableMovementList.Add(Vector2.right);
        if (!isHit("down", curPos, hitSize, obstacleMask)) availableMovementList.Add(Vector2.down);
        if (!isHit("left", curPos, hitSize, obstacleMask)) availableMovementList.Add(Vector2.left);

        // 進行方向を進行可能なマスからランダムに1マス選ぶ
        if (availableMovementList.Count > 0)
        {
            int randomIndex = Random.Range(0, availableMovementList.Count);
            curPos += availableMovementList[randomIndex];
        }

        StartCoroutine(SmoothMove(Random.Range(patrolInterval.x, patrolInterval.y)));
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
        if (!hit)
        {
            nodesList.Add(new Node(checkPoint, parent));
        }
    }

    Vector2 FindNextStep(Vector2 startPos, Vector2 targetPos)
    {
        int listIndex = 0;
        Vector2 myPos = startPos;
        nodesList.Clear();
        nodesList.Add(new Node(startPos, startPos));
        while (myPos != targetPos && listIndex < 1000 && nodesList.Count > 0) // TODO: listIndex < 1000は維持的、ダンジョンの大きさで最大値異なる
        {
            // up, right, down, leftの移動可能なマスを追加
            CheckNode(myPos + Vector2.up, myPos);
            CheckNode(myPos + Vector2.right, myPos);
            CheckNode(myPos + Vector2.down, myPos);
            CheckNode(myPos + Vector2.left, myPos);

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
                    if (nodesList[i].parent == startPos)
                    {
                        return myPos; // 次のマス目
                    }
                    myPos = nodesList[i].parent;
                }
            }
        }
        return startPos;
    }

    IEnumerator SmoothMove(float speed)
    {
        isMoving = true;
        while (Vector2.Distance(transform.position, curPos) > .01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, curPos, 5f * Time.deltaTime);
            yield return null; // 1フレーム待つ
        }
        transform.position = curPos;

        yield return new WaitForSeconds(speed); // TODO: プレイヤーが動くまで？

        isMoving = false;
    }

    /// <summary>
    /// プレイヤーとの距離によって放浪するかプレイヤーを追いかけるかプレイヤーを攻撃する関数
    /// </summary>
    /// <returns></returns>
    IEnumerator Movement()
    {
        while (true)
        {
            yield return new WaitForSeconds(.1f);

            if (!isMoving)
            {
                // プレイヤーとの距離を取得
                float dist = GetDistanceFromPlayer();
                if (dist <= alertRange)
                {
                    // 攻撃
                    if (dist <= attackRange)
                    {
                        Attack();
                        yield return new WaitForSeconds(Random.Range(.5f, 1.15f));
                    }
                    // 追いかける
                    else
                    {
                        Vector2 newPos = FindNextStep(transform.position, player.transform.position);
                        if (newPos != curPos)
                        {
                            curPos = newPos;
                            StartCoroutine(SmoothMove(chaseSpeed));
                        }
                        else
                        {
                            Patrol();
                        }
                    }
                }
                // 放浪
                else
                {
                    Patrol();
                }
            }
        }
    }

    public void Attack()
    {
        int roll = Random.Range(0, 100);
        if (attackPercentage <= roll)
        {
            Debug.Log(name + " attacked and hit for " + power + " points of damage");
            player.TakeDamage(power);
        }
        else
        {
            Debug.Log(name + " attacked and missed");
        }
    }

    public void TakeDamage(int damageToTake)
    {
        currentHealth -= damageToTake;
        if (currentHealth <= 0) Destroy(gameObject);
    }

    private void OnMouseDown()
    {
        // 敵をタップでも攻撃可能
        float dist = GetDistanceFromPlayer();
        if (dist <= takeDamageRange) TakeDamage(player.CurrentPower);
    }

    bool isHit(string direction, Vector2 myPos, Vector2 hitSize, LayerMask targetMask)
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
        return Vector2.Distance(transform.position, player.transform.position);
    }
}
