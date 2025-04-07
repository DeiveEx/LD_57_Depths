using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Behavior;
using UnityEngine;

public class AIController : MonoBehaviour
{
    [SerializeField] private BehaviorGraphAgent _behaviorAgent;
    [SerializeField] private float _moveSpeed = 1;

    private Vector3Int _housePosition;
    private List<Vector3Int> _currentPath;
    
    public Vector3Int CurrentGridPos => World.GetGridPosFromWorldPos(transform.position);
    public bool IsFollowingPath => _currentPath != null;
    public Vector3Int HousePosition => _housePosition;

    private World World => World.Instance;

    private void Awake()
    {
        _housePosition = CurrentGridPos;
        
        //Right now apparently the only way to control when the BehaviorAgent starts is to disable the
        //component on the GameObject and the enable it by code...
        _behaviorAgent.enabled = true;
    }

    public void FollowGridPath(List<Vector3Int> path)
    {
        if (IsFollowingPath)
        {
            StopAllCoroutines();
            transform.DOKill();
        }

        _currentPath = path;
        StartCoroutine(FollowPathRoutine());
    }

    private IEnumerator FollowPathRoutine()
    {
        for (int i = 0; i < _currentPath.Count; i++)
        {
            var targetPos = _currentPath[i];
            
            var tween = transform.DOMove(World.GetWorldCenterPosition(targetPos), 1f / _moveSpeed)
                .Play();

            yield return new WaitForSeconds(1f / _moveSpeed);
        }

        _currentPath = null;
    }

    private void OnDrawGizmosSelected()
    {
        PathFinderService.DrawPathGizmos(_currentPath);
    }
}
