namespace SpreadCube_Core;

public class BehindThe2DView
{
    public Category[] HorizontalCategories { get; private set; }
    public Category[] VerticalCategories { get; private set; }
    public Category[] HiddenCategories { get; private set; }
    public Cell[,] VisibleCells { get; private set; }

    List<Cell> _allCells { get; set; }

    public BehindThe2DView()
    {
        string[] monthsIndices = A("Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug");
        var colCount = monthsIndices.Length;
        Category months = new("Months");
        string[] yearsIndices = A("2019", "2020", "2021", "2022", "2023");
        var rowCount = yearsIndices.Length;
        Category years = new("Years");
        var content = 0;
        VisibleCells = new Cell[colCount, rowCount];
        _allCells = new List<Cell>();
        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < colCount; col++)
            {
                //set cell value:
                var cell = new Cell();
                cell.SetTextContent(content.ToString());
                VisibleCells[col, row] = cell;
                _allCells.Add(cell);
                content++;

                //set months value:
                months.AddIndex(monthsIndices[col], cell);
                years.AddIndex(yearsIndices[row], cell);
            }
        }
        HorizontalCategories = new Category[] { months };
        VerticalCategories = new Category[] { years };
        HiddenCategories = Array.Empty<Category>();
    }

    static T[] A<T>(params T[] arg) => arg;

    public void SetCellContent(string textContent, List<(string category, string index)> coordinates)
    {
        //Note: The categories are the dimensions. This method must be provided with coordinates in *all* dimensions
        //  to ensure that they point to a single cell.
        if (coordinates.Count != HorizontalCategories.Length + VerticalCategories.Length + HiddenCategories.Length)
            throw new ArgumentException($"In method: {nameof(SetCellContent)} - Invalid coordinate.");

        //Note: Each category (dimension) index is associated with a sequence of cells via their ids.
        //  Here we try to find a specific cell by finding the intersection of all category indices.
        List<Guid> cellIdIntersection = new();
        foreach (var (category, index) in coordinates)
        {
            var cellIds = GetCellIds(category, index);
            if (!cellIdIntersection.Any())
                cellIdIntersection = cellIds;
            else
                cellIdIntersection = cellIdIntersection.Intersect(cellIds).ToList();
        }

        if (cellIdIntersection.Count != 1)
            throw new Exception($"In method: {nameof(SetCellContent)} - Invalid category or cell state.");

        //Note: Once we have the cell id we can use it to find its corresponding cell and then update its value.
        var cellId = cellIdIntersection.First();
        var cellMatch = _allCells.Where(c => c.Id == cellId);
        if (cellMatch.Count() != 1)
            throw new Exception($"In method: {nameof(SetCellContent)} - Invalid cell state.");
        cellMatch.First().TextContent = textContent;
    }

    List<Guid> GetCellIds(string category, string index)
    {
        var allCategories = HorizontalCategories.Concat(VerticalCategories).Concat(HiddenCategories);
        var matched = allCategories.Where(c => c.Name == category);
        //Note: Categories should be unique.
        if (matched.Count() != 1)
            throw new Exception($"In method: {nameof(GetCellIds)} - Invalid category state.");
        var iToC = matched.First().IndexToCells;
        //Note: As state is handled by code, the index should always have a match.
        if (!iToC.ContainsKey(index))
            throw new Exception($"In method: {nameof(GetCellIds)} - Invalid index state.");
        return iToC[index];
    }
}