using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int _startingHealth = 3;

    private int _currentHealth;
    
    public event Action OnHealthChanged;

    public int CurrentHealth => _currentHealth;

    private void Awake()
    {
        _currentHealth = _startingHealth;
    }

    public void ApplyDamage(int amount)
    {
        _currentHealth -= amount;
        OnHealthChanged?.Invoke();

        if (_currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        //Reload scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
