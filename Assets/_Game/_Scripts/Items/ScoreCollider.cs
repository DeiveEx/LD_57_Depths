using UnityEngine;

public class ScoreCollider : MonoBehaviour
{
    [SerializeField] private int _scoreToAdd = 1;
    
    private void OnTriggerEnter(Collider other)
    {
        if(!other.TryGetComponent(out PlayerScore score))
            return;
        
        score.AddScore(_scoreToAdd);
        Destroy(this.gameObject);
    }
}
