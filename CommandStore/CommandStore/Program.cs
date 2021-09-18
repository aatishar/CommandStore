using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CommandStore
{
    class Program
    {
        static void Main(string[] args)
        {
            var commandsBatch1 = new List<AddBookCommand>()
            {
                new AddBookCommand(new Book { Title = "Book 1" }),
                new AddBookCommand(new Book { Title = "Book 2" }),
                new AddBookCommand(new Book { Title = "Book 3" }),
                new AddBookCommand(new Book { Title = "Book 4" }),
                new AddBookCommand(new Book { Title = "Book 5" }),
                new AddBookCommand(new Book { Title = "Book 6" }),
            };

            var commandsBatch2 = new List<AddBookCommand>()
            {
                new AddBookCommand(new Book { Title = "Book 7" }),
                new AddBookCommand(new Book { Title = "Book 8" }),
                new AddBookCommand(new Book { Title = "Book 9" }),
                new AddBookCommand(new Book { Title = "Book 10" }),
                new AddBookCommand(new Book { Title = "Book 11" }),
                new AddBookCommand(new Book { Title = "Book 12" }),
            };

            var repositoryFatory = new RepositoryFactory();
            var locationPath = @$"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\CommandStore";

            if (!Directory.Exists(locationPath))
            {
                Directory.CreateDirectory(locationPath);
            }

            var engine = new Engine<Repository>(repositoryFatory, locationPath);
            engine.RegisterCommandAndCommandHander<AddBookCommand, AddBookCommandHandler>();

            engine.Replay();
            engine.CreateSnapshot();
            engine.ExecuteCommmands(commandsBatch1.Cast<Command>().ToList());
            engine.SaveCommandsToJournal();

            engine.ExecuteCommmands(commandsBatch2.Cast<Command>().ToList());
            engine.SaveCommandsToJournal();


            var engine2 = new Engine<Repository>(repositoryFatory, locationPath);
            engine2.RegisterCommandAndCommandHander<AddBookCommand, AddBookCommandHandler>();
            engine2.RegisterQueryAndQueryHander<GetBookByTitileQuery, GetBookByTitileQueryHandler>();
            engine2.Replay();
            var queryResult = engine2.ExecuteQuery(new GetBookByTitileQuery { Title = "Book 1" }).ToList();
            Console.WriteLine(engine2.repository.Books[0].Title);
        }
    }
}
