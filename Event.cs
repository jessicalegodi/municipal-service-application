namespace MunicipalServicesApplication
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Priority { get; set; } = 3;          // 1 = urgent, 2 = important, 3 = normal
        public bool IsAnnouncement { get; set; }

        public override string ToString()
        {
            string prefix = IsAnnouncement ? "NOTICE" : "EVENT";
            return $"[{Date:dd MMM}] {prefix}: {Title}";
        }
    }
}