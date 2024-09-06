using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    PlayerHealth playerHealth;
    int currentLevel = 1;
    int totalXp = 0;
    int requiredXp = 10;
    float requiredXpRamp = 1.1f;

    public int CurrentLevel { get { return currentLevel; } }

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    public void AccumulateXpAndCheckLevelUp(int gainedXp)
    {
        totalXp += gainedXp;
        if (totalXp >= requiredXp) LevelUp();
    }

    public void LevelUp()
    {
        currentLevel++;

        // 経験値修正
        requiredXpRamp += .1f;
        requiredXp = (int)Math.Round(requiredXp * requiredXpRamp, 0, MidpointRounding.AwayFromZero);

        // ステータス修正
        playerHealth.IncreaseMaxHealth(playerHealth.HealthRamp);

        // HP・状態異常回復
        playerHealth.IncreaseHealth(playerHealth.MaxHealth);
    }
}
