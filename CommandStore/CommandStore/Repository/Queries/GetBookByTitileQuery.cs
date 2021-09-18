using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommandStore
{
    public class GetBookByTitileQuery : IQuery<IEnumerable<Book>>
    {
        public string Title { get; set; }
    }

    class GetBookByTitileQueryHandler : IQueryHandler<Repository, GetBookByTitileQuery, IEnumerable<Book>>
    {
        public IEnumerable<Book> Execute(Repository repository, GetBookByTitileQuery query)
        {
            return repository
                .Books
                .Where(a => string.Equals(a.Title, query.Title, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
