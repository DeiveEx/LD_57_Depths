using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayer : MonoBehaviour
{
    private static readonly int PlayerPos = Shader.PropertyToID("_PlayerPos");
    
    [SerializeField] private GridGenerator _gridGenerator;

    private void Update()
    {
        Vector3Int currentGridPos = _gridGenerator.GetGridPosFromWorldPos(transform.position);
        
        if(Keyboard.current.wKey.wasPressedThisFrame)
            MoveOrDig(currentGridPos + Vector3Int.forward);
        
        if(Keyboard.current.sKey.wasPressedThisFrame)
            MoveOrDig(currentGridPos + Vector3Int.back);
        
        if(Keyboard.current.aKey.wasPressedThisFrame)
            MoveOrDig(currentGridPos + Vector3Int.left);
        
        if(Keyboard.current.dKey.wasPressedThisFrame)
            MoveOrDig(currentGridPos + Vector3Int.right);
        
        if(Keyboard.current.eKey.wasPressedThisFrame)
            MoveOrDig(currentGridPos + Vector3Int.up);
        
        if(Keyboard.current.qKey.wasPressedThisFrame)
            MoveOrDig(currentGridPos + Vector3Int.down);
        
        Shader.SetGlobalVector(PlayerPos, transform.position);
    }

    private void MoveOrDig(Vector3Int targetPos)
    {
        switch (_gridGenerator.GetTile(targetPos))
        {
            case TileType.Empty:
                Move(targetPos);
                break;
            
            case TileType.Rock:
                Dig(targetPos);
                break;
        }
    }

    public void Move(Vector3Int targetPos)
    {
        transform.position = _gridGenerator.Grid.GetWorldCenterPosition(targetPos);
    }
    
    private void Dig(Vector3Int targetPos)
    {
        _gridGenerator.SetTile(targetPos, TileType.Empty);
    }
}
