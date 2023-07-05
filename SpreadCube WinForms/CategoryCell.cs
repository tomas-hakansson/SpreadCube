namespace SpreadCube_WinForms;

internal class CategoryCell : ICell
{
    public string Name { get; private set; }

    public CategoryCell(string name) =>
        Name = name;
}