using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] int maxHealth = 12;
    [SerializeField] Slider healthBar;
    [SerializeField] TextMeshProUGUI healthCount;
    [SerializeField] int healthRamp = 2;

    int currentHealth = 0;

    /// <summary>
    /// HP回復用Playerの動きの回数。回復するたびに0にする。
    /// </summary>
    int countForHealth = 0;

    public int MaxHealth { get { return maxHealth; } }
    public int CurrentHealth { get { return currentHealth; } }
    public int HealthRamp { get { return healthRamp; } }

    private void Start()
    {
        currentHealth = maxHealth;
        healthBar.maxValue = currentHealth;
        healthBar.value = currentHealth;
        UpdateHealthCount();
    }

    /// <summary>
    /// Playerの行動によりHPを回復する
    /// </summary>
    public void IncreaseHealthByPlayerMovement()
    {
        countForHealth++;
        if (countForHealth == 2)
        {
            IncreaseHealth(1); // TODO: 体力回復の係数調整
            countForHealth = 0;
        }
    }

    public void IncreaseHealth(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        healthBar.value = currentHealth;
        UpdateHealthCount();
    }

    public void DecreaseHealth(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;
        healthBar.value = currentHealth;
        UpdateHealthCount();
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        healthBar.maxValue = maxHealth;
        UpdateHealthCount();
    }

    public void DecreaseMaxHealth(int amount)
    {
        maxHealth -= amount;
        healthBar.maxValue = maxHealth;
        UpdateHealthCount();
    }

    void UpdateHealthCount()
    {
        healthCount.text = maxHealth + " / " + currentHealth;
    }
}
