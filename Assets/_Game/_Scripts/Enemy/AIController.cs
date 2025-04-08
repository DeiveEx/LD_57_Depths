using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Behavior;
using UnityEngine;

public class AIController : MonoBehaviour
{
    [SerializeField] private BehaviorGraphAgent _behaviorAgent;
    [SerializeField] private float _moveSpeed = 1;
    [SerializeField] private float _digSpeed = 1;
    [SerializeField] private float _maxRoamDistance = 1;

    private Vector3Int _housePosition;
    private List<Vector3Int> _currentPath;
    
    public Vector3Int CurrentGridPos => World.GetGridPosFromWorldPos(transform.position);
    public bool IsFollowingPath => _currentPath != null;
    public float MaxRoamDistance => _maxRoamDistance;

    private World World => World.Instance;

    private void Awake()
    {
        _housePosition = CurrentGridPos;
        _behaviorAgent.BlackboardReference.SetVariableValue("HousePosition", _housePosition);
        _behaviorAgent.BlackboardReference.SetVariableValue("RoamRange", _maxRoamDistance);
        
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
        int pathIndex = 0;

        while (pathIndex < _currentPath.Count)
        {
            var targetPos = _currentPath[pathIndex];
            
            //Dig if target pos is a wall
            var targetBlock = World.GetBlock(targetPos);

            if (targetBlock.Type == BlockType.Rock)
            {
                Dig(targetPos);
                yield return new WaitForSeconds(1f / _digSpeed);
            }
            else
            {
                Move(targetPos);
                pathIndex++;
                yield return new WaitForSeconds(1f / _moveSpeed);
            }
        }

        _currentPath = null;
    }

    private void Dig(Vector3Int targetPos)
    {
        World.TrySetBlock(targetPos, BlockType.Empty);
    }

    private void Move(Vector3Int targetPos)
    {
        var tween = transform.DOMove(World.GetWorldCenterPosition(targetPos), 1f / _moveSpeed)
            .Play();
    }

    private void OnDrawGizmosSelected()
    {
        if(!Application.isPlaying)
            return;

        Gizmos.color = Color.red;
        
        PathFinderService.DrawPathGizmos(_currentPath);
        var housePos = World.Instance.GetWorldCenterPosition(_housePosition);
        Gizmos.DrawWireSphere(housePos, 0.25f);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, housePos);
    }
}
