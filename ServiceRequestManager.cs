namespace MunicipalServicesApplication
{
    public class ServiceRequestManager
    {
        private readonly BinarySearchTree _tree = new();
        private readonly List<ServiceRequest> _requests = new();

        public ServiceRequestManager()
        {
            SeedRequests();
        }

        public int Count
        {
            get { return _requests.Count; }
        }

        public void Add(ServiceRequest request)
        {
            _requests.Add(request);
            _tree.Insert(request);
        }

        // In-order traversal of a binary search tree returns requests sorted by ID.
        public List<ServiceRequest> GetAll()
        {
            return _tree.InOrder();
        }

        // Tracking by ID uses the binary search tree, not a loop over every request.
        public ServiceRequest? Track(int id)
        {
            return _tree.Find(id);
        }

        // The heap hands back the most urgent unresolved requests first.
        public List<ServiceRequest> GetMostUrgent(int count)
        {
            var heap = new MinHeap();
            foreach (ServiceRequest request in _requests.Where(r => r.Status != "Resolved"))
            {
                heap.Add(request);
            }

            var result = new List<ServiceRequest>();
            while (heap.Count > 0 && result.Count < count)
            {
                ServiceRequest? next = heap.RemoveMin();
                if (next != null)
                {
                    result.Add(next);
                }
            }
            return result;
        }

        private void Seed(int id, string title, string category, string ward,
                          string status, int priority, int daysAgo)
        {
            Add(new ServiceRequest
            {
                Id = id,
                Title = title,
                Category = category,
                Ward = ward,
                Status = status,
                Priority = priority,
                DateSubmitted = DateTime.Today.AddDays(-daysAgo)
            });
        }

        // IDs are added out of order on purpose so the tree is reasonably balanced.
        private void SeedRequests()
        {
            Seed(1007, "Burst water pipe in Mandela Street", "Water", "Ward 3", "In Progress", 1, 2);
            Seed(1003, "Pothole on Main Road", "Roads", "Ward 1", "Submitted", 2, 5);
            Seed(1011, "Streetlight not working", "Electricity", "Ward 4", "Submitted", 3, 3);
            Seed(1001, "Illegal dumping near the park", "Sanitation", "Ward 2", "Resolved", 3, 20);
            Seed(1005, "Sewage spill at the taxi rank", "Sanitation", "Ward 3", "In Progress", 1, 4);
            Seed(1009, "Traffic light out of order", "Roads", "Ward 5", "Submitted", 2, 1);
            Seed(1013, "Water leak at the community hall", "Water", "Ward 2", "Submitted", 2, 2);
            Seed(1002, "Missed refuse collection", "Sanitation", "Ward 1", "Resolved", 3, 12);
            Seed(1004, "Faulty electricity meter", "Electricity", "Ward 5", "In Progress", 2, 8);
            Seed(1006, "Blocked storm drain", "Roads", "Ward 4", "Submitted", 2, 6);
            Seed(1008, "Broken playground equipment", "Community", "Ward 2", "Submitted", 3, 9);
            Seed(1010, "No water supply in street", "Water", "Ward 1", "In Progress", 1, 3);
            Seed(1012, "Overgrown vacant lot", "Community", "Ward 3", "Resolved", 3, 15);
            Seed(1014, "Cable theft causing power outage", "Electricity", "Ward 1", "In Progress", 1, 1);
            Seed(1015, "Damaged road sign", "Roads", "Ward 2", "Submitted", 3, 7);
        }
    }
}