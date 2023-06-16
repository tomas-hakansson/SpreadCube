namespace SpreadCube_Core;

public class Category
{
    public string Name { get; private set; }
    public Dictionary<string, List<Guid>> IndexToCells { get; private set; }

    public Category(string name)
    {
        Name = name;
        IndexToCells = new Dictionary<string, List<Guid>>();
    }

    public void AddIndex(string index, Cell cell)
    {
        if (IndexToCells.ContainsKey(index))
            IndexToCells[index].Add(cell.Id);
        else
            IndexToCells[index] = new List<Guid>() { cell.Id };
    }
}