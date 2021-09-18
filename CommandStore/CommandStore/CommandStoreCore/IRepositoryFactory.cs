namespace CommandStore
{
    public interface IRepositoryFactory<TRepository>
    {
        TRepository CreateNewRepository();
    }
}
