using SpreadCube_Core;

namespace SpreadCube_WinForms;

public partial class Frm_main : Form
{
    Panel _pnl_Spreadsheet;
    TextBox _tb_activeCell;
    VScrollBar _vScrollBar;
    HScrollBar _hScrollBar;

    BehindThe2DView _core;

    Dictionary<Rectangle, IArea> _guiRectangleToArea;

    public Frm_main()
    {
        _pnl_Spreadsheet = new();
        _tb_activeCell = new();
        _vScrollBar = new();
        _hScrollBar = new();

        _core = new();
        _guiRectangleToArea = new();
        //initialise generated components:
        InitializeComponent();
        //Initialise components
        Initialise();
    }

    void Initialise()
    {
        // A panel that we can draw our spreadsheet on:
        _pnl_Spreadsheet.Name = nameof(_pnl_Spreadsheet);
        _pnl_Spreadsheet.TabIndex = 2;
        _pnl_Spreadsheet.Dock = DockStyle.Fill;
        _pnl_Spreadsheet.Paint += new PaintEventHandler(Pnl_Spreadsheet__Paint);
        _pnl_Spreadsheet.MouseDown += new MouseEventHandler(Pnl_Spreadsheet__MouseDown);
        _pnl_Spreadsheet.MouseUp += new MouseEventHandler(Pnl_Spreadsheet__MouseUp);
        _pnl_Spreadsheet.Resize += new EventHandler(Pnl_Spreadsheet__Resize);
        Controls.Add(_pnl_Spreadsheet);

        // Vertical scroll bar:
        _vScrollBar.Name = nameof(_vScrollBar);
        _vScrollBar.Dock = DockStyle.Right;
        //https://stackoverflow.com/a/46134105
        //_vScrollBar.LargeChange = 50;//along with the maximum value, affects the size of the movable bar.
        //this is how we use scroll bars:
        //https://stackoverflow.com/a/17971078
        _vScrollBar.ValueChanged += new EventHandler(ScrollBar__ValueChanged);
        Controls.Add(_vScrollBar);

        // Horizontal scroll bar:
        _hScrollBar.Name = nameof(_hScrollBar);
        _hScrollBar.Dock = DockStyle.Bottom;
        _hScrollBar.ValueChanged += new EventHandler(ScrollBar__ValueChanged);
        Controls.Add(_hScrollBar);


        // A textbox on our painted panel:
        _tb_activeCell.Name = nameof(_tb_activeCell);
        _tb_activeCell.AutoSize = false;
        _tb_activeCell.Location = new Point(0, 0);
        //activeCell.Size = new Size(50, 100);
        _tb_activeCell.Visible = false;
        _tb_activeCell.LostFocus += new EventHandler(Tb_activeCell__LostFocus);
        _tb_activeCell.LocationChanged += new EventHandler(Tb_activeCell__LocationChanged);
        //_pnl_Spreadsheet.Controls.Add(_tb_activeCell);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        // To create the space in the lower right corner between the two scroll bars:
        // copied from: https://stackoverflow.com/q/32130907 with minor changes.
        base.OnSizeChanged(e);
        _vScrollBar.MaximumSize = new Size(
            Int32.MaxValue,
            this.ClientSize.Height - _hScrollBar.Height);
        _hScrollBar.MaximumSize = new Size(
            this.ClientSize.Width - _vScrollBar.Width,
            Int32.MaxValue);
    }

    void Pnl_Spreadsheet__Paint(object? sender, PaintEventArgs e)
    {
        PaintSpreadsheet();
#if DEBUG
        //For testing when adding other controls besides this one:
        var cr = _pnl_Spreadsheet.ClientRectangle;
        var rec = new Rectangle(cr.X, cr.Y, cr.Width - 1, cr.Height - 1);
        e.Graphics.DrawRectangle(Pens.Blue, rec);
#endif
    }

    CategoryListType? _draggedCategoryListType = null;
    string _draggedCategory = string.Empty;

    void Pnl_Spreadsheet__MouseDown(object? sender, MouseEventArgs e)
    {
        ////ToDo: make the failfast depend on _core data:

        ////evenHeight is the maximal height, that is evenly divisible by cell height, that can fit inside the panel.
        //int pHeight = _pnl_Spreadsheet.Height;
        //var evenHeight = pHeight - pHeight % _tb_activeCell.Height;
        ////if the mouse cursor is outside of the grid:
        //if (e.Y > evenHeight)
        //    return;
        //// see evenHeight comment
        //int pWidth = _pnl_Spreadsheet.Width;
        //var evenWidth = pWidth - pWidth % _tb_activeCell.Width;
        ////if the mouse cursor is outside of the grid:
        //if (e.X > evenWidth)
        //    return;
        ////Figure out the X and Y of the containing cell:
        //var xCoord = e.X - e.X % _tb_activeCell.Width;
        //var yCoord = e.Y - e.Y % _tb_activeCell.Height;
        ////Move the textbox there and turn it visible:
        //_tb_activeCell.Visible = false;
        //_tb_activeCell.Location = new Point(xCoord, yCoord);
        //_tb_activeCell.Visible = true;
        //_tb_activeCell.Focus();

        /* for drag and drop of categories:
         *  identify what the cursor is on
         *  if category
         *      go into drag and drop mode
         *  if dropped on non category area
         *      return to original position
         *  else
         *      set to new position
         */
        IArea area;
        var matches = _guiRectangleToArea.Where(pp => pp.Key.Contains(e.Location));
        if (matches.Any())
        {
            if (matches.Count() != 1)
                throw new Exception("There can be only one!");

            area = matches.First().Value;
            switch (area.Type)
            {
                case AreaType.HorizontalCategories:
                    _draggedCategoryListType = CategoryListType.Horizontal;
                    AssignDraggedCategory();
                    break;
                case AreaType.VerticalCategories:
                    _draggedCategoryListType = CategoryListType.Vertical;
                    AssignDraggedCategory();
                    break;
                case AreaType.HiddenCategories:
                    _draggedCategoryListType = CategoryListType.Hidden;
                    AssignDraggedCategory();
                    break;
                case AreaType.HorizontalIndices:
                case AreaType.VerticalIndices:
                case AreaType.Cells:
                default:
                    throw new NotImplementedException();
            }
        }

        void AssignDraggedCategory()
        {
            if (area is not Categories categoryArea)
            {
                _draggedCategoryListType = null;
                throw new Exception("big error here");
            }

            if (categoryArea.Cell is CategoryCell categoryCell)
                _draggedCategory = categoryCell.Name;
        }
    }

    void Pnl_Spreadsheet__MouseUp(object? sender, MouseEventArgs e)
    {
        IArea area;
        var matches = _guiRectangleToArea.Where(pp => pp.Key.Contains(e.Location));
        if (matches.Any())
        {
            if (matches.Count() != 1)
                throw new Exception("There can be only one!");

            area = matches.First().Value;
            switch (area.Type)
            {
                case AreaType.HorizontalCategories:
                    MoveCategory(CategoryListType.Horizontal);
                    break;
                case AreaType.VerticalCategories:
                    MoveCategory(CategoryListType.Vertical);
                    break;
                case AreaType.HiddenCategories:
                    MoveCategory(CategoryListType.Hidden);
                    break;
                case AreaType.HorizontalIndices:
                case AreaType.VerticalIndices:
                case AreaType.Cells:
                default:
                    throw new NotImplementedException();
            }
        }

        void MoveCategory(CategoryListType categoryList)
        {
            if (area is not Categories categoryArea)
                throw new Exception("big error here");

            if (_draggedCategoryListType != null &&
                !string.IsNullOrWhiteSpace(_draggedCategory) &&
                categoryArea.Cell is DropCell dropCell)
            {
                _core.MoveCategory(_draggedCategory, _draggedCategoryListType.Value, categoryList, dropCell.Index);
                Refresh();
            }

            _draggedCategoryListType = null;
            _draggedCategory = string.Empty;
        }
    }

    void Pnl_Spreadsheet__Resize(object? sender, EventArgs e)
    {
        _pnl_Spreadsheet.Invalidate();
    }

    void ScrollBar__ValueChanged(object? sender, EventArgs e)
    {
        _pnl_Spreadsheet.Invalidate();
    }

    string previousText = string.Empty;
    Point previousTextBoxLocation;
    void Tb_activeCell__LostFocus(object? sender, EventArgs e)
    {
        if (sender is not TextBox tb)
            return;
        if (!string.IsNullOrWhiteSpace(tb.Text))
        {
            previousText = tb.Text;
            previousTextBoxLocation = tb.Location;
        }
    }

    void Tb_activeCell__LocationChanged(object? sender, EventArgs e)
    {
        List<(string category, string index)> coordinates = new();
        if (!string.IsNullOrWhiteSpace(previousText))
        {
            //Note: Write the TextBox previous value in its previous location:
            var pg = _pnl_Spreadsheet.CreateGraphics();
            Brush brush = new SolidBrush(Color.Black);
            pg.DrawString(previousText, Font, brush, previousTextBoxLocation);

            //Note: Update the cell value of the previous location:
            //coordinates = PointToCoordinates(previousTextBoxLocation);
            //_core.SetCellContent(previousText, coordinates);

            previousText = string.Empty;
        }

        //Note: Use the current text location to set the Text of the text box location:
        //coordinates = PointToCoordinates(_tb_activeCell.Location);
        //var cell = _core.GetCell(coordinates);
        _tb_activeCell.Clear();
        //_tb_activeCell.Text = cell.TextContent;
    }

    void PaintSpreadsheet()
    {
        var p = _pnl_Spreadsheet;
        var g = p.CreateGraphics();
        g.Clear(BackColor);
        _guiRectangleToArea.Clear();
        Pen pen = new(Color.Black);
        Brush brush = new SolidBrush(Color.Black);

        var horizontalCategories = _core.HorizontalCategories;
        var verticalCategories = _core.VerticalCategories;

        var textBoxWidth = _tb_activeCell.Width;
        var textBoxHeight = _tb_activeCell.Height;

        var spreadsheetPanelWidth = _pnl_Spreadsheet.Width;
        var spreadsheetPanelHeight = _pnl_Spreadsheet.Height;

        var horizontalScrollBarValue = _hScrollBar.Value;
        var verticalScrollBarValue = _vScrollBar.Value;

        var startingX = verticalCategories.Count * textBoxWidth;
        var startingY = textBoxHeight + horizontalCategories.Count * textBoxHeight;


        //g.DrawString("width value: ", Font, brush, 0, 0);
        //g.DrawString(_pnl_Spreadsheet.Width.ToString(), Font, brush, 120, 0);
        //g.DrawString("height value: ", Font, brush, 0, 16);
        //g.DrawString(_pnl_Spreadsheet.Height.ToString(), Font, brush, 120, 16);

        //g.DrawString("Vscroll value: ", Font, brush, 0, 0);
        //g.DrawString(_vScrollBar.Value.ToString(), Font, brush, 120, 0);
        //g.DrawString("Hscroll value: ", Font, brush, 0, 16);
        //g.DrawString(_hScrollBar.Value.ToString(), Font, brush, 120, 16);

        //todo:
        //  simple experiments to figure out scrollbars.
        //      figure out how I want scrolling to work
        //      figure out how to draw parts of characters 

        var nrOfHCats = horizontalCategories.Count;
        var catWidth = nrOfHCats * textBoxWidth;
        var joinWidth = (nrOfHCats + 1) * (textBoxWidth / 4);
        var categoryListWidth = catWidth + joinWidth;

        List<(AreaType areaType, int startingX, int startingY, int endingY)> categoryListValues = new() {
            (AreaType.HiddenCategories, 0, 0, textBoxHeight),
            (AreaType.HorizontalCategories, spreadsheetPanelWidth - categoryListWidth, 0, textBoxHeight),
            (AreaType.VerticalCategories, 0, spreadsheetPanelHeight - textBoxHeight, spreadsheetPanelHeight)};

        foreach (var item in categoryListValues)
            DrawCategories(g, pen, brush, item.startingX, item.startingY, item.endingY, item.areaType);

        DrawUpperCategorySeparator(g, pen);
        DrawLowerCategorySeparator(g, pen);

        DrawCornerBox(g, pen);

        //the horizontal indices:
        var lengthValues = IndicesWithSizes(horizontalCategories, textBoxWidth)
            .Select((v, i) => AccumulatedIndexSizes(v, startingX, horizontalScrollBarValue));

        int xTextBounds = verticalCategories.Count * textBoxWidth;

        //FixMe: When index goes off panel this throws an exception.
        var horizontalLineLength = lengthValues.First().Last().AccumulatedSize;
        var horizontalLineY = textBoxHeight * 2;
        var HorizontalTextY = textBoxHeight;
        var verticalLineY = textBoxHeight;
        foreach (var innerLengths in lengthValues)
        {
            //draw all the horizontal lines of the horizontal indices:
            g.DrawLine(pen, new Point(startingX, horizontalLineY), new Point(horizontalLineLength, horizontalLineY));
            horizontalLineY += textBoxHeight;
            //draw all the vertical lines of the horizontal indices:
            foreach (var (index, accumulatedLength, length) in innerLengths)
            {
                g.DrawLine(pen, new Point(accumulatedLength, verticalLineY), new Point(accumulatedLength, verticalLineY + textBoxHeight));
                var x = Math.Max(xTextBounds, accumulatedLength - length);
                g.DrawString(index, base.Font, brush, new Point(x, HorizontalTextY));
            }

            verticalLineY += textBoxHeight;
            HorizontalTextY += textBoxHeight;
        }

        //the vertical indices:
        var heightValues = IndicesWithSizes(verticalCategories, textBoxHeight)
            .Select((v, i) => AccumulatedIndexSizes(v, startingY, verticalScrollBarValue));

        int yTextBounds = horizontalCategories.Count * textBoxHeight + textBoxHeight;

        //FixMe: When index goes off panel this throws an exception.
        var verticalLineLength = heightValues.First().Last().AccumulatedSize;
        var horizontalLineX = 0;
        var verticalLineX = textBoxWidth;
        var verticalTextX = 0;
        foreach (var innerHeights in heightValues)
        {
            //draw all the vertical lines of the vertical indices:
            g.DrawLine(pen, new Point(verticalLineX, startingY), new Point(verticalLineX, verticalLineLength));
            verticalLineX += textBoxWidth;
            //draw all the horizontal lines of the vertical indices:
            foreach (var (index, accumulatedHeight, height) in innerHeights)
            {
                g.DrawLine(pen, new Point(horizontalLineX, accumulatedHeight), new Point(horizontalLineX + textBoxWidth, accumulatedHeight));
                var y = Math.Max(yTextBounds, accumulatedHeight - height);
                g.DrawString(index, base.Font, brush, new Point(verticalTextX, y));
            }
            horizontalLineX += textBoxWidth;
            verticalTextX += textBoxWidth;
        }

        //Ponder: should these values be constant (a sliding window looking at a constant state) or not?
        SetScrollBarValues(horizontalLineLength, verticalLineLength);//FutureExperiment: Try hard coding the values.

        //DrawCellLines(g, pen, brush, startingX, startingY);


        ////Note: Write the cell contents:
        //height = tbHeigth;
        //for (int row = 0; row < visCells.GetLength(1); row++)
        //{
        //    width = tbWidth;
        //    for (int col = 0; col < visCells.GetLength(0); col++)
        //    {
        //        Brush brush = new SolidBrush(Color.Black);
        //        var cellContent = visCells[col, row].TextContent;
        //        g.DrawString(cellContent, Font, brush, new Point(width, height));
        //        width += tbWidth;
        //    }
        //    height += tbHeigth;
        //}
    }

    void DrawUpperCategorySeparator(Graphics g, Pen pen) =>
        g.DrawLine(pen, new Point(0, _tb_activeCell.Height), new Point(_pnl_Spreadsheet.Width, _tb_activeCell.Height));

    void DrawLowerCategorySeparator(Graphics g, Pen pen)
    {
        var y = _pnl_Spreadsheet.Height - _tb_activeCell.Height;
        g.DrawLine(pen, new Point(0, y), new Point(_pnl_Spreadsheet.Width, y));
    }

    void DrawCornerBox(Graphics g, Pen pen)
    {
        var nrOfHorizontalCategories = _core.HorizontalCategories.Count;
        var nrOfVerticalCategories = _core.VerticalCategories.Count;
        var textBoxHeight = _tb_activeCell.Height;
        var textBoxWidth = _tb_activeCell.Width;
        //Vertical line:
        int x = nrOfVerticalCategories * textBoxWidth;
        int y = nrOfHorizontalCategories * textBoxHeight + textBoxHeight;
        g.DrawLine(pen, new Point(x, textBoxHeight), new Point(x, y));
        //Horizontal line:
        g.DrawLine(pen, new Point(0, y), new Point(x, y));
    }

    List<(string index, int AccumulatedSize, int size)> AccumulatedIndexSizes(
        List<(string index, int size)> values,
        int startingPosition,
        int offset)
    {
        List<(string, int, int)> result = new();
        int acc = startingPosition;
        int i = 0;
        for (; i < values.Count; i++)
        {
            var (ci, cv) = values[i];
            if (offset <= cv)
            {
                acc += cv - offset;
                result.Add((ci ,acc, cv));
                AccumulatedIndexSizes2ndState();
                return result;
            }
            else
            {
                offset -= cv;
                continue;
            }
        }

        return result;

        void AccumulatedIndexSizes2ndState()
        {
            i++;
            for (; i < values.Count; i++)
            {
                var (ci, cv) = values[i];
                acc += cv;
                result.Add((ci, acc, cv));
            }
        }
    }

    private List<List<(string index, int size)>> IndicesWithSizes(List<string> categories, int baseSize)
    {
        List<List<(string, int)>> result = new();
        var countsAndSizes = IndexCountsAndSizes(categories, baseSize);

        for (int categoryIndex = 0, countIndex = countsAndSizes.Count - 1; countIndex >= 0; categoryIndex++, countIndex--)
        {
            var indices = _core.CategoryToIndices[categories[categoryIndex]];
            var repeatingIndices = Repeat(indices);

            var (count, size) = countsAndSizes[countIndex];
            var Sizes = Enumerable.Repeat(size, count);

            result.Add(repeatingIndices.Zip(Sizes).ToList());
        }
        return result;
    }

    /// <summary>
    /// Repeats the given sequence infinitely.
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    static IEnumerable<string> Repeat(IList<string> values)
    {
        int index = 0;
        while (true)
        {
            yield return values[index];
            index++;
            index %= values.Count;
        }
    }

    List<(int count, int size)> IndexCountsAndSizes(List<string> categories, int baseSize)
    {
        var firstCategory = categories.First();
        var currentIndices = _core.CategoryToIndices[firstCategory];

        int currentCount = currentIndices.Count;
        if (categories.Count == 1)
            return new List<(int count, int width)>() { (currentCount, baseSize) };

        var result = IndexCountsAndSizes(categories.Skip(1).ToList(), baseSize);

        var (pc, pw) = result.Last();
        var currentCellWidth = pc * pw;

        for (int i = 0; i < result.Count; i++)
        {
            var (previousCount, previousWidth) = result[i];
            result[i] = (previousCount * currentCount, previousWidth);
        }

        result.Add((currentCount, currentCellWidth));

        return result;
    }

    /// <summary>
    /// Method copied with minor modifications from:
    /// https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.scrollbar.largechange
    /// </summary>
    /// <param name="hMaximum"></param>
    public void SetScrollBarValues(int hMaximum, int vMaximum)
    {
        //Set the following scrollbar properties:

        //Minimum: Set to 0

        //SmallChange and LargeChange: Per UI guidelines, these must be set
        //    relative to the size of the view that the user sees, not to
        //    the total size including the unseen part.  In this example,
        //    these must be set relative to the picture box, not to the image.

        //Maximum: Calculate in steps:
        //Step 1: The maximum to scroll is the size of the unseen part.
        //Step 2: Add the size of visible scrollbars if necessary.
        //Step 3: Add an adjustment factor of ScrollBar.LargeChange.

        //Configure the horizontal scrollbar
        //---------------------------------------------

        var hScrollBar1 = _hScrollBar;
        var vScrollBar1 = _vScrollBar;

        if (hScrollBar1.Visible)
        {
            hScrollBar1.Minimum = 0;
            //this.hScrollBar1.SmallChange = this.pictureBox1.Width / 20;
            hScrollBar1.SmallChange = _pnl_Spreadsheet.Width / 20;
            //this.hScrollBar1.LargeChange = this.pictureBox1.Width / 10;
            hScrollBar1.LargeChange = _pnl_Spreadsheet.Width / 10;

            //this.hScrollBar1.Maximum = this.pictureBox1.Image.Size.Width - pictureBox1.ClientSize.Width;  //step 1
            hScrollBar1.Maximum = hMaximum;

            if (vScrollBar1.Visible) //step 2
            {
                hScrollBar1.Maximum += vScrollBar1.Width;
            }

            hScrollBar1.Maximum += hScrollBar1.LargeChange; //step 3
        }

        //Configure the vertical scrollbar
        //---------------------------------------------
        if (vScrollBar1.Visible)
        {
            vScrollBar1.Minimum = 0;
            //this.vScrollBar1.SmallChange = this.pictureBox1.Height / 20;
            vScrollBar1.SmallChange = _pnl_Spreadsheet.Height / 20;
            //this.vScrollBar1.LargeChange = this.pictureBox1.Height / 10;
            vScrollBar1.LargeChange = _pnl_Spreadsheet.Height / 10;

            //this.vScrollBar1.Maximum = this.pictureBox1.Image.Size.Height - pictureBox1.ClientSize.Height; //step 1
            vScrollBar1.Maximum = vMaximum;

            if (hScrollBar1.Visible) //step 2
            {
                vScrollBar1.Maximum += hScrollBar1.Height;
            }

            vScrollBar1.Maximum += vScrollBar1.LargeChange; //step 3
        }
    }

    private void DrawCategories(
        Graphics g,
        Pen pen,
        Brush brush,
        int startingX,
        int startingY,
        int endingY,
        AreaType areaType)
    {
        Point upper = new(startingX, startingY);
        Point lower = new(startingX, endingY);
        var textBoxWidth = _tb_activeCell.Width;
        var categoryListIndex = 0;
        var varyingX = startingX;
        Rectangle listCell;
        List<string> categories = areaType switch
        {
            AreaType.HiddenCategories => _core.HiddenCategories,
            AreaType.HorizontalCategories => _core.HorizontalCategories,
            AreaType.VerticalCategories => _core.VerticalCategories,
            _ => throw new ArgumentException("The only valid AreaType here is category types")
        };
        foreach (var category in categories)
        {
            //Note: Draw first line:
            g.DrawLine(pen, upper, lower);
            varyingX += textBoxWidth / 4;
            lower.X = varyingX;

            //Note: Add point to drop cell:
            listCell = new(upper.X, upper.Y, lower.X - upper.X, lower.Y - upper.Y);
            _guiRectangleToArea.Add(listCell, new Categories(areaType, new DropCell(categoryListIndex)));
            categoryListIndex++;
            upper.X = varyingX;

            //Note: Draw second line and category:
            g.DrawLine(pen, upper, lower);
            g.DrawString(category, Font, brush, upper);

            //Note: Add point to category:
            varyingX += textBoxWidth;
            lower.X = varyingX;
            listCell = new(upper.X, upper.Y, lower.X - upper.X, lower.Y - upper.Y);
            _guiRectangleToArea.Add(listCell, new Categories(areaType, new CategoryCell(category)));
            upper.X = varyingX;
        }

        //Note: Draw penultimate line:
        g.DrawLine(pen, upper, lower);
        varyingX += textBoxWidth / 4;

        //Note: Add final point to drop cell:
        lower.X = varyingX;
        listCell = new(upper.X, upper.Y, lower.X - upper.X, lower.Y - upper.Y);
        _guiRectangleToArea.Add(listCell, new Categories(areaType, new DropCell(categoryListIndex)));

        //Note: Draw final line:
        upper.X = varyingX;
        g.DrawLine(pen, upper, lower);
    }

    void DrawCellLines(Graphics g, Pen pen, Brush brush, int startingX, int startingY)
    {
        var textBoxWidth = _tb_activeCell.Width;
        var totalHorizontalCellCount = _core.HorizontalCategories
            .Select(c => _core.CategoryToIndices[c].Count)
            .Aggregate((x, y) => x * y);
        var endingX = startingX + totalHorizontalCellCount * textBoxWidth;

        var textBoxHeight = _tb_activeCell.Height;
        var totalVerticalCellCount = _core.VerticalCategories
            .Select(c => _core.CategoryToIndices[c].Count)
            .Aggregate((x, y) => x * y);
        var endingY = startingY + totalVerticalCellCount * textBoxHeight;

        //Note: Draw horizontal line:
        var varyingY = startingY;
        for (int i = 0; i < totalVerticalCellCount; i++)
        {
            g.DrawLine(pen, new Point(startingX, varyingY), new Point(endingX, varyingY));
            varyingY += textBoxHeight;
        }
        g.DrawLine(pen, new Point(startingX, endingY), new Point(endingX, endingY));

        //Note: Draw vertical line:
        var varyingX = startingX;
        for (int i = 0; i < totalHorizontalCellCount; i++)
        {
            g.DrawLine(pen, new Point(varyingX, startingY), new Point(varyingX, endingY));
            varyingX += textBoxWidth;
        }
        g.DrawLine(pen, new Point(endingX, startingY), new Point(endingX, endingY));
    }
}