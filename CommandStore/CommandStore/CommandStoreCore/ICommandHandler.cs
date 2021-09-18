namespace CommandStore
{
    public interface ICommandHandler<TRepository>
    {
        void Execute(TRepository repository, object item);
    }
}
