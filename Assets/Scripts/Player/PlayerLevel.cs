using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerLevel : MonoBehaviour
{
    [SerializeField] Slider xpBar;
    [SerializeField] TextMeshProUGUI levelCount;

    PlayerHealth playerHealth;
    int currentLevel = 1;
    int totalXp = 0;
    int currentXp = 0;
    int requiredXp = 10;
    float requiredXpRamp = 1.1f;

    public int CurrentLevel { get { return currentLevel; } }

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        xpBar.maxValue = requiredXp;
        xpBar.value = currentXp;
        UpdateLevelCount(currentLevel);
    }

    public void AccumulateXpAndCheckLevelUp(int gainedXp)
    {
        currentXp += gainedXp;
        totalXp += currentXp;
        xpBar.value = currentXp;
        if (currentXp >= requiredXp) LevelUp();
    }

    public void LevelUp()
    {
        currentLevel++;
        UpdateLevelCount(currentLevel);

        int exceededXP = currentXp - requiredXp;

        // 経験値修正
        requiredXpRamp += .1f;
        requiredXp = (int)Math.Round(requiredXp * requiredXpRamp, 0, MidpointRounding.AwayFromZero);
        xpBar.maxValue = requiredXp;

        // ステータス修正
        playerHealth.IncreaseMaxHealth(playerHealth.HealthRamp);

        // HP・状態異常回復
        playerHealth.IncreaseHealth(playerHealth.MaxHealth);

        currentXp = exceededXP;
        xpBar.value = currentXp;

        // TODO: 一回の経験値で連続的なレベルアップ
        int i = 0;
        while (currentXp >= requiredXp)
        {
            if (i > 3) { Debug.Log("infiniteloop"); break; }
            LevelUp();
            i++;
        }
    }

    void UpdateLevelCount(int level)
    {
        levelCount.text = level.ToString();
    }
}
