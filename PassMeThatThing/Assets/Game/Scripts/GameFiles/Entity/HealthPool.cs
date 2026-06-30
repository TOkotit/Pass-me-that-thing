// Game.Scripts.GameFiles.Entity.HealthPool
using System;
using UnityEngine;


public class HealthPool
{
    private int _maxHealth;
    private int _currentHealth;

    public int MaxHealth => _maxHealth;
    public int CurrentHealth => _currentHealth;

    public HealthPool(int maxHealth)
    {
        _maxHealth = maxHealth;
        _currentHealth = maxHealth;
    }

    public void SetMaxHealth(int newMaxHealth)
    {
        _maxHealth = newMaxHealth;
    }
    
    public void SetCurrentHealth(int value)
    {
        _currentHealth = Mathf.Clamp(value, 0, _maxHealth);
    }

    public int TakeDamage(int damage)
    {
        if (damage == 0) return _currentHealth;
        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        // Debug.Log($"Health after damage: {_currentHealth}");
        return _currentHealth;
    }
}