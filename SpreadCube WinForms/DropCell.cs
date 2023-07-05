namespace SpreadCube_WinForms;

internal class DropCell : ICell
{
    public int Index { get; private set; }

    public DropCell(int index) =>
        Index = index;
}
