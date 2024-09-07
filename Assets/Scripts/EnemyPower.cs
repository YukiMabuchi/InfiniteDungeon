using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPower : MonoBehaviour
{
    [SerializeField] int maxPower = 4;
    [Tooltip("1マスは1.1fが安全")]
    [SerializeField] float attackRange = 1.1f;

    int attackSuccessPercentage = 70;

    public float AttackRange { get { return attackRange; } }
    public int MaxPower { get { return maxPower; } }

    public void AttackPlayer(Player player)
    {
        int roll = Random.Range(0, 100);
        if (roll <= attackSuccessPercentage) player.TakeDamage(maxPower);
    }
}
