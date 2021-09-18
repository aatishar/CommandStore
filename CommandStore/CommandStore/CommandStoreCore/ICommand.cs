namespace CommandStore
{
    public interface ICommand<TItem>
    {
        TItem Item { get; set; }
    }
}
