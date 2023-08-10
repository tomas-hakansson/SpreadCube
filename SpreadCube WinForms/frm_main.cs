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
        // copied from: https://stackoverflow.com/q/32130907 with minor change.
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
        Invalidate();
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

        var textBoxHeight = _tb_activeCell.Height;

        var startingX = verticalCategories.Count * _tb_activeCell.Width;
        var startingY = textBoxHeight + horizontalCategories.Count * textBoxHeight;

        //g.DrawString("width value: ", Font, brush, 0, 0);
        //g.DrawString(_pnl_Spreadsheet.Width.ToString(), Font, brush, 120, 0);
        //g.DrawString("height value: ", Font, brush, 0, 16);
        //g.DrawString(_pnl_Spreadsheet.Height.ToString(), Font, brush, 120, 16);

        //todo:
        //  simple experiments to figure out scrollbars.

        DrawHorizontalCategories(g, pen, brush, startingX);
        DrawHorizontalIndices(g, pen, brush, startingX, textBoxHeight, horizontalCategories);
        DrawCellLines(g, pen, brush, startingX, startingY);
        DrawVerticalIndices(g, pen, brush, 0, startingY, verticalCategories);
        DrawVerticalCategories(g, pen, brush, startingX, startingY);

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

    private void DrawHorizontalCategories(Graphics g, Pen pen, Brush brush, int initialX)
    {
        var horizontalCategories = _core.HorizontalCategories;
        var textBoxHeight = _tb_activeCell.Height;
        var textBoxWidth = _tb_activeCell.Width;

        //calculate the ending x
        var totalHorizontalCellCount = horizontalCategories
            .Select(c => _core.CategoryToIndices[c].Count)
            .Aggregate((x, y) => x * y);
        var endingX = _pnl_Spreadsheet.Width;// initialX + totalHorizontalCellCount * textBoxWidth;
        //subtract the last from the first and we have the starting x
        var startingX = endingX - CategoryListWidth(horizontalCategories);
        //Note: Draw horizontal line separating these categories from spreadsheet:
        g.DrawLine(pen, new Point(0, textBoxHeight), new Point(endingX, textBoxHeight));
        //Note: Draw vertical lines:
        var varyingX = startingX;
        Point upperLeft = new(varyingX, 0);
        Point lowerRight = new(varyingX, textBoxHeight);
        DrawCategories(g, pen, brush, horizontalCategories, varyingX, upperLeft, lowerRight, AreaType.HorizontalCategories);
    }

    void DrawHorizontalIndices(Graphics g, Pen pen, Brush brush, int startingX, int startingY, List<string> categories)
    {
        /* for current category.
         *  use remaining categories to figure out width
         *  draw per each index of current category and for each call self recursively
         */

        var textBoxWidth = _tb_activeCell.Width;
        var textBoxHeight = _tb_activeCell.Height;

        var currentIndices = _core.CategoryToIndices[categories.First()];
        var remainingCategories = categories.Skip(1).ToList();
        int cellWidth;
        if (remainingCategories.Any())
        {
            var remainingCellCount = remainingCategories
                .Select(c => _core.CategoryToIndices[c].Count)
                .Aggregate((x, y) => x * y);
            cellWidth = remainingCellCount * textBoxWidth;
        }
        else
            cellWidth = textBoxWidth;

        //Note: Draw horizontal line:
        var horizontalLineLength = startingX + (cellWidth * currentIndices.Count);
        g.DrawLine(pen, new Point(startingX, startingY), new Point(horizontalLineLength, startingY));

        //Note: Draw vertical lines and write indices:
        var varyingX = startingX;
        var nextY = startingY + textBoxHeight;
        foreach (var index in currentIndices)
        {
            g.DrawLine(pen, new Point(varyingX, startingY), new Point(varyingX, startingY + textBoxHeight));
            g.DrawString(index, Font, brush, new Point(varyingX, startingY));

            if (remainingCategories.Any())
                DrawHorizontalIndices(g, pen, brush, varyingX, nextY, remainingCategories);
            varyingX += cellWidth;
        }
        g.DrawLine(pen, new Point(horizontalLineLength, startingY), new Point(horizontalLineLength, startingY + textBoxHeight));
    }

    void DrawVerticalIndices(Graphics g, Pen pen, Brush brush, int startingX, int startingY, List<string> categories)
    {
        /* for current category.
         *  use remaining categories to figure out width
         *  draw per each index of current category and for each call self recursively
         */

        var textBoxWidth = _tb_activeCell.Width;
        var textBoxHeight = _tb_activeCell.Height;

        var currentIndices = _core.CategoryToIndices[categories.First()];
        var remainingCategories = categories.Skip(1).ToList();
        int cellHeight;
        if (remainingCategories.Any())
        {
            var remainingCellCount = remainingCategories
                .Select(c => _core.CategoryToIndices[c].Count)
                .Aggregate((x, y) => x * y);
            cellHeight = remainingCellCount * textBoxHeight;
        }
        else
            cellHeight = textBoxHeight;

        //Note: Draw vertical line:
        var verticalLineLength = startingY + (cellHeight * currentIndices.Count);
        g.DrawLine(pen, new Point(startingX, startingY), new Point(startingX, verticalLineLength));

        //Note: Draw horizontal lines and write indices:
        var varyingY = startingY;
        var nextX = startingX + textBoxWidth;
        foreach (var index in currentIndices)
        {
            g.DrawLine(pen, new Point(startingX, varyingY), new Point(startingX + textBoxWidth, varyingY));
            g.DrawString(index, Font, brush, new Point(startingX, varyingY));

            if (remainingCategories.Any())
                DrawVerticalIndices(g, pen, brush, nextX, varyingY, remainingCategories);
            varyingY += cellHeight;
        }
        g.DrawLine(pen, new Point(startingX, verticalLineLength), new Point(startingX + textBoxWidth, verticalLineLength));
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

    void DrawVerticalCategories(Graphics g, Pen pen, Brush brush, int startingX, int initialY)
    {
        var verticalCategories = _core.VerticalCategories;
        var textBoxHeight = _tb_activeCell.Height;

        //calculate the ending x
        var totalVerticalCellCount = verticalCategories
            .Select(c => _core.CategoryToIndices[c].Count)
            .Aggregate((x, y) => x * y);
        var endingY = _pnl_Spreadsheet.Height;// startingY + textBoxHeight;
        var startingY = endingY - textBoxHeight;// initialY + totalVerticalCellCount * textBoxHeight;
        //calculate the width of the category list
        int catListWidth = CategoryListWidth(verticalCategories);
        //Note: Draw horizontal line separating these categories from the spreadsheet:
        g.DrawLine(pen, new Point(0, startingY), new Point(_pnl_Spreadsheet.Width, startingY));
        //Note: Draw vertical lines:
        var inititialX = 0;
        Point upperLeft = new(inititialX, startingY);
        Point lowerRight = new(inititialX, endingY);
        DrawCategories(g, pen, brush, verticalCategories, inititialX, upperLeft, lowerRight, AreaType.VerticalCategories);
    }

    private void DrawCategories(Graphics g,
                                Pen pen,
                                Brush brush,
                                List<string> categories,
                                int initialX,
                                Point upperLeft,
                                Point lowerRight,
                                AreaType areaType)
    {
        var textBoxWidth = _tb_activeCell.Width;
        var categoryListIndex = 0;
        var varyingX = initialX;
        Rectangle listCell;
        foreach (var category in categories)
        {
            //Note: Draw first line:
            g.DrawLine(pen, upperLeft, lowerRight);
            varyingX += textBoxWidth / 4;
            lowerRight.X = varyingX;

            //Note: Add point to drop cell:
            listCell = new(upperLeft.X, upperLeft.Y, lowerRight.X - upperLeft.X, lowerRight.Y - upperLeft.Y);
            _guiRectangleToArea.Add(listCell, new Categories(areaType, new DropCell(categoryListIndex)));
            categoryListIndex++;
            upperLeft.X = varyingX;

            //Note: Draw second line and category:
            g.DrawLine(pen, upperLeft, lowerRight);
            g.DrawString(category, Font, brush, upperLeft);

            //Note: Add point to category:
            varyingX += textBoxWidth;
            lowerRight.X = varyingX;
            listCell = new(upperLeft.X, upperLeft.Y, lowerRight.X - upperLeft.X, lowerRight.Y - upperLeft.Y);
            _guiRectangleToArea.Add(listCell, new Categories(areaType, new CategoryCell(category)));
            upperLeft.X = varyingX;
        }

        //Note: Draw penultimate line:
        g.DrawLine(pen, upperLeft, lowerRight);
        varyingX += textBoxWidth / 4;

        //Note: Add final point to drop cell:
        lowerRight.X = varyingX;
        listCell = new(upperLeft.X, upperLeft.Y, lowerRight.X - upperLeft.X, lowerRight.Y - upperLeft.Y);
        _guiRectangleToArea.Add(listCell, new Categories(areaType, new DropCell(categoryListIndex)));

        //Note: Draw final line:
        upperLeft.X = varyingX;
        g.DrawLine(pen, upperLeft, lowerRight);
    }

    int CategoryListWidth(List<string> categories)
    {
        var textBoxWidth = _tb_activeCell.Width;
        var nrOfCats = categories.Count;
        var catWidth = nrOfCats * textBoxWidth;
        var joinWidth = (nrOfCats + 1) * (textBoxWidth / 4);
        return catWidth + joinWidth;
    }
}