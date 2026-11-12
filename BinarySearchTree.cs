namespace MunicipalServicesApplication
{
    public class BstNode
    {
        public ServiceRequest Request { get; }
        public BstNode? Left { get; set; }
        public BstNode? Right { get; set; }

        public BstNode(ServiceRequest request)
        {
            Request = request;
        }
    }

    public class BinarySearchTree
    {
        private BstNode? _root;
        public int Count { get; private set; }

        public void Insert(ServiceRequest request)
        {
            _root = Insert(_root, request);
        }

        private BstNode Insert(BstNode? node, ServiceRequest request)
        {
            if (node == null)
            {
                Count++;
                return new BstNode(request);
            }
            if (request.Id < node.Request.Id)
            {
                node.Left = Insert(node.Left, request);
            }
            else if (request.Id > node.Request.Id)
            {
                node.Right = Insert(node.Right, request);
            }
            return node;
        }

        public ServiceRequest? Find(int id)
        {
            BstNode? current = _root;
            while (current != null)
            {
                if (id == current.Request.Id)
                {
                    return current.Request;
                }
                current = id < current.Request.Id ? current.Left : current.Right;
            }
            return null;
        }

        public List<ServiceRequest> InOrder()
        {
            var list = new List<ServiceRequest>();
            InOrder(_root, list);
            return list;
        }

        private void InOrder(BstNode? node, List<ServiceRequest> list)
        {
            if (node == null)
            {
                return;
            }
            InOrder(node.Left, list);
            list.Add(node.Request);
            InOrder(node.Right, list);
        }
    }
}