namespace SpreadCube_Core;

public class BehindThe2DView
{
    public Category[] HorizontalCategories { get; private set; }
    public Category[] VerticalCategories { get; private set; }
    public Category[] HiddenCategories { get; private set; }
    public Cell[,] VisibleCells { get; private set; }

    public BehindThe2DView()
    {
        HorizontalCategories = new Category[] { new Category("Months", A("Jan", "Feb", "Mar", "Apr", "May", "Jun")) };
        VerticalCategories = new Category[] { new Category("Years", A("2019", "2020", "2021", "2022", "2023")) };
        HiddenCategories = new Category[0];
        var rowCount = VerticalCategories[0].Vectors.Length;
        var colCount = HorizontalCategories[0].Vectors.Length;
        VisibleCells = new Cell[colCount, rowCount];
        var content = 0;

        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < colCount; col++)
            {
                VisibleCells[col, row] = new Cell(content.ToString());
                content++;
            }
        }
    }

    static T[] A<T>(params T[] arg) => arg;
}