using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private World _world;
    [SerializeField] private TestPlayer _player;
    [SerializeField] private Vector3Int _playerStartPos;

    private void Awake()
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
