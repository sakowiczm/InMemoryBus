namespace InMemoryBus
{
    public interface ICommandHandler<T> : IMessageHandler<T> where T : ICommand { }
}