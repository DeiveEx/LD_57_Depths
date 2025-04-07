using DG.Tweening;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform _avatar;
    [SerializeField] private float _moveSpeed = 1;
    [SerializeField] private float _digSpeed = 1;

    private InputState _inputState;
    private float _lastActionTime;
    private Vector3Int _moveDir;
    private Vector3Int _lookDir;
    private bool _isFalling;

    private World World => World.Instance;
    private Vector3Int CurrentGridPos => World.GetGridPosFromWorldPos(transform.position);
    private bool IsDigging => _inputState.IsDiggingForward || _inputState.IsDiggingDown || _inputState.IsDiggingUp;

    private void Awake()
    {
        _lookDir = Vector3Int.right;
    }

    private void Update()
    {
        TryMove();
        TryDig();
        
        _avatar.forward = _lookDir;
    }
    
    public void Teleport(Vector3Int targetPos)
    {
        transform.position = World.GetWorldCenterPosition(targetPos);
    }

    public void SetInput(InputState inputState)
    {
        _inputState = inputState;

        if (_inputState.MoveInput.sqrMagnitude > 0)
        {
            if(_inputState.MoveInput.x != 0)
                _moveDir = Vector3Int.right * (int)Mathf.Sign(_inputState.MoveInput.x);
            else
                _moveDir = Vector3Int.forward * (int) Mathf.Sign(_inputState.MoveInput.y);
        }
        else
        {
            _moveDir = Vector3Int.zero;
        }
        
        if(_moveDir.sqrMagnitude > 0)
            _lookDir = _moveDir;
    }

    private void TryMove()
    {    
        if(Time.time - _lastActionTime < 1f / _moveSpeed)
            return;
        
        //Should we fall?
        var downPos = CurrentGridPos + Vector3Int.down;
        _isFalling = IsBlockFree(downPos);
        
        if(_isFalling)
            Fall();
        else
            Move();
    }

    private void Fall()
    {
        var downPos = CurrentGridPos + Vector3Int.down;
        
        transform.DOMove(World.GetWorldCenterPosition(downPos), 1f / _moveSpeed)
            .Play();
        
        _lastActionTime = Time.time;
    }

    private void Move()
    {
        if(_moveDir.sqrMagnitude == 0)
            return;
        
        //Is there a wall?
        Vector3Int targetPos = CurrentGridPos + _moveDir;

        if (!IsBlockFree(targetPos))
        {
            TryJump();
            return;
        }
        
        transform.DOMove(World.GetWorldCenterPosition(targetPos), 1f / _moveSpeed)
            .Play();
        
        _lastActionTime = Time.time;
    }

    private void TryJump()
    {
        //To be able to jump, 2 specific blocks needs to be empty:
        // - the block above the player
        // - the block in the diagonal-up of the player's facing direction
        Vector3Int targetPos = CurrentGridPos + Vector3Int.up;
        
        if(!IsBlockFree(targetPos))
            return;

        targetPos += _lookDir;
        
        if(!IsBlockFree(targetPos))
            return;

        transform.DOJump(World.GetWorldCenterPosition(targetPos), .5f, 1, 1f / _moveSpeed)
            .Play();
        
        _lastActionTime = Time.time;
    }

    private void TryDig()
    {
        if(!IsDigging || Time.time - _lastActionTime < 1f / _digSpeed)
            return;
        
        Vector3Int digDirection = Vector3Int.zero;
        
        if(_inputState.IsDiggingUp)
            digDirection = Vector3Int.up;
        else if(_inputState.IsDiggingDown)
            digDirection = Vector3Int.down;
        else if (_inputState.IsDiggingForward)
            digDirection = _lookDir;

        var targetPos = CurrentGridPos + digDirection;
        
        if(IsBlockFree(targetPos))
            return;
        
        World.TrySetBlock(targetPos, BlockType.Empty);
        _lastActionTime = Time.time;
    }

    private bool IsBlockFree(Vector3Int targetPos)
    {
        return World.GetBlock(targetPos).Type == BlockType.Empty;
    }
}
