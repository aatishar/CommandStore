using System.Collections.Generic;

namespace CommandStore
{
    public class RepositoryFactory : IRepositoryFactory<Repository>
    {
        public Repository CreateNewRepository()
        {
            return new Repository
            {
                Books = new List<Book>()
            };
        }
    }
}
