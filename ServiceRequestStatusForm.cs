namespace MunicipalServicesApplication
{
    public partial class ServiceRequestStatusForm : Form
    {
        private readonly ServiceRequestManager _manager;
        private List<ServiceRequest> _urgentRequests = new();
        private bool _loading;   // stops the selection handler firing while the list is rebuilt

        public ServiceRequestStatusForm(ServiceRequestManager manager)
        {
            _manager = manager;
            BuildUi();
            WireEvents();
            LoadRequests();
            RefreshUrgent();
        }

        private void WireEvents()
        {
            btnTrack.Click += BtnTrack_Click;
            btnShowAll.Click += BtnShowAll_Click;
            btnBack.Click += (s, e) => Close();   // the Main Menu shows itself again when this closes
            lvRequests.SelectedIndexChanged += LvRequests_SelectedIndexChanged;
            lstUrgent.SelectedIndexChanged += LstUrgent_SelectedIndexChanged;
            AcceptButton = btnTrack;              // pressing Enter tracks the request
        }

        // ------------------------------------------------------------------
        // Loading the lists
        // ------------------------------------------------------------------
        private void LoadRequests()
        {
            _loading = true;
            lvRequests.BeginUpdate();
            lvRequests.Items.Clear();

            foreach (ServiceRequest request in _manager.GetAll())
            {
                var item = new ListViewItem(request.Reference);
                item.SubItems.Add(request.Title);
                item.SubItems.Add(request.Category);
                item.SubItems.Add(request.Ward);
                item.SubItems.Add(request.Status);
                item.SubItems.Add(PriorityText(request.Priority));
                item.Tag = request;

                if (request.Status == "Resolved")
                {
                    item.ForeColor = Color.SeaGreen;
                }
                else if (request.Priority == 1)
                {
                    item.ForeColor = Color.Firebrick;   // urgent and not yet resolved
                }
                lvRequests.Items.Add(item);
            }

            lvRequests.EndUpdate();
            _loading = false;

            rtbDetails.Clear();
            progressStatus.Value = 0;
            lblStatus.Text = $"Showing all {_manager.Count} service requests, sorted by ID using the binary search tree.";
        }

        private void RefreshUrgent()
        {
            _urgentRequests = _manager.GetMostUrgent(5);   // comes from the heap
            lstUrgent.Items.Clear();
            foreach (ServiceRequest request in _urgentRequests)
            {
                lstUrgent.Items.Add($"{request.Reference} - {request.Title} ({request.Status})");
            }
        }

        // ------------------------------------------------------------------
        // Tracking
        // ------------------------------------------------------------------
        private void BtnTrack_Click(object? sender, EventArgs e)
        {
            string text = txtRequestId.Text.Trim().ToUpper().Replace("SR-", "");

            if (!int.TryParse(text, out int id))
            {
                MessageBox.Show("Enter the request number, for example 1005.",
                    "Track request", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ServiceRequest? request = _manager.Track(id);   // binary search tree lookup
            if (request == null)
            {
                lblStatus.Text = $"No request found with ID {id}.";
                MessageBox.Show($"No request found with ID {id}. Check the number and try again.",
                    "Track request", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ShowDetails(request);
            SelectInList(request);
            lblStatus.Text = $"Found {request.Reference}: {request.Status}.";
        }

        private void BtnShowAll_Click(object? sender, EventArgs e)
        {
            txtRequestId.Clear();
            LoadRequests();
        }

        private void SelectInList(ServiceRequest request)
        {
            foreach (ListViewItem item in lvRequests.Items)
            {
                if (item.Tag is ServiceRequest candidate && candidate.Id == request.Id)
                {
                    item.Selected = true;
                    item.EnsureVisible();
                    break;
                }
            }
        }

        // ------------------------------------------------------------------
        // Details and progress
        // ------------------------------------------------------------------
        private void ShowDetails(ServiceRequest request)
        {
            rtbDetails.Clear();
            AppendDetail(request.Reference + ": " + request.Title, true);
            AppendDetail($"Category: {request.Category}", false);
            AppendDetail($"Ward: {request.Ward}", false);
            AppendDetail($"Status: {request.Status}", false);
            AppendDetail($"Priority: {PriorityText(request.Priority)}", false);
            AppendDetail($"Submitted: {request.DateSubmitted:dd MMMM yyyy}", false);

            progressStatus.Value = request.Status switch
            {
                "Submitted" => 33,
                "In Progress" => 66,
                "Resolved" => 100,
                _ => 0
            };
        }

        private void AppendDetail(string text, bool bold)
        {
            rtbDetails.SelectionStart = rtbDetails.TextLength;
            rtbDetails.SelectionLength = 0;
            rtbDetails.SelectionFont = new Font(rtbDetails.Font, bold ? FontStyle.Bold : FontStyle.Regular);
            rtbDetails.AppendText(text + Environment.NewLine);
        }

        private static string PriorityText(int priority)
        {
            return priority switch
            {
                1 => "Urgent",
                2 => "Important",
                _ => "Normal"
            };
        }

        private void LvRequests_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_loading || lvRequests.SelectedItems.Count == 0)
            {
                return;
            }
            if (lvRequests.SelectedItems[0].Tag is ServiceRequest request)
            {
                ShowDetails(request);
            }
        }

        private void LstUrgent_SelectedIndexChanged(object? sender, EventArgs e)
        {
            int index = lstUrgent.SelectedIndex;
            if (index >= 0 && index < _urgentRequests.Count)
            {
                ShowDetails(_urgentRequests[index]);
            }
        }
    }
}