using SpreadCube_Core;
using System.IO.Enumeration;

namespace SpreadCube_WinForms
{
    public partial class Frm_main : Form
    {
        List<TextBox> _cells = new();
        FlowLayoutPanel flp_MainGui;
        Button btn_Test;
        Panel pnl_Spreadsheet;
        TextBox tb_activeCell;
        //TableLayoutPanel tlp_TextBoxes;

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
            flp_MainGui = new FlowLayoutPanel();
            flp_MainGui.Location = new Point(0, 0);
            //flp_MainGui.Size = new Size(Width, Height);
            flp_MainGui.Name = nameof(flp_MainGui);
            flp_MainGui.AutoSize = true;
            //flp_MainGui.Dock = DockStyle.Fill;
            //Note: both AutoSize and DockStyle.Fill have the same effect in this case.
            //flp_MainGui.Size = new Size(100, 100);
            flp_MainGui.TabIndex = 0;
            flp_MainGui.FlowDirection = FlowDirection.TopDown;
            Controls.Add(flp_MainGui);

            // a button for testing:
            btn_Test = new Button();
            btn_Test.Name = nameof(btn_Test);
            btn_Test.Text = "click to test";
            btn_Test.TabIndex = 1;
            btn_Test.AutoSize = true;
            btn_Test.Click += new EventHandler(Btn_Test__Click);
            flp_MainGui.Controls.Add(btn_Test);

            //this.button1.Location = new System.Drawing.Point(294, 34);
            //this.button1.Name = "button1";
            //this.button1.Size = new System.Drawing.Size(112, 34);
            //this.button1.TabIndex = 1;
            //this.button1.Text = "button1";
            //this.button1.UseVisualStyleBackColor = true;
            //this.button1.Click += new System.EventHandler(this.button1_Click);

            // a panel that we can draw our spreadsheet on:

            pnl_Spreadsheet = new();
            pnl_Spreadsheet.Name = nameof(pnl_Spreadsheet);
            pnl_Spreadsheet.TabIndex = 2;
            pnl_Spreadsheet.Size = new Size(Width, Height);
            pnl_Spreadsheet.Paint += new PaintEventHandler(Pnl_Spreadsheet__Paint);
            pnl_Spreadsheet.MouseDown += new MouseEventHandler(Pnl_Spreadsheet__MouseDown);
            flp_MainGui.Controls.Add(pnl_Spreadsheet);

            // a textbox on our painted panel:

            tb_activeCell = new();
            tb_activeCell.Name = nameof(tb_activeCell);
            tb_activeCell.AutoSize = false;
            tb_activeCell.Location = new Point(0, 0);
            //activeCell.Size = new Size(50, 100);
            //activeCell.Visible = false;
            tb_activeCell.LostFocus += new EventHandler(Tb_activeCell__LostFocus);
            tb_activeCell.LocationChanged += new EventHandler(Tb_activeCell__LocationChanged);
            pnl_Spreadsheet.Controls.Add(tb_activeCell);

            // a table layout panel for our spreadsheet:
            //tlp_TextBoxes = new TableLayoutPanel();
            //tlp_TextBoxes.Name = nameof(tlp_TextBoxes);
            //tlp_TextBoxes.TabIndex = 2;
            //tlp_TextBoxes.AutoSize = true;
            //flp_MainGui.Controls.Add(tlp_TextBoxes);

            // our code behind:
            BehindThe2DView core = new();
            Category[] hc = core.HorizontalCategories;
            Category[] vc = core.VerticalCategories;
            Category[] hic = core.HiddenCategories;
            Cell[,] cs = core.VisibleCells;

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
            MessageBox.Show(tb_activeCell.Text);
        }

        void Pnl_Spreadsheet__Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel p)
                return;
            var g = e.Graphics;

            Pen penBlack = new(Color.Black);

            //evenHeight is the maximal height, that is evenly divisible by cell height, that can fit inside the panel.
            var evenHeight = p.Height - p.Height % tb_activeCell.Height;
            // see evenHeight comment
            var evenWidth = p.Width - p.Width % tb_activeCell.Width;

            var nrOfHorizontalLines = evenHeight / tb_activeCell.Height;
            var height = 0;
            for (int i = 0; i <= nrOfHorizontalLines; i++)
            {
                g.DrawLine(penBlack, new Point(0, height), new Point(evenWidth, height));
                height += tb_activeCell.Height;
            }

            var nrOfVerticalLines = evenWidth / tb_activeCell.Width;
            var width = 0;
            for (int i = 0; i <= nrOfVerticalLines; i++)
            {
                g.DrawLine(penBlack, new Point(width, 0), new Point(width, evenHeight));
                width += tb_activeCell.Width;
            }
        }

        void Pnl_Spreadsheet__MouseDown(object? sender, MouseEventArgs e)
        {
            //evenHeight is the maximal height, that is evenly divisible by cell height, that can fit inside the panel.
            int pHeight = pnl_Spreadsheet.Height;
            var evenHeight = pHeight - pHeight % tb_activeCell.Height;
            //if the mouse cursor is outside of the grid:
            if (e.Y > evenHeight)
                return;
            // see evenHeight comment
            int pWidth = pnl_Spreadsheet.Width;
            var evenWidth = pWidth - pWidth % tb_activeCell.Width;
            //if the mouse cursor is outside of the grid:
            if (e.X > evenWidth)
                return;
            //Figure out the X and Y of the containing cell:
            var xCoord = e.X - e.X % tb_activeCell.Width;
            var yCoord = e.Y - e.Y % tb_activeCell.Height;
            //Move the textbox there and turn it visible:
            tb_activeCell.Visible = false;
            tb_activeCell.Location = new Point(xCoord, yCoord);
            tb_activeCell.Clear();
            tb_activeCell.Visible = true;
            tb_activeCell.Focus();
        }

        string previousText = string.Empty;
        Point previousTextLocation;
        void Tb_activeCell__LostFocus(object? sender, EventArgs e)
        {
            if (sender is not TextBox tb)
                return;
            if (!string.IsNullOrWhiteSpace(tb.Text))
            {
                previousText = tb.Text;
                previousTextLocation = Add(tb.Location, tb.GetPositionFromCharIndex(0));
            }
        }

        void Tb_activeCell__LocationChanged(object? sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(previousText))
            {
                var pg = pnl_Spreadsheet.CreateGraphics();
                Brush brush = new SolidBrush(Color.Black);
                pg.DrawString(previousText, Font, brush, previousTextLocation);
                previousText = string.Empty;
            }
        }

        Point Add(Point a, Point b) =>
            new Point(a.X + b.X, a.Y + b.Y);
    }
}