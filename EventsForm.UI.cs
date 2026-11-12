namespace MunicipalServicesApplication
{
    public partial class EventsForm : Form
    {
        // Use the same colours as your Part 1 forms so the app looks consistent.
        private static readonly Color PrimaryColor = Color.FromArgb(0, 102, 102);
        private static readonly Color BackgroundColor = Color.FromArgb(245, 248, 250);
        private static readonly Color PanelTextColor = Color.Black;

        // Controls
        private ComboBox cmbCategory = null!;
        private CheckBox chkDate = null!;
        private DateTimePicker dtpDate = null!;
        private TextBox txtKeyword = null!;
        private Button btnSearch = null!;
        private Button btnClear = null!;
        private Button btnPrevious = null!;
        private Button btnBack = null!;
        private ListView lvEvents = null!;
        private RichTextBox rtbDetails = null!;
        private ListBox lstUrgent = null!;
        private ListBox lstNextUp = null!;
        private ListBox lstRecommended = null!;
        private Label lblStatus = null!;
        private Label lblDates = null!;
        private Label lblHistory = null!;

        private void BuildUi()
        {
            Text = "Local Events and Announcements";
            Size = new Size(1100, 740);
            MinimumSize = new Size(900, 620);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = BackgroundColor;
            Font = new Font("Segoe UI", 10F);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(12)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));    // title
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 105));   // search
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));    // content
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));    // status
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));    // footer
            Controls.Add(root);

            var lblTitle = new Label
            {
                Text = "Local Events and Announcements",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = PrimaryColor,
                TextAlign = ContentAlignment.MiddleLeft
            };

            root.Controls.Add(lblTitle, 0, 0);
            root.Controls.Add(BuildSearchPanel(), 0, 1);
            root.Controls.Add(BuildContentPanel(), 0, 2);
            root.Controls.Add(BuildStatusPanel(), 0, 3);
            root.Controls.Add(BuildFooterPanel(), 0, 4);
        }

        private Control BuildSearchPanel()
        {
            var group = new GroupBox
            {
                Text = "Search events",
                Dock = DockStyle.Fill,
                ForeColor = PrimaryColor,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = true,
                ForeColor = PanelTextColor,
                Font = new Font("Segoe UI", 10F)
            };
            group.Controls.Add(flow);

            flow.Controls.Add(MakeLabel("Category:"));
            cmbCategory = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
            flow.Controls.Add(cmbCategory);

            chkDate = new CheckBox { Text = "Date:", AutoSize = true, Margin = new Padding(15, 8, 3, 3) };
            flow.Controls.Add(chkDate);
            dtpDate = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 110, Enabled = false };
            flow.Controls.Add(dtpDate);

            flow.Controls.Add(MakeLabel("Keyword:"));
            txtKeyword = new TextBox { Width = 170 };
            flow.Controls.Add(txtKeyword);

            btnSearch = MakeButton("Search", 85);
            btnClear = MakeButton("Clear", 85);
            btnPrevious = MakeButton("Previous Search", 135);
            flow.Controls.Add(btnSearch);
            flow.Controls.Add(btnClear);
            flow.Controls.Add(btnPrevious);

            return group;
        }

        private Control BuildContentPanel()
        {
            var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));

            // Left: results list and details
            var left = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 65));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 35));

            lvEvents = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false,
                HideSelection = false
            };
            lvEvents.Columns.Add("Date", 100);
            lvEvents.Columns.Add("Type", 85);
            lvEvents.Columns.Add("Title", 240);
            lvEvents.Columns.Add("Category", 100);
            lvEvents.Columns.Add("Location", 160);

            rtbDetails = new RichTextBox { ReadOnly = true, BackColor = Color.White, BorderStyle = BorderStyle.None };

            left.Controls.Add(MakeGroup("Upcoming events and announcements", lvEvents), 0, 0);
            left.Controls.Add(MakeGroup("Details", rtbDetails), 0, 1);

            // Right: urgent notices, next up, recommendations
            var right = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 34));

            lstUrgent = MakeListBox();
            lstNextUp = MakeListBox();
            lstRecommended = MakeListBox();

            right.Controls.Add(MakeGroup("Urgent announcements", lstUrgent), 0, 0);
            right.Controls.Add(MakeGroup("Next up", lstNextUp), 0, 1);
            right.Controls.Add(MakeGroup("Recommended for you", lstRecommended), 0, 2);

            content.Controls.Add(left, 0, 0);
            content.Controls.Add(right, 1, 0);
            return content;
        }

        private Control BuildStatusPanel()
        {
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 34));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 33));

            lblStatus = MakeInfoLabel(PrimaryColor, FontStyle.Bold);
            lblDates = MakeInfoLabel(Color.DimGray, FontStyle.Regular);
            lblHistory = MakeInfoLabel(Color.DimGray, FontStyle.Regular);

            panel.Controls.Add(lblStatus, 0, 0);
            panel.Controls.Add(lblDates, 0, 1);
            panel.Controls.Add(lblHistory, 0, 2);
            return panel;
        }

        private Control BuildFooterPanel()
        {
            var footer = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            btnBack = MakeButton("Back to Main Menu", 170);
            footer.Controls.Add(btnBack);
            return footer;
        }

        // ---------- small helpers so the controls look consistent ----------
        private static Label MakeLabel(string text)
        {
            return new Label { Text = text, AutoSize = true, Margin = new Padding(12, 8, 3, 3) };
        }

        private static Label MakeInfoLabel(Color color, FontStyle style)
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = color,
                Font = new Font("Segoe UI", 9.5F, style),
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private static Button MakeButton(string text, int width)
        {
            var button = new Button
            {
                Text = text,
                Width = width,
                Height = 32,
                BackColor = PrimaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(8, 3, 3, 3)
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private static ListBox MakeListBox()
        {
            return new ListBox
            {
                HorizontalScrollbar = true,
                IntegralHeight = false,
                BorderStyle = BorderStyle.None
            };
        }

        private static GroupBox MakeGroup(string title, Control child)
        {
            var group = new GroupBox
            {
                Text = title,
                Dock = DockStyle.Fill,
                ForeColor = PrimaryColor,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Padding = new Padding(6)
            };
            child.Dock = DockStyle.Fill;
            child.ForeColor = PanelTextColor;
            child.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            group.Controls.Add(child);
            return group;
        }
    }
}