namespace MunicipalServicesApplication
{
    public class ServiceRequest
    {
        public int Id { get; set; }                          // unique number
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string Status { get; set; } = "Submitted";    // Submitted, In Progress, Resolved
        public int Priority { get; set; } = 3;               // 1 = urgent
        public DateTime DateSubmitted { get; set; } = DateTime.Today;

        public string Reference
        {
            get { return "SR-" + Id; }
        }
    }
}