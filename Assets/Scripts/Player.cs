using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    LayerMask obstacleMask;
    Vector2 targetPos;
    Transform GFX;
    float flipx;
    bool isMoving;

    void Start()
    {
        obstacleMask = LayerMask.GetMask("Wall", "Enemy");
        GFX = GetComponentInChildren<SpriteRenderer>().transform;
        flipx = GFX.localScale.x;
    }

    void Update()
    {
        Move();
    }

    void Move()
    {
        // キー入力で方向転換
        float horizontal = Math.Sign(Input.GetAxisRaw("Horizontal")); // InputManagerの設定の名前
        float vertical = Math.Sign(Input.GetAxisRaw("Vertical")); // InputManagerの設定の名前

        if (Mathf.Abs(horizontal) > 0 || Mathf.Abs(vertical) > 0) // キー入力時のみ
        {
            // キャラの向き
            if (Mathf.Abs(horizontal) > 0)
            {
                GFX.localScale = new Vector2(flipx * horizontal, GFX.localScale.y);
            }

            // キャラの進行
            if (!isMoving)
            {
                // 進行先の設定 (1マス先)
                if (Mathf.Abs(horizontal) > 0)
                {
                    targetPos = new Vector2(transform.position.x + horizontal, transform.position.y);
                }
                else if (Mathf.Abs(vertical) > 0)
                {
                    targetPos = new Vector2(transform.position.x, transform.position.y + vertical);
                }

                // check for collisions
                Vector2 hitSize = Vector2.one * 0.8f;
                Collider2D hit = Physics2D.OverlapBox(targetPos, hitSize, 0, obstacleMask);

                // 進行
                if (!hit)
                {
                    StartCoroutine(SmoothMove());
                }
            }
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
        isMoving = false;
    }
}
