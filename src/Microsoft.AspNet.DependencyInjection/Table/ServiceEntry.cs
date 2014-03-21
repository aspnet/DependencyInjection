namespace Microsoft.AspNet.DependencyInjection.Table
{
    internal class ServiceEntry
    {
        public ServiceEntry(IService service)
        {
            First = service;
            Last = service;
        }

        public IService First { get; private set; }
        public IService Last { get; private set; }

        public void Add(IService service)
        {
            Last.Next = service;
            Last = service;
        }
    }
}
