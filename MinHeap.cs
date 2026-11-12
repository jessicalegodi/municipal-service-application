namespace MunicipalServicesApplication
{
    // Min-heap: the most urgent request (lowest priority number, then oldest) is always first.
    public class MinHeap
    {
        private readonly List<ServiceRequest> _items = new();
        public int Count
        {
            get { return _items.Count; }
        }

        private static bool Less(ServiceRequest a, ServiceRequest b)
        {
            return a.Priority < b.Priority
                || (a.Priority == b.Priority && a.DateSubmitted < b.DateSubmitted);
        }

        public void Add(ServiceRequest request)
        {
            _items.Add(request);
            int i = _items.Count - 1;
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (!Less(_items[i], _items[parent]))
                {
                    break;
                }
                (_items[i], _items[parent]) = (_items[parent], _items[i]);
                i = parent;
            }
        }

        public ServiceRequest? RemoveMin()
        {
            if (_items.Count == 0)
            {
                return null;
            }

            ServiceRequest min = _items[0];
            ServiceRequest last = _items[_items.Count - 1];
            _items.RemoveAt(_items.Count - 1);

            if (_items.Count > 0)
            {
                _items[0] = last;
                int i = 0;
                while (true)
                {
                    int left = 2 * i + 1;
                    int right = 2 * i + 2;
                    int smallest = i;
                    if (left < _items.Count && Less(_items[left], _items[smallest]))
                    {
                        smallest = left;
                    }
                    if (right < _items.Count && Less(_items[right], _items[smallest]))
                    {
                        smallest = right;
                    }
                    if (smallest == i)
                    {
                        break;
                    }
                    (_items[i], _items[smallest]) = (_items[smallest], _items[i]);
                    i = smallest;
                }
            }
            return min;
        }
    }
}