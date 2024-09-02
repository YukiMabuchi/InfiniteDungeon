using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float alertRange;
    public Vector2 patrolInterval;
    public float chaseSpeed;
    public Vector2 damageRange;

    Player player;
    Vector2 curPos;
    LayerMask obstacleMask, unwalkableMask;
    List<Vector2> availableMovementList = new List<Vector2>();
    List<Node> nodesList = new List<Node>();
    bool isMoving;

    void Start()
    {
        player = FindObjectOfType<Player>();
        obstacleMask = LayerMask.GetMask("Wall", "Enemy", "Player");
        unwalkableMask = LayerMask.GetMask("Wall", "Enemy");
        curPos = transform.position;
        StartCoroutine(Movement());
    }

    /// <summary>
    /// 自由に歩く関数
    /// </summary>
    void Patrol()
    {
        availableMovementList.Clear();

        Vector2 hitSize = Vector2.one * .8f;

        // 進行可能マスの生成
        Collider2D hitUp = Physics2D.OverlapBox(curPos + Vector2.up, hitSize, 0, obstacleMask);
        if (!hitUp)
        {
            availableMovementList.Add(Vector2.up);
        }

        Collider2D hitRight = Physics2D.OverlapBox(curPos + Vector2.right, hitSize, 0, obstacleMask);
        if (!hitRight)
        {
            availableMovementList.Add(Vector2.right);
        }

        Collider2D hitDown = Physics2D.OverlapBox(curPos + Vector2.down, hitSize, 0, obstacleMask);
        if (!hitDown)
        {
            availableMovementList.Add(Vector2.down);
        }

        Collider2D hitLeft = Physics2D.OverlapBox(curPos + Vector2.left, hitSize, 0, obstacleMask);
        if (!hitLeft)
        {
            availableMovementList.Add(Vector2.left);
        }

        // 進行方向を進行可能なマスからランダムに1マス選ぶ
        if (availableMovementList.Count > 0)
        {
            int randomIndex = Random.Range(0, availableMovementList.Count);
            curPos += availableMovementList[randomIndex];
        }

        StartCoroutine(SmoothMove(Random.Range(patrolInterval.x, patrolInterval.y)));
    }

    // TODO
    void Attack()
    {
        int roll = Random.Range(0, 100);
        if (roll > 50)
        {
            float damageAmount = Mathf.Ceil(Random.Range(damageRange.x, damageRange.y));
            Debug.Log(name + " attacked and hit for " + damageAmount + " points of damage");
        }
        else
        {
            Debug.Log(name + " attacked and missed");
        }
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
                float dist = Vector2.Distance(transform.position, player.transform.position);
                if (dist <= alertRange)
                {
                    // 攻撃
                    if (dist <= 1.1f) // 1マスは1.1の方が安全
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
}
