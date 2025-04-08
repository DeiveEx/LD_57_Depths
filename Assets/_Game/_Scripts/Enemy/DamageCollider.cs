using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    [SerializeField] private int _damage = 1;
    
    private void OnTriggerEnter(Collider other)
    {
        if(!other.TryGetComponent(out PlayerHealth health))
            return;
        
        health.ApplyDamage(_damage);
    }
}
