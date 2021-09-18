using System.Text.Json;

namespace CommandStore
{
    public class AddBookCommand : Command, ICommand<Book>
    {
        public Book Item { get; set; }

        private AddBookCommand()
        {
        }

        public AddBookCommand(Book book)
        {
            this.CommandType = nameof(AddBookCommand);
            this.Item = book;
            this.ItemInBytes = JsonSerializer.SerializeToUtf8Bytes(book);
            this.ItemObject = book;
        }

        public static AddBookCommand CreateCommand(Command command)
        {
            return new AddBookCommand
            {
                CommandNumber = command.CommandNumber,
                CommandType = command.CommandType,
                ItemInBytes = command.ItemInBytes,
                Item = DeserialiserHelper.Deserialize<Book>(command.ItemInBytes)
            };
        }
    }

    public class AddBookCommandHandler : ICommandHandler<Repository>
    {
        public void Execute(Repository repository, object item)
        {

            repository.Books.Add((Book)item);
        }
    }
}
