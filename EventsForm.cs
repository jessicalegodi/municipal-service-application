namespace MunicipalServicesApplication
{
    public partial class EventsForm : Form
    {
        private const string AllCategories = "All categories";

        private readonly EventManager _manager;
        private List<Event> _currentResults = new();
        private bool _loading;   // stops the selection handler firing while the list is rebuilt

        public EventsForm(EventManager manager)
        {
            _manager = manager;
            BuildUi();
            WireEvents();
            LoadInitialData();
        }

        // ------------------------------------------------------------------
        // Setup
        // ------------------------------------------------------------------
        private void WireEvents()
        {
            btnSearch.Click += BtnSearch_Click;
            btnClear.Click += BtnClear_Click;
            btnPrevious.Click += BtnPrevious_Click;
            btnBack.Click += BtnBack_Click;

            chkDate.CheckedChanged += (s, e) => dtpDate.Enabled = chkDate.Checked;

            lvEvents.SelectedIndexChanged += LvEvents_SelectedIndexChanged;
            lstUrgent.SelectedIndexChanged += ListBox_SelectedIndexChanged;
            lstNextUp.SelectedIndexChanged += ListBox_SelectedIndexChanged;
            lstRecommended.SelectedIndexChanged += ListBox_SelectedIndexChanged;

            AcceptButton = btnSearch;   // pressing Enter runs the search
        }

        private void LoadInitialData()
        {
            // Categories come from a HashSet, so there are no duplicates.
            cmbCategory.Items.Add(AllCategories);
            foreach (string category in _manager.GetCategories())
            {
                cmbCategory.Items.Add(category);
            }
            cmbCategory.SelectedIndex = 0;

            // Dates come from a SortedSet, so the first one is the soonest.
            List<DateTime> nextDates = _manager.GetUpcomingDates(6);
            if (nextDates.Count > 0)
            {
                dtpDate.Value = nextDates[0];
            }
            lblDates.Text = "Dates with events: " + string.Join(", ", nextDates.Select(d => d.ToString("dd MMM")));

            RefreshSidePanels();
            RunSearch(false);
        }

        private void RefreshSidePanels()
        {
            lstUrgent.Items.Clear();
            foreach (Event ev in _manager.GetUrgentAnnouncements(5))   // priority queue
            {
                lstUrgent.Items.Add(ev);
            }

            lstNextUp.Items.Clear();
            foreach (Event ev in _manager.GetNextUp(5))                // queue
            {
                lstNextUp.Items.Add(ev);
            }
        }

        // ------------------------------------------------------------------
        // Searching
        // ------------------------------------------------------------------
        private void RunSearch(bool recordSearch)
        {
            string? category = null;
            if (cmbCategory.SelectedItem is string selected && selected != AllCategories)
            {
                category = selected;
            }

            DateTime? date = chkDate.Checked ? dtpDate.Value.Date : null;
            string keyword = txtKeyword.Text.Trim();
            bool hasFilter = category != null || date.HasValue || keyword.Length > 0;

            _currentResults = _manager.Search(category, date, keyword);

            if (recordSearch && hasFilter)
            {
                _manager.RecordSearch(new SearchState(category, date, keyword));   // push on the stack
            }

            PopulateEventList(_currentResults);
            RefreshRecommendations();
            RefreshHistory();

            if (_currentResults.Count == 0)
            {
                lblStatus.Text = "No events or announcements match your search.";
                if (recordSearch)
                {
                    MessageBox.Show("No events matched your search. Try a different category, date or keyword.",
                        "No results", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else if (hasFilter)
            {
                lblStatus.Text = $"{_currentResults.Count} result(s) found.";
            }
            else
            {
                lblStatus.Text = $"Showing all {_currentResults.Count} upcoming events and announcements.";
            }
        }

        private void PopulateEventList(List<Event> results)
        {
            _loading = true;
            lvEvents.BeginUpdate();
            lvEvents.Items.Clear();

            foreach (Event ev in results)
            {
                var item = new ListViewItem(ev.Date.ToString("ddd dd MMM yyyy"));
                item.SubItems.Add(ev.IsAnnouncement ? "Notice" : "Event");
                item.SubItems.Add(ev.Title);
                item.SubItems.Add(ev.Category);
                item.SubItems.Add(ev.Location);
                item.Tag = ev;

                if (ev.IsAnnouncement && ev.Priority == 1)
                {
                    item.ForeColor = Color.Firebrick;   // urgent notices stand out
                }
                lvEvents.Items.Add(item);
            }

            lvEvents.EndUpdate();
            _loading = false;
            rtbDetails.Clear();
        }

        // ------------------------------------------------------------------
        // Recommendations and history
        // ------------------------------------------------------------------
        private void RefreshRecommendations()
        {
            lstRecommended.Items.Clear();

            if (!_manager.HasInterestData)
            {
                lstRecommended.Items.Add("Search for events to get recommendations.");
                return;
            }

            // Exclude what is already on screen so every suggestion is something new.
            List<Recommendation> recommendations =
                _manager.GetRecommendations(_currentResults.Select(e => e.Id), 5);

            if (recommendations.Count == 0)
            {
                lstRecommended.Items.Add("No new recommendations right now.");
                return;
            }

            foreach (Recommendation recommendation in recommendations)
            {
                lstRecommended.Items.Add(recommendation);
            }
        }

        private void RefreshHistory()
        {
            List<SearchState> recent = _manager.GetRecentSearches(3);
            lblHistory.Text = recent.Count == 0
                ? "Recent searches: none yet"
                : "Recent searches (newest first): " + string.Join("  |  ", recent.Select(Describe));
        }

        private static string Describe(SearchState state)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(state.Category)) parts.Add(state.Category);
            if (state.Date.HasValue) parts.Add(state.Date.Value.ToString("dd MMM"));
            if (!string.IsNullOrEmpty(state.Keyword)) parts.Add($"\"{state.Keyword}\"");
            return string.Join(" + ", parts);
        }

        // ------------------------------------------------------------------
        // Details panel
        // ------------------------------------------------------------------
        private void ShowDetails(Event ev)
        {
            rtbDetails.Clear();
            AppendDetail(ev.Title, true);
            AppendDetail($"{(ev.IsAnnouncement ? "Announcement" : "Event")} | {ev.Category} | Priority: {PriorityText(ev.Priority)}", false);
            AppendDetail($"Date: {ev.Date:dddd, dd MMMM yyyy}", false);
            AppendDetail($"Location: {ev.Location}", false);
            AppendDetail(string.Empty, false);
            AppendDetail(ev.Description, false);
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

        // ------------------------------------------------------------------
        // Event handlers
        // ------------------------------------------------------------------
        private void BtnSearch_Click(object? sender, EventArgs e)
        {
            RunSearch(true);
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            cmbCategory.SelectedIndex = 0;
            chkDate.Checked = false;
            txtKeyword.Clear();
            RunSearch(false);
        }

        private void BtnPrevious_Click(object? sender, EventArgs e)
        {
            SearchState? previous = _manager.GoBackSearch();   // pop from the stack
            if (previous == null)
            {
                MessageBox.Show("There is no earlier search to go back to.",
                    "Previous Search", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int index = cmbCategory.FindStringExact(previous.Category ?? AllCategories);
            cmbCategory.SelectedIndex = index >= 0 ? index : 0;
            chkDate.Checked = previous.Date.HasValue;
            if (previous.Date.HasValue)
            {
                dtpDate.Value = previous.Date.Value;
            }
            txtKeyword.Text = previous.Keyword;

            RunSearch(false);
        }

        private void BtnBack_Click(object? sender, EventArgs e)
        {
            Close();   // the Main Menu shows itself again when this form closes
        }

        private void LvEvents_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_loading || lvEvents.SelectedItems.Count == 0)
            {
                return;
            }

            if (lvEvents.SelectedItems[0].Tag is Event ev)
            {
                ShowDetails(ev);
                _manager.RecordView(ev);        // viewing an event adds to the user's interests
                RefreshRecommendations();
            }
        }

        private void ListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (sender is not ListBox box || box.SelectedItem == null)
            {
                return;
            }

            Event? ev = box.SelectedItem switch
            {
                Event direct => direct,
                Recommendation recommendation => recommendation.Event,
                _ => null
            };

            if (ev == null)
            {
                return;
            }

            ShowDetails(ev);
            _manager.RecordView(ev);
            if (box != lstRecommended)
            {
                RefreshRecommendations();
            }
        }
    }
}