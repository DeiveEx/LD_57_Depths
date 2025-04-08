using TMPro;
using UnityEngine;

public class HudController : MonoBehaviour
{
    [SerializeField] private PlayerScore _playerScore;
    [SerializeField] private TMP_Text _scoreText;

    private void Awake()
    {
        _playerScore.OnScoreChanged += OnScoreChange;
        OnScoreChange();
    }

    private void OnDestroy()
    {
        _playerScore.OnScoreChanged -= OnScoreChange;
    }

    private void OnScoreChange()
    {
        _scoreText.text = $"Score: {_playerScore.Score}";
    }
}
