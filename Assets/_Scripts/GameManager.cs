using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private World _world;
    [SerializeField] private Vector3Int _playerStartPos;

    private void Awake()
    {
        _world.GenerateNewWorld();
        _world.SetBlock(_playerStartPos, BlockType.Empty);
    }

    private void Update()
    {
        _world.UpdateChunks();
    }
}
