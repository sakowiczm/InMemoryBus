namespace InMemoryBus
{
    public interface IEventHandler<T> : IMessageHandler<T> where T : IEvent { }
}