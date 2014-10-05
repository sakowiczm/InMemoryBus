namespace InMemoryBus
{
    public interface IMessageHandler<T> where T : IMessage
    {
        void Handle(T message);
    }
}