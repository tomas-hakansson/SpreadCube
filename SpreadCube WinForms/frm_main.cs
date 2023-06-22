using SpreadCube_Core;
using System.IO.Enumeration;

namespace SpreadCube_WinForms
{
    public partial class Frm_main : Form
    {
        List<TextBox> _cells = new();
        FlowLayoutPanel _flp_MainGui;
        Button _btn_Test;
        Panel _pnl_Spreadsheet;
        TextBox _tb_activeCell;

        BehindThe2DView _core;

        Dictionary<Rectangle, (AreaType areaType, object index)> _guiRectangleToBackendProperty;
        enum AreaType
        {
            HorizontalCategories,
            VerticalCategories,
            HorizontalIndices,
            VerticalIndices,
            Cells,
        }

        public Frm_main()
        {
            //Anders tips:
            //  ha en panel som huvudcontainer (för scrollbars) och rita kalkylbladet direkt i GDI+.
            //  bara ha *en* textbox som man flyttar omkring där den behövs.

            //initialize generated components:
            InitializeComponent();

            //2nd attempt:

            //  test data:
            /* need too handle:
             *  categories (dimensions?) can have one or more (vectors? length and direction.).
             *  cells
             *  formulas
             *  display order (think of better name)
             */


            // a flow layout panel frow top to bottom to contain or top level controls:
            _flp_MainGui = new FlowLayoutPanel();
            _flp_MainGui.Location = new Point(0, 0);
            //flp_MainGui.Size = new Size(Width, Height);
            _flp_MainGui.Name = nameof(_flp_MainGui);
            _flp_MainGui.AutoSize = true;
            //flp_MainGui.Dock = DockStyle.Fill;
            //Note: both AutoSize and DockStyle.Fill have the same effect in this case.
            //flp_MainGui.Size = new Size(100, 100);
            _flp_MainGui.TabIndex = 0;
            _flp_MainGui.FlowDirection = FlowDirection.TopDown;
            Controls.Add(_flp_MainGui);

            // a button for testing:
            _btn_Test = new Button();
            _btn_Test.Name = nameof(_btn_Test);
            _btn_Test.Text = "click to test";
            _btn_Test.TabIndex = 1;
            _btn_Test.AutoSize = true;
            _btn_Test.Click += new EventHandler(Btn_Test__Click);
            _flp_MainGui.Controls.Add(_btn_Test);

            //this.button1.Location = new System.Drawing.Point(294, 34);
            //this.button1.Name = "button1";
            //this.button1.Size = new System.Drawing.Size(112, 34);
            //this.button1.TabIndex = 1;
            //this.button1.Text = "button1";
            //this.button1.UseVisualStyleBackColor = true;
            //this.button1.Click += new System.EventHandler(this.button1_Click);

            // a panel that we can draw our spreadsheet on:

            //ToDo: figure out how to scroll bar.
            _pnl_Spreadsheet = new();
            _pnl_Spreadsheet.Name = nameof(_pnl_Spreadsheet);
            _pnl_Spreadsheet.TabIndex = 2;
            _pnl_Spreadsheet.Size = new Size(Width, Height);
            _pnl_Spreadsheet.Paint += new PaintEventHandler(Pnl_Spreadsheet__Paint);
            _pnl_Spreadsheet.MouseDown += new MouseEventHandler(Pnl_Spreadsheet__MouseDown);
            _pnl_Spreadsheet.MouseUp += new MouseEventHandler(Pnl_Spreadsheet__MouseUp);
            _flp_MainGui.Controls.Add(_pnl_Spreadsheet);

            // a textbox on our painted panel:

            _tb_activeCell = new();
            _tb_activeCell.Name = nameof(_tb_activeCell);
            _tb_activeCell.AutoSize = false;
            _tb_activeCell.Location = new Point(0, 0);
            //activeCell.Size = new Size(50, 100);
            _tb_activeCell.Visible = false; //ToDo: have this placed in the proper place, automatically.
            _tb_activeCell.LostFocus += new EventHandler(Tb_activeCell__LostFocus);
            _tb_activeCell.LocationChanged += new EventHandler(Tb_activeCell__LocationChanged);
            _pnl_Spreadsheet.Controls.Add(_tb_activeCell);

            // a table layout panel for our spreadsheet:
            //tlp_TextBoxes = new TableLayoutPanel();
            //tlp_TextBoxes.Name = nameof(tlp_TextBoxes);
            //tlp_TextBoxes.TabIndex = 2;
            //tlp_TextBoxes.AutoSize = true;
            //flp_MainGui.Controls.Add(tlp_TextBoxes);

            // our code behind:
            _core = new();
            //Category[] hc = _core.HorizontalCategories;
            //Category[] vc = _core.VerticalCategories;
            //Category[] hic = _core.HiddenCategories;
            //CellValue[,] cs = _core.VisibleCells;

            _guiRectangleToBackendProperty = new();

            //  cells:

            // add rows and columns to our table layout panel:

            //tlp_TextBoxes.ColumnCount = cs.GetLength(0) + 1;
            //for (int col = 0; col < tlp_TextBoxes.ColumnCount; col++)
            //{
            //    tlp_TextBoxes.ColumnStyles.Add(new ColumnStyle());
            //}
            //tlp_TextBoxes.RowCount = cs.GetLength(1) + 1;
            //for (int row = 0; row < tlp_TextBoxes.RowCount; row++)
            //{
            //    tlp_TextBoxes.RowStyles.Add(new RowStyle());
            //}
            //TextBox insteadOfNothing = new();
            ////insteadOfNothing.Visible = false;
            //tlp_TextBoxes.Controls.Add(insteadOfNothing, 0, 0);
            //for (int col = 1; col < tlp_TextBoxes.ColumnCount; col++)
            //{
            //    TextBox tb = new();
            //    tb.Text = core.HorizontalCategories[0].Vectors[col - 1];
            //    tlp_TextBoxes.Controls.Add(tb, col, 0);
            //}

            //for (int row = 1; row < tlp_TextBoxes.RowCount; row++)
            //{
            //    TextBox tb = new();
            //    tb.Text = core.VerticalCategories[0].Vectors[row - 1];
            //    tlp_TextBoxes.Controls.Add(tb, 0, row);
            //}

            //for (int row = 0; row < cs.GetLength(1); row++)
            //{
            //    for (int col = 0; col < cs.GetLength(0); col++)
            //    {
            //        TextBox tb = new();
            //        tb.Text = cs[col, row].TextContent;
            //        //source: https://stackoverflow.com/questions/1730961/convert-a-2d-array-index-into-a-1d-index
            //        _cells.Add(tb);
            //        tlp_TextBoxes.Controls.Add(tb);
            //        //tlp_TextBoxes.Controls.Add(tb, col, row);
            //    }
            //}
        }

        void Btn_Test__Click(object? sender, EventArgs e)
        {
            MessageBox.Show(_tb_activeCell.Text);
        }

        void Pnl_Spreadsheet__Paint(object? sender, PaintEventArgs e)
        {
            PaintSpreadsheet();
        }

        string _draggedCategory;

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

            var matches = _guiRectangleToBackendProperty.Where(pp => pp.Key.Contains(e.Location));
            if (matches.Any())
            {
                if (matches.Count() != 1)
                    throw new Exception("There can only be one!");
                var (areaType, anObject) = matches.First().Value;
                if (areaType == AreaType.HorizontalCategories || areaType == AreaType.VerticalCategories)
                {
                    var at = areaType;
                    if (anObject is (CategoryListType, string))
                    {
                        var pair = anObject as (CategoryListType, string)?;
                        if (pair != null)
                        {
                            var (clt, category) = pair.Value;
                            if (clt == CategoryListType.Category)
                                _draggedCategory = category;
                        }
                    }
                }
                else
                    throw new NotImplementedException("I'll get to this. Don't rush me!");
            }
        }

        void Pnl_Spreadsheet__MouseUp(object? sender, MouseEventArgs e)
        {
            var matches = _guiRectangleToBackendProperty.Where(pp => pp.Key.Contains(e.Location));
            if (matches.Any())
            {
                if (matches.Count() != 1)
                    throw new Exception("There can only be one!");
                var (areaType, anObject) = matches.First().Value;
                if (!string.IsNullOrWhiteSpace(_draggedCategory) &&
                    (areaType == AreaType.HorizontalCategories || areaType == AreaType.VerticalCategories))
                {
                    if (anObject is (CategoryListType, int))
                    {
                        var pair = anObject as (CategoryListType, int)?;
                        if (pair != null)
                        {
                            var (categoryListType, categoryIndex) = pair.Value;
                            if (categoryListType == CategoryListType.DropCell)
                            {
                                if (areaType == AreaType.HorizontalCategories)
                                    MoveCategory(categoryIndex, areaType);
                                else if (areaType == AreaType.VerticalCategories)
                                    MoveCategory(categoryIndex, areaType);
                                Refresh();
                            }
                        }
                    }
                }
                else
                    throw new NotImplementedException("I'll get to this, don't rush me!");
            }
            _draggedCategory = string.Empty;
        }

        void MoveCategory(int newIndex, AreaType receivingArea)
        {
            if (string.IsNullOrWhiteSpace(_draggedCategory))
                return;

            List<string> oldList;
            List<string> newList;
            if (receivingArea == AreaType.HorizontalCategories)
            {
                oldList = _core.VerticalCategories;
                newList = _core.HorizontalCategories;
            }
            else
            {
                oldList = _core.HorizontalCategories;
                newList = _core.VerticalCategories;
            }

            if (newList.Contains(_draggedCategory))
            {
                var oldIndex = newList.IndexOf(_draggedCategory);
                if (oldIndex == newIndex)
                    return;

                newList.RemoveAt(oldIndex);
                if (oldIndex < newIndex)
                    newIndex--;
                newList.Insert(newIndex, _draggedCategory);
            }
            else
            {
                oldList.Remove(_draggedCategory);
                newList.Insert(newIndex, _draggedCategory);
            }
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
                previousTextBoxLocation = tb.Location;// Add(tb.Location, tb.GetPositionFromCharIndex(0));
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

        private void PaintSpreadsheet()
        {
            var p = _pnl_Spreadsheet;
            var g = p.CreateGraphics();
            g.Clear(BackColor);
            _guiRectangleToBackendProperty.Clear();
            Pen pen = new(Color.Black);
            Brush brush = new SolidBrush(Color.Black);

            var horizontalCategories = _core.HorizontalCategories;
            var verticalCategories = _core.VerticalCategories;

            var textBoxHeight = _tb_activeCell.Height;

            var startingX = verticalCategories.Count * _tb_activeCell.Width;
            var startingY = textBoxHeight + horizontalCategories.Count * textBoxHeight;

            DrawHorizontalCategories(g, pen, brush, startingX);
            DrawHorizontalIndices(g, pen, brush, startingX, textBoxHeight, horizontalCategories);
            DrawVerticalIndices(g, pen, brush, 0, startingY, verticalCategories);
            DrawCellLines(g, pen, brush, startingX, startingY);
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

        enum CategoryListType
        {
            Category,
            DropCell
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
            var endingX = initialX + totalHorizontalCellCount * textBoxWidth;
            //subtract the last from the first and we have the starting x
            var startingX = endingX - CategoryListWidth(horizontalCategories);
            //Note: Draw horizontal line:
            g.DrawLine(pen, new Point(startingX, 0), new Point(endingX, 0));
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
            var startingY = initialY + totalVerticalCellCount * textBoxHeight;
            var endingY = startingY + textBoxHeight;
            //calculate the width of the category list
            int catListWidth = CategoryListWidth(verticalCategories);
            //Note: Draw horizontal line:
            g.DrawLine(pen, new Point(0, endingY), new Point(catListWidth, endingY));
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
                _guiRectangleToBackendProperty.Add(listCell, (areaType, (CategoryListType.DropCell, categoryListIndex)));
                categoryListIndex++;
                upperLeft.X = varyingX;

                //Note: Draw second line and category:
                g.DrawLine(pen, upperLeft, lowerRight);
                g.DrawString(category, Font, brush, upperLeft);

                //Note: Add point to category:
                varyingX += textBoxWidth;
                lowerRight.X = varyingX;
                listCell = new(upperLeft.X, upperLeft.Y, lowerRight.X - upperLeft.X, lowerRight.Y - upperLeft.Y);
                _guiRectangleToBackendProperty.Add(listCell, (areaType, (CategoryListType.Category, category)));
                upperLeft.X = varyingX;
            }

            //Note: Draw penultimate line:
            g.DrawLine(pen, upperLeft, lowerRight);
            varyingX += textBoxWidth / 4;

            //Note: Add final point to drop cell:
            lowerRight.X = varyingX;
            listCell = new(upperLeft.X, upperLeft.Y, lowerRight.X - upperLeft.X, lowerRight.Y - upperLeft.Y);
            _guiRectangleToBackendProperty.Add(listCell, (areaType, (CategoryListType.DropCell, categoryListIndex)));

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
}