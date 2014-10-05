namespace InMemoryBus
{
    public interface IBus
    {
        void Subscribe<T>(IEventHandler<T> handler) where T : class, IEvent;
        void Unsubscribe<T>(IEventHandler<T> handler) where T : IEvent;
        void Register<T>(ICommandHandler<T> handler) where T : class, ICommand;
        void Publish<T>(T message) where T : class, IEvent;
        void Send<T>(T command) where T : class, ICommand;
    }
}