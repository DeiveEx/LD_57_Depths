using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GridGenerator _gridGenerator;
    [SerializeField] private Vector3Int _gridSize;
    [SerializeField] private TestPlayer _player;
    [SerializeField] private Vector3Int _playerStartPos;

    private void Awake()
    {
        _gridGenerator.GenerateGridData(_gridSize);
        _gridGenerator.BuildGrid();
    }

    private void Start()
    {
        _gridGenerator.SetTile(_playerStartPos, TileType.Empty);
        _player.Move(_playerStartPos);
    }
}
