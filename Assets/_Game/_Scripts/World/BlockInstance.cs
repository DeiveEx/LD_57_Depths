public class BlockInstance
{
    public BlockType Type => Data != null ? Data.Type : BlockType.None;
    public BlockDataSO Data { get; }

    public BlockInstance(BlockDataSO data)
    {
        Data = data;
    }
}
