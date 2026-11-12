namespace MunicipalServicesApplication
{
    // One saved search. A record gives value equality and a tidy constructor.
    public record SearchState(string? Category, DateTime? Date, string Keyword);

    // A suggested event with its score and the reason shown to the user.
    public class Recommendation
    {
        public Event Event { get; set; } = new Event();
        public int Score { get; set; }
        public string Reason { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{Event.Title} ({Event.Date:dd MMM}) - Why: {Reason}";
        }
    }

    public class EventManager
    {
        // ---- Sorted dictionary: events kept in date order (key = the day) ----
        private readonly SortedDictionary<DateTime, List<Event>> _eventsByDate = new();

        // ---- Dictionaries (hash tables): O(1) lookup by category and by id ----
        private readonly Dictionary<string, List<Event>> _eventsByCategory = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, Event> _eventsById = new();

        // ---- Sets: unique categories and unique event dates ----
        private readonly HashSet<string> _categories = new(StringComparer.OrdinalIgnoreCase);
        private readonly SortedSet<DateTime> _eventDates = new();

        // ---- Stack: search history (last in, first out) for the Previous Search button ----
        private readonly Stack<SearchState> _searchHistory = new();

        // ---- Recommendation data: how often the user showed interest in something ----
        private readonly Dictionary<string, int> _categoryInterest = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _keywordInterest = new(StringComparer.OrdinalIgnoreCase);

        // Which categories are related, used to suggest events the user has not searched for yet.
        private static readonly Dictionary<string, string[]> RelatedCategories =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "Sports",    new[] { "Community", "Health" } },
                { "Culture",   new[] { "Community", "Education" } },
                { "Community", new[] { "Culture", "Sports" } },
                { "Health",    new[] { "Community", "Sports" } },
                { "Education", new[] { "Culture", "Community" } },
                { "Safety",    new[] { "Roads", "Community" } },
                { "Roads",     new[] { "Utilities", "Safety" } },
                { "Utilities", new[] { "Roads" } }
            };

        public EventManager()
        {
            SeedEvents();
        }

        // ------------------------------------------------------------------
        // Adding and reading events
        // ------------------------------------------------------------------
        public void AddEvent(Event ev)
        {
            DateTime day = ev.Date.Date;

            if (!_eventsByDate.TryGetValue(day, out List<Event>? onDay))
            {
                onDay = new List<Event>();
                _eventsByDate[day] = onDay;
            }
            onDay.Add(ev);

            if (!_eventsByCategory.TryGetValue(ev.Category, out List<Event>? inCategory))
            {
                inCategory = new List<Event>();
                _eventsByCategory[ev.Category] = inCategory;
            }
            inCategory.Add(ev);

            _eventsById[ev.Id] = ev;
            _categories.Add(ev.Category);   // a set ignores duplicates
            _eventDates.Add(day);           // a sorted set keeps dates ordered
        }

        public List<Event> GetAllEvents()
        {
            // The sorted dictionary already returns days in order.
            return _eventsByDate.Values.SelectMany(list => list).ToList();
        }

        public List<string> GetCategories()
        {
            return _categories.OrderBy(c => c).ToList();
        }

        public List<DateTime> GetUpcomingDates(int count)
        {
            return _eventDates.Where(d => d >= DateTime.Today).Take(count).ToList();
        }

        public List<Event> GetEventsOnDate(DateTime date)
        {
            return _eventsByDate.TryGetValue(date.Date, out List<Event>? onDay)
                ? new List<Event>(onDay)
                : new List<Event>();
        }

        // ------------------------------------------------------------------
        // Search (category, date and keyword)
        // ------------------------------------------------------------------
        public List<Event> Search(string? category, DateTime? date, string keyword)
        {
            IEnumerable<Event> source;

            if (date.HasValue)
            {
                source = GetEventsOnDate(date.Value);                 // sorted dictionary lookup
            }
            else if (!string.IsNullOrWhiteSpace(category))
            {
                source = _eventsByCategory.TryGetValue(category, out List<Event>? inCategory)
                    ? inCategory                                      // dictionary lookup
                    : new List<Event>();
            }
            else
            {
                source = GetAllEvents();
            }

            if (date.HasValue && !string.IsNullOrWhiteSpace(category))
            {
                source = source.Where(e => e.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                source = source.Where(e => MatchesKeyword(e, keyword));
            }

            return source.OrderBy(e => e.Date).ThenBy(e => e.Title).ToList();
        }

        private static bool MatchesKeyword(Event e, string keyword)
        {
            return e.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || e.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || e.Location.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || e.Category.Contains(keyword, StringComparison.OrdinalIgnoreCase);
        }

        // ------------------------------------------------------------------
        // Stack: search history
        // ------------------------------------------------------------------
        public void RecordSearch(SearchState state)
        {
            _searchHistory.Push(state);

            if (!string.IsNullOrWhiteSpace(state.Category))
            {
                Increment(_categoryInterest, state.Category);
            }

            foreach (string word in state.Keyword.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (word.Length >= 3)
                {
                    Increment(_keywordInterest, word);
                }
            }
        }

        // Viewing an event is a weaker signal of interest in its category.
        public void RecordView(Event ev)
        {
            Increment(_categoryInterest, ev.Category);
        }

        // Pop the current search and return the one before it (null if there is none).
        public SearchState? GoBackSearch()
        {
            if (_searchHistory.Count <= 1)
            {
                return null;
            }
            _searchHistory.Pop();
            return _searchHistory.Peek();
        }

        public List<SearchState> GetRecentSearches(int count)
        {
            return _searchHistory.Take(count).ToList();   // a stack enumerates newest first
        }

        public bool HasInterestData
        {
            get { return _categoryInterest.Count > 0 || _keywordInterest.Count > 0; }
        }

        private static void Increment(Dictionary<string, int> counts, string key)
        {
            counts.TryGetValue(key, out int current);
            counts[key] = current + 1;
        }

        // ------------------------------------------------------------------
        // Queue: the next upcoming events in date order
        // ------------------------------------------------------------------
        public Queue<Event> BuildUpcomingQueue()
        {
            var queue = new Queue<Event>();
            foreach (KeyValuePair<DateTime, List<Event>> pair in _eventsByDate)
            {
                if (pair.Key < DateTime.Today)
                {
                    continue;
                }
                foreach (Event ev in pair.Value.Where(e => !e.IsAnnouncement))
                {
                    queue.Enqueue(ev);
                }
            }
            return queue;
        }

        public List<Event> GetNextUp(int count)
        {
            Queue<Event> queue = BuildUpcomingQueue();
            var result = new List<Event>();
            while (queue.Count > 0 && result.Count < count)
            {
                result.Add(queue.Dequeue());   // first in, first out
            }
            return result;
        }

        // ------------------------------------------------------------------
        // Priority queue: most urgent announcements first
        // ------------------------------------------------------------------
        public List<Event> GetUrgentAnnouncements(int count)
        {
            var queue = new PriorityQueue<Event, int>();
            foreach (Event ev in _eventsById.Values.Where(e => e.IsAnnouncement && e.Date.Date >= DateTime.Today))
            {
                // Lower number = served first. Priority decides, then the sooner date.
                int daysAway = Math.Min((ev.Date.Date - DateTime.Today).Days, 999);
                queue.Enqueue(ev, ev.Priority * 1000 + daysAway);
            }

            var result = new List<Event>();
            while (queue.Count > 0 && result.Count < count)
            {
                result.Add(queue.Dequeue());
            }
            return result;
        }

        // ------------------------------------------------------------------
        // Recommendations: score events using the user's search patterns
        // ------------------------------------------------------------------
        public List<Recommendation> GetRecommendations(IEnumerable<int> excludeIds, int max = 5)
        {
            var excluded = new HashSet<int>(excludeIds);
            DateTime today = DateTime.Today;

            // The user's three most-searched categories, most popular first.
            List<string> topCategories = _categoryInterest
                .OrderByDescending(p => p.Value)
                .Select(p => p.Key)
                .Take(3)
                .ToList();

            var scored = new List<Recommendation>();

            foreach (Event ev in _eventsById.Values)
            {
                if (excluded.Contains(ev.Id) || ev.Date.Date < today)
                {
                    continue;
                }

                int score = 0;
                var reasons = new List<string>();

                // 1. Category match: +3 for the top category, +2 for second, +1 for third.
                int rank = topCategories.FindIndex(c => c.Equals(ev.Category, StringComparison.OrdinalIgnoreCase));
                if (rank >= 0)
                {
                    score += 3 - rank;
                    reasons.Add($"interest in {ev.Category}");
                }

                // 2. Related category: +1 if it relates to something the user searched.
                foreach (string top in topCategories)
                {
                    if (RelatedCategories.TryGetValue(top, out string[]? related)
                        && related.Contains(ev.Category, StringComparer.OrdinalIgnoreCase))
                    {
                        score += 1;
                        reasons.Add($"related to {top}");
                        break;
                    }
                }

                // 3. Keyword match: up to +3 per keyword the user has searched for.
                string text = $"{ev.Title} {ev.Description} {ev.Location}";
                foreach (KeyValuePair<string, int> pair in _keywordInterest)
                {
                    if (text.Contains(pair.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        score += Math.Min(pair.Value, 3);
                        reasons.Add($"keyword \"{pair.Key}\"");
                    }
                }

                if (score > 0)
                {
                    // 4. Small bonus when the event is within a week.
                    if ((ev.Date.Date - today).Days <= 7)
                    {
                        score += 1;
                    }

                    scored.Add(new Recommendation
                    {
                        Event = ev,
                        Score = score,
                        Reason = string.Join(", ", reasons.Take(2))
                    });
                }
            }

            List<Recommendation> result = scored
                .OrderByDescending(r => r.Score)
                .ThenBy(r => r.Event.Date)
                .Take(max)
                .ToList();

            // Fallback: if there are few matches, fill up with the soonest events (the queue).
            if (HasInterestData && result.Count < 3)
            {
                foreach (Event ev in BuildUpcomingQueue())
                {
                    if (result.Count >= 3)
                    {
                        break;
                    }
                    if (excluded.Contains(ev.Id) || result.Any(r => r.Event.Id == ev.Id))
                    {
                        continue;
                    }
                    result.Add(new Recommendation { Event = ev, Score = 0, Reason = "happening soon" });
                }
            }

            return result;
        }

        // ------------------------------------------------------------------
        // Sample data (dates are relative to today so they are always upcoming)
        // ------------------------------------------------------------------
        private void Seed(int id, string title, string category, int daysAhead, string location,
                          string description, int priority = 3, bool isAnnouncement = false)
        {
            AddEvent(new Event
            {
                Id = id,
                Title = title,
                Category = category,
                Date = DateTime.Today.AddDays(daysAhead),
                Location = location,
                Description = description,
                Priority = priority,
                IsAnnouncement = isAnnouncement
            });
        }

        private void SeedEvents()
        {
            Seed(1, "Community Clean-Up Day", "Community", 2, "Central Park",
                 "Volunteers collect litter and plant trees. Gloves and bags provided.");
            Seed(2, "Youth Soccer Tournament", "Sports", 3, "Municipal Stadium",
                 "Local under-17 teams compete for the mayor's trophy.");
            Seed(3, "Heritage Day Cultural Festival", "Culture", 5, "Civic Centre Lawn",
                 "Traditional food, music and dance from across the municipality.");
            Seed(4, "Free Health Screening", "Health", 4, "Community Clinic",
                 "Free blood pressure, glucose and HIV screening for residents.");
            Seed(5, "Planned Water Interruption", "Utilities", 1, "Ward 7",
                 "Water will be off from 08:00 to 16:00 for pipe repairs. Store water in advance.", 1, true);
            Seed(6, "Load Reduction Schedule Update", "Utilities", 2, "All wards",
                 "The electricity load reduction schedule has changed. Check your ward timetable.", 2, true);
            Seed(7, "Main Road Resurfacing", "Roads", 3, "Main Road, 1st to 5th Avenue",
                 "Expect delays and lane closures while the road is resurfaced.", 2, true);
            Seed(8, "Library Reading Programme", "Education", 6, "Central Library",
                 "Weekly reading sessions for children aged 6 to 12.");
            Seed(9, "Charity Fun Run", "Sports", 9, "Riverside Promenade",
                 "A 5 km fun run raising funds for the local animal shelter.");
            Seed(10, "Neighbourhood Watch Meeting", "Safety", 7, "Community Hall",
                 "Residents and police discuss crime prevention in the neighbourhood.");
            Seed(11, "Live Music Night", "Culture", 10, "Town Hall",
                 "Local bands and choirs perform. Entry is free.");
            Seed(12, "Municipal Budget Public Meeting", "Community", 12, "Council Chamber",
                 "Residents can comment on the proposed municipal budget.");
            Seed(13, "Skills Development Workshop", "Education", 14, "Youth Centre",
                 "CV writing and interview skills for unemployed youth.");
            Seed(14, "Refuse Collection Delay", "Utilities", 3, "Wards 3 and 4",
                 "Refuse collection will run one day late due to truck repairs.", 2, true);
            Seed(15, "Road Safety Awareness Campaign", "Safety", 8, "Taxi Rank",
                 "Traffic officers share road safety tips with commuters and drivers.");
            Seed(16, "Farmers Market", "Community", 11, "Market Square",
                 "Fresh produce and crafts from local farmers and small businesses.");
            Seed(17, "Netball League Finals", "Sports", 13, "Sports Complex",
                 "The final matches of the municipal netball league.");
            Seed(18, "Marathon Road Closures", "Roads", 9, "City centre",
                 "Streets around the city centre close from 05:00 to 12:00 for the marathon.", 2, true);
        }
    }
}