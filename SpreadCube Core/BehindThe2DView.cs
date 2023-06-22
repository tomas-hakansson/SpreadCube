namespace SpreadCube_Core;

public class BehindThe2DView
{
    /* external api:
     *  data:
     *      the dimensions and their indices (categories and their members).
     *      the cursor position, the current coordinate.
     *      what about ranges for when you select several cells?
     *
     *  operations:
     *      a way of accessing the value of specific coordinates.
     */

    public List<string> HorizontalCategories { get; private set; }
    public List<string> VerticalCategories { get; private set; }
    public List<string> HiddenCategories { get; private set; }

    public Dictionary<string, List<string>> CategoryToIndices { get; private set; }
    public HashSet<(string category, string index)> Cursor { get; private set; }
    //public HashSet<CellValue> CellValues { get; private set; }
    //public Dictionary<CellValue, HashSet<(string category, string index)>> CellValueToCoordinates { get; private set; }
    //public Dictionary<HashSet<(string category, string index)>, CellValue> CoordinatesToCellValue { get; private set; }
    //List<CellValue> _allCells { get; set; }

    public BehindThe2DView()
    {
        /* Changes to data structure:
         *  When thinking about possible optimisations I realised that it would be rather silly to create
         *  and store a Cell for each coordinate even when it holds no value and that made me realize
         *  that I don't think 'VisibleCells' is needed at all. All I need are the categories with the
         *  knowledge of which are horizontal, vertical as well as hidden. The Cells can be stored in
         *  a HashSet. Assuming it works, this completely obviates the whole 'multidimensional array
         *  issue'.
         */

        var yearsIndices = A("2019");
        var monthsIndices = A("Jan", "Feb", "Mar", "Apr", "May", "Jun");
        var daysIndices = A("Mon", "tue");
        var staffIndices = A("Bob", "Matilda");
        var productsIndices = A("LPs", "Letters", "Salves");

        HorizontalCategories = new() { "Years", "Months", "Days" };
        VerticalCategories = new() { "Staff", "Products" };
        HiddenCategories = new();

        //Ponder: We can't handle situations where only one of vertical or horizontal has categories yet.
        //  Not sure how to fix that. See 8:29 in https://www.youtube.com/watch?v=lTko_Pt2ZZg for example.
        //HCats = new();
        //VCats= new() { "Months", "Years" };

        CategoryToIndices = new()
        {
            { "Years", yearsIndices.ToList() },
            { "Months", monthsIndices.ToList() },
            { "Days", daysIndices.ToList() },
            { "Staff", staffIndices.ToList() },
            { "Products", productsIndices.ToList() },
        };

        Cursor = new() { ("Months", "Jan"), ("Years", "2019"), ("Staff", "Bob") };

        //var content = 0;
        //VisibleCells = new CellValue[monthsCount, yearsCount];
        //_allCells = new List<CellValue>();
        //for (int row = 0; row < yearsCount; row++)
        //{
        //    for (int col = 0; col < monthsCount; col++)
        //    {
        //        //set cell value:
        //        //var cell = new CellValue();
        //        //cell.SetTextContent(content.ToString());
        //        VisibleCells[col, row] = cell;
        //        //_allCells.Add(cell);
        //        //content++;

        //        //set months value:
        //        monthsCat.AddIndex(monthsIndices[col], cell);
        //        yearsCat.AddIndex(yearsIndices[row], cell);
        //    }
        //}
        //HorizontalCategories = new Category[] { monthsCat };
        //VerticalCategories = new Category[] { yearsCat };
        //HiddenCategories = Array.Empty<Category>();
    }

    static T[] A<T>(params T[] arg) => arg;

    //public void SetCellContent(string textContent, List<(string category, string index)> coordinates) =>
    //    GetCell(coordinates).TextContent = textContent;

    //public CellValue GetCell2(HashSet<(string category, string index)> coordinates)
    //{
    //    if (CoordinatesToCellValue.ContainsKey(coordinates))
    //        return CoordinatesToCellValue[coordinates];
    //    else
    //        throw new ArgumentException($"In method: {nameof(GetCell2)} - Invalid coordinates");
    //}

    //public CellValue GetCell(List<(string category, string index)> coordinates)
    //{
    //    //Note: The categories are the dimensions. This method must be provided with coordinates in *all* dimensions
    //    //  to ensure that they point to a single cell.
    //    if (coordinates.Count != HorizontalCategories.Length + VerticalCategories.Length + HiddenCategories.Length)
    //        throw new ArgumentException($"In method: {nameof(SetCellContent)} - Invalid coordinates.");

    //    //Note: Each category (dimension) index is associated with a sequence of cells via their ids.
    //    //  Here we try to find a specific cell by finding the intersection of all category indices.
    //    List<Guid> cellIdIntersection = new();
    //    foreach (var (category, index) in coordinates)
    //    {
    //        var cellIds = GetCellIds(category, index);
    //        if (!cellIdIntersection.Any())
    //            cellIdIntersection = cellIds;
    //        else
    //            cellIdIntersection = cellIdIntersection.Intersect(cellIds).ToList();
    //    }

    //    if (cellIdIntersection.Count != 1)
    //        throw new Exception($"In method: {nameof(SetCellContent)} - Invalid category or cell state.");

    //    //Note: Once we have the cell id we can use it to find its corresponding cell and then update its value.
    //    var cellId = cellIdIntersection.First();
    //    var temp = CellValues.FirstOrDefault(c => c.Equals(cellId));//ToDo:                                                   
    //    //Cells.TryGetValue

    //    var cellMatch = _allCells.Where(c => c.Id == cellId);
    //    if (cellMatch.Count() != 1)
    //        throw new Exception($"In method: {nameof(SetCellContent)} - Invalid cell state.");
    //    return cellMatch.First();
    //}

    //List<Guid> GetCellIds(string category, string index)
    //{
    //    var allCategories = HorizontalCategories.Concat(VerticalCategories).Concat(HiddenCategories);
    //    var matched = allCategories.Where(c => c.Name == category);
    //    //Note: Categories should be unique.
    //    if (matched.Count() != 1)
    //        throw new Exception($"In method: {nameof(GetCellIds)} - Invalid category state.");
    //    var iToC = matched.First().IndexToCells;
    //    //Note: As state is handled by code, the index should always have a match.
    //    if (!iToC.ContainsKey(index))
    //        throw new Exception($"In method: {nameof(GetCellIds)} - Invalid index state.");
    //    return iToC[index];
    //}
}