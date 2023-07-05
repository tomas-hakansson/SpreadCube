namespace SpreadCube_WinForms;

internal class Categories : IArea
{
    public AreaType Type { get; private set; }
    public ICell Cell { get; private set; }

    public Categories(AreaType type, ICell cell)
    {
        Type = type;
        Cell = cell;
    }
}