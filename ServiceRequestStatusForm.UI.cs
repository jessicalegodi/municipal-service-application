namespace MunicipalServicesApplication
{
    public partial class ServiceRequestStatusForm : Form
    {
        private static readonly Color PrimaryColor = Color.FromArgb(0, 102, 102);
        private static readonly Color BackgroundColor = Color.FromArgb(245, 248, 250);
        private static readonly Color PanelTextColor = Color.Black;

        private TextBox txtRequestId = null!;
        private Button btnTrack = null!;
        private Button btnShowAll = null!;
        private Button btnBack = null!;
        private ListView lvRequests = null!;
        private RichTextBox rtbDetails = null!;
        private ProgressBar progressStatus = null!;
        private ListBox lstUrgent = null!;
        private Label lblStatus = null!;

        private void BuildUi()
        {
            Text = "Service Request Status";
            Size = new Size(1100, 720);
            MinimumSize = new Size(900, 600);
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
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 75));    // tracking
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));    // content
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));    // status line
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));    // footer
            Controls.Add(root);

            var lblTitle = new Label
            {
                Text = "Service Request Status",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = PrimaryColor,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblStatus = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = PrimaryColor,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            root.Controls.Add(lblTitle, 0, 0);
            root.Controls.Add(BuildTrackPanel(), 0, 1);
            root.Controls.Add(BuildContentPanel(), 0, 2);
            root.Controls.Add(lblStatus, 0, 3);
            root.Controls.Add(BuildFooterPanel(), 0, 4);
        }

        private Control BuildTrackPanel()
        {
            var group = new GroupBox
            {
                Text = "Track a request by its ID",
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

            flow.Controls.Add(new Label { Text = "Request ID:", AutoSize = true, Margin = new Padding(12, 8, 3, 3) });
            txtRequestId = new TextBox { Width = 140, PlaceholderText = "for example 1005" };
            flow.Controls.Add(txtRequestId);

            btnTrack = MakeButton("Track", 90);
            btnShowAll = MakeButton("Show all", 100);
            flow.Controls.Add(btnTrack);
            flow.Controls.Add(btnShowAll);
            return group;
        }

        private Control BuildContentPanel()
        {
            var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));

            lvRequests = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false,
                HideSelection = false
            };
            lvRequests.Columns.Add("Reference", 85);
            lvRequests.Columns.Add("Title", 230);
            lvRequests.Columns.Add("Category", 95);
            lvRequests.Columns.Add("Ward", 70);
            lvRequests.Columns.Add("Status", 95);
            lvRequests.Columns.Add("Priority", 70);

            var right = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 42));

            // Details text with a progress bar underneath
            var detailsLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            rtbDetails = new RichTextBox
            {
                ReadOnly = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill
            };
            progressStatus = new ProgressBar { Dock = DockStyle.Fill, Minimum = 0, Maximum = 100 };
            detailsLayout.Controls.Add(rtbDetails, 0, 0);
            detailsLayout.Controls.Add(progressStatus, 0, 1);

            lstUrgent = new ListBox
            {
                HorizontalScrollbar = true,
                IntegralHeight = false,
                BorderStyle = BorderStyle.None
            };

            right.Controls.Add(MakeGroup("Tracking details", detailsLayout), 0, 0);
            right.Controls.Add(MakeGroup("Most urgent requests", lstUrgent), 0, 1);

            content.Controls.Add(MakeGroup("Submitted service requests", lvRequests), 0, 0);
            content.Controls.Add(right, 1, 0);
            return content;
        }

        private Control BuildFooterPanel()
        {
            var footer = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            btnBack = MakeButton("Back to Main Menu", 170);
            footer.Controls.Add(btnBack);
            return footer;
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