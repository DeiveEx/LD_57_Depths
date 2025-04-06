using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private World _world;
    [SerializeField] private PlayerController _player;
    [SerializeField] private Vector3Int _playerStartPos;

    private void Start()
    {
        _world.GenerateNewWorld();
        _world.SetBlock(_playerStartPos, BlockType.Empty);
        _player.Move(_playerStartPos);
    }

    private void Update()
    {
        _world.UpdateChunks();
    }
}
