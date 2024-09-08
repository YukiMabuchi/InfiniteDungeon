using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPower : MonoBehaviour
{
    [SerializeField] int maxPower = 4;
    LayerMask enemyMask;

    Player player;
    PlayerHealth playerHealth;
    int currentPower = 0;
    int attackSuccessPercentage = 90;

    public int CurrentPower { get { return currentPower; } }

    void Start()
    {
        player = GetComponent<Player>();
        playerHealth = GetComponent<PlayerHealth>();
        enemyMask = LayerMask.GetMask("Enemy");
        currentPower = maxPower;
    }

    private void Update()
    {
        // テスト用
        if (Application.isEditor && Input.GetKeyDown(KeyCode.B))
        {
            maxPower = 1000;
            currentPower = maxPower;
        }
    }

    // 攻撃
    public void AttackEnemy()
    {
        if (GameManager.instance.CurrentGameState == GameState.Waiting)
        {
            // HP回復
            playerHealth.IncreaseHealthByPlayerMovement();

            // 攻撃判定
            int roll = Random.Range(0, 100);
            if (player.IsHit(player.CurrentDirection, enemyMask))
            {
                if (roll <= attackSuccessPercentage)
                {
                    GameObject floor = DungeonManager.instance.GetFloorByPos(player.GenerateTargetPos(player.CurrentDirection));
                    if (floor == null) return;

                    Enemy enemy = floor.GetComponentInChildren<Enemy>();
                    if (enemy != null) enemy.TakeDamage(currentPower);
                }
                else
                {
                    // 攻撃が外れた
                }
            }
            GameManager.instance.SetCurrentState(GameState.PlayerTurn);
        }
    }
}
