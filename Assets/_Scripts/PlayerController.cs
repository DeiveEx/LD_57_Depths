using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private static readonly int PlayerPos = Shader.PropertyToID("_PlayerPos");
    
    [SerializeField] private World _world;

    private void Update()
    {
        Vector3Int currentGridPos = _world.GetGridPosFromWorldPos(transform.position);
        
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
        transform.position = _world.GetWorldCenterPosition(targetPos);
    }
    
    private void Dig(Vector3Int targetPos)
    {
        _world.SetBlock(targetPos, BlockType.Empty);
    }
}
