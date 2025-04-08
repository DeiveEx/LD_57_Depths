using System;
using UnityEngine;

public class PlayerScore : MonoBehaviour
{
    private int _score;

    public int Score => _score;

    public event Action OnScoreChanged;

    public void AddScore(int amount)
    {
        _score += amount;
        OnScoreChanged?.Invoke();
    }
}
