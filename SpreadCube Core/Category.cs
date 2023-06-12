namespace SpreadCube_Core;

public class Category
{
    public string Name { get; }
    public string[] Vectors { get; }

    public Category(string name, string[] vectors)
    {
        Name = name;
        Vectors = vectors;
    }
}