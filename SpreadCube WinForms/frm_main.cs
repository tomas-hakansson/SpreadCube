using SpreadCube_Core;
using System.IO.Enumeration;

namespace SpreadCube_WinForms
{
    public partial class frm_main : Form
    {
        List<TextBox> _cells = new();
        FlowLayoutPanel flp_MainGui;
        Button btn_Test;
        Panel pnl_Spreadsheet;
        TextBox activeCell;
        //TableLayoutPanel tlp_TextBoxes;

        public frm_main()
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
            btn_Test.Click += new EventHandler(btn_Test__Click);
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
            pnl_Spreadsheet.Paint += new PaintEventHandler(pnl_Spreadsheet__Paint);
            pnl_Spreadsheet.MouseDown += new MouseEventHandler(pnl_Spreadsheet__MouseDown);
            flp_MainGui.Controls.Add(pnl_Spreadsheet);

            // a textbox on our painted panel:

            activeCell = new();
            activeCell.Name = nameof(activeCell);
            activeCell.AutoSize = false;
            activeCell.Location = new Point(0, 0);
            //activeCell.Size = new Size(50, 100);
            //activeCell.Visible = false;
            pnl_Spreadsheet.Controls.Add(activeCell);

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

        void btn_Test__Click(object? sender, EventArgs e)
        {
            MessageBox.Show(activeCell.Text);
        }

        void pnl_Spreadsheet__Paint(object? sender, PaintEventArgs e)
        {
            var p = sender as Panel;
            if (p == null)
                return;
            var g = e.Graphics;

            Pen penBlack = new Pen(Color.Black);

            var horizontalLines = p.Height / activeCell.Height;
            var height = 0;
            for (int i = 0; i < horizontalLines; i++)
            {
                g.DrawLine(penBlack, new Point(0, height), new Point(p.Width, height));
                height += activeCell.Height;
            }

            var verticalLines = p.Width / activeCell.Width + 2;
            var width = 0;
            for (int i = 0; i < verticalLines; i++)
            {
                g.DrawLine(penBlack, new Point(width, 0), new Point(width, p.Height));
                width += activeCell.Width;
            }
        }

        void pnl_Spreadsheet__MouseDown(object? sender, MouseEventArgs e)
        {
            //Figure out the X and Y of the containing cell:
            var xCoord = e.X - e.X % activeCell.Width;
            var yCoord = e.Y - e.Y % activeCell.Height;
            //Move the textbox there and turn it visible:
            activeCell.Visible = false;
            Point newTBlocation = new Point(xCoord, yCoord);
            activeCell.Location = newTBlocation;
            activeCell.Clear();
            activeCell.Visible = true;
            activeCell.Focus();
        }
    }
}