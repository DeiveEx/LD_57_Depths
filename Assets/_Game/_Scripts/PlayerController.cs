using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private static readonly int PlayerPos = Shader.PropertyToID("_PlayerPos");
    
    [SerializeField] private World _world;
    [SerializeField] private float _moveSpeed = 1;
    [SerializeField] private float _digSpeed = 1;
    
    private bool _canMove = true;
    private float _lastActionTime;

    private void Awake()
    {
        
    }

    private void Update()
    {
        Vector3Int currentGridPos = _world.GetGridPosFromWorldPos(transform.position);
        
        if(Keyboard.current.wKey.isPressed)
            MoveOrDig(currentGridPos + Vector3Int.forward);
        
        if(Keyboard.current.sKey.isPressed)
            MoveOrDig(currentGridPos + Vector3Int.back);
        
        if(Keyboard.current.aKey.isPressed)
            MoveOrDig(currentGridPos + Vector3Int.left);
        
        if(Keyboard.current.dKey.isPressed)
            MoveOrDig(currentGridPos + Vector3Int.right);
        
        if(Keyboard.current.eKey.isPressed)
            MoveOrDig(currentGridPos + Vector3Int.up);
        
        if(Keyboard.current.qKey.isPressed)
            MoveOrDig(currentGridPos + Vector3Int.down);
        
        Shader.SetGlobalVector(PlayerPos, transform.position);
    }

    private void MoveOrDig(Vector3Int targetPos)
    {
        switch (_world.GetBlock(targetPos))
        {
            case BlockType.Empty:
                Move(targetPos);
                break;
            
            case BlockType.Rock:
                Dig(targetPos);
                break;
        }
    }

    public void Move(Vector3Int targetPos)
    {
        if(Time.time - _lastActionTime < 1f / _moveSpeed)
            return;
        
        transform.position = _world.GetWorldCenterPosition(targetPos);
        _lastActionTime = Time.time;
    }
    
    private void Dig(Vector3Int targetPos)
    {
        if(Time.time - _lastActionTime < 1f / _digSpeed)
            return;
        
        _world.TrySetBlock(targetPos, BlockType.Empty);
        _lastActionTime = Time.time;
    }

    public void Teleport(Vector3Int targetPos)
    {
        transform.position = _world.GetWorldCenterPosition(targetPos);
    }
}
