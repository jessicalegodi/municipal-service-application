using MunicipalServicesApplication;

namespace MunicipalServicesApplication
{
    public class MainMenuForm : Form
    {
        private static readonly Color PrimaryColor = Color.FromArgb(0, 102, 102);
        private static readonly Color BackgroundColor = Color.FromArgb(245, 248, 250);

        private readonly EventManager _eventManager = new EventManager();
        private readonly ServiceRequestManager _requestManager = new ServiceRequestManager();
        public MainMenuForm()
        {
            Text = "Municipal Services";
            Size = new Size(520, 560);
            MinimumSize = new Size(420, 480);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = BackgroundColor;
            Font = new Font("Segoe UI", 11F);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(30)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 5; i++)
            {
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
            }
            Controls.Add(layout);

            var title = new Label
            {
                Text = "Municipal Services",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = PrimaryColor,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Button btnReport = MakeMenuButton("Report Issues", true);
            Button btnEvents = MakeMenuButton("Local Events and Announcements", true);
            Button btnStatus = MakeMenuButton("Service Request Status", true);
            Button btnExit = MakeMenuButton("Exit", true);

            btnReport.Click += BtnReport_Click;
            btnEvents.Click += BtnEvents_Click;
            btnStatus.Click += BtnStatus_Click;
            btnExit.Click += (s, e) => Close();

            layout.Controls.Add(title, 0, 0);
            layout.Controls.Add(btnReport, 0, 1);
            layout.Controls.Add(btnEvents, 0, 2);
            layout.Controls.Add(btnStatus, 0, 3);
            layout.Controls.Add(btnExit, 0, 4);
        }

        private static Button MakeMenuButton(string text, bool enabled)
        {
            var button = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 8, 0, 8),
                Enabled = enabled,
                FlatStyle = FlatStyle.Flat,
                BackColor = enabled ? PrimaryColor : Color.Silver,
                ForeColor = Color.White
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private void BtnReport_Click(object? sender, EventArgs e)
        {
            Hide();
            using (var reportForm = new ReportIssuesForm())   // your Part 1 form
            {
                reportForm.ShowDialog();
            }
            Show();
        }

        private void BtnEvents_Click(object? sender, EventArgs e)
        {
            Hide();
            using (var eventsForm = new EventsForm(_eventManager))
            {
                eventsForm.ShowDialog();
            }
            Show();
        }
        private void BtnStatus_Click(object? sender, EventArgs e)
        {
            Hide();
            using (var statusForm = new ServiceRequestStatusForm(_requestManager))
            {
                statusForm.ShowDialog();
            }
            Show();
        }
    }
}
