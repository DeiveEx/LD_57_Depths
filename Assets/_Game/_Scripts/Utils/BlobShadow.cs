using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(DecalProjector))]
public class BlobShadow : MonoBehaviour
{
    #region Subtypes
    
    #endregion
    
    #region Fields

    [SerializeField] private Vector2 _minMaxDistance;
    [SerializeField] private Vector2 _minMaxScale;
    [SerializeField] private LayerMask _groundMask;

    private DecalProjector _decal;
    
    #endregion

    #region Properties

    #endregion

    #region Events & Delegates

    #endregion

    #region Unity Events

    private void Awake()
    {
        _decal = GetComponent<DecalProjector>();
    }

    private void Update()
    {
        float targetScale = _minMaxScale.x;

        if (Physics.Raycast(transform.position, transform.forward, out var hit, 100, _groundMask))
        {
            float distance = Mathf.InverseLerp(_minMaxDistance.x, _minMaxDistance.y, hit.distance);
            targetScale = Mathf.Lerp(_minMaxScale.y, _minMaxScale.x, distance);
        }

        transform.localScale = Vector3.one * targetScale;
    }

    #endregion

    #region Public Methods

    #endregion

    #region Private Methods

    #endregion
}
