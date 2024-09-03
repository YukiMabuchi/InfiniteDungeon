using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] int health = 12;
    [SerializeField] int power = 4;
    // [SerializeField] int stamina; // TODO
    [SerializeField] float speed;
    [SerializeField] Slider healthBar;
    LayerMask obstacleMask;
    Vector2 targetPos;
    Transform GFX;

    int currentHealth = 0;
    int currentPower = 0;
    float flipx;
    bool isMoving;

    public int CurrentPower { get { return currentPower; } }

    void Start()
    {
        currentHealth = health;
        currentPower = power;
        healthBar.maxValue = currentHealth;
        healthBar.value = currentHealth;
        obstacleMask = LayerMask.GetMask("Wall", "Enemy");
        GFX = GetComponentInChildren<SpriteRenderer>().transform;
        flipx = GFX.localScale.x;
    }

    // 移動
    public void Move(string direction)
    {
        bool isHorizontal = direction == "left" || direction == "right";
        bool isVertical = direction == "up" || direction == "down";
        int directionNum = GetDirection(direction);

        // キャラの向き
        if (isHorizontal)
        {
            GFX.localScale = new Vector2(flipx * directionNum, GFX.localScale.y);
        }

        // キャラの進行
        if (!isMoving)
        {
            // 進行先の設定 (1マス先)
            if (isHorizontal)
            {
                targetPos = new Vector2(transform.position.x + directionNum, transform.position.y);
            }
            else if (isVertical)
            {
                targetPos = new Vector2(transform.position.x, transform.position.y + directionNum);
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

    // ダメージ
    public void TakeDamage(int damageToTake)
    {
        currentHealth -= damageToTake;
        healthBar.value = currentHealth;
        if (currentHealth <= 0) GameManager.instance.ShowGameOverPopup();
    }
}
