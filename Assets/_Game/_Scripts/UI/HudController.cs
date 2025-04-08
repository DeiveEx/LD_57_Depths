using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudController : MonoBehaviour
{
    [SerializeField] private PlayerScore _playerScore;
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _healthText;
    [SerializeField] private CanvasGroup _damageOverlay;
    [SerializeField] private float _damageOverlayAnimDuration = .5f;

    private void Awake()
    {
        _playerScore.OnScoreChanged += OnScoreChange;
        _playerHealth.OnHealthChanged += OnHealthChanged;
        
        OnScoreChange();
        UpdateHealthText();
        
        _damageOverlay.alpha = 0;
    }

    private void OnDestroy()
    {
        _playerScore.OnScoreChanged -= OnScoreChange;
        _playerHealth.OnHealthChanged -= OnHealthChanged;
    }

    private void OnScoreChange()
    {
        _scoreText.text = $"Score: {_playerScore.Score}";
    }
    
    private void OnHealthChanged()
    {
        UpdateHealthText();

        _damageOverlay.DOFade(0, _damageOverlayAnimDuration)
            .From(1)
            .Play();
    }

    private void UpdateHealthText()
    {
        _healthText.text = $"Health: {_playerHealth.CurrentHealth}";
    }
}
