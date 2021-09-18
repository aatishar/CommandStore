namespace CommandStore
{
    public interface IQueryHandler<TRepository,TQuery, TResult> where TQuery: IQuery<TResult>
    {
        TResult Execute(TRepository repository, TQuery query);
    }
}
