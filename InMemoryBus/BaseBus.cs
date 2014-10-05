using System.Collections.Generic;

namespace InMemoryBus
{
    /// <summary>
    /// Low level deals only with messages
    /// </summary>
    public class BaseBus
    {
        private static BaseBus _instance;

        public static BaseBus Instance { get { return _instance ?? (_instance = new BaseBus()); } }

        // todo: quick & dirty but not very performant
        private readonly Dictionary<string, Dictionary<string, IBusMessageHandler>> _messageHandlers;

        public BaseBus()
        {
            _messageHandlers = new Dictionary<string, Dictionary<string, IBusMessageHandler>>();
        }

        /*        static BaseBus()
                {
                    var handlerType = typeof(IBusMessageHandler<>);

                    _messageHandlers = handlerType.Assembly
                        .GetTypes()
                        .Where(t => !t.IsAbstract && t.IsClass && (t.BaseType != null && t.BaseType.IsGenericType))
                        .Where(t => t.BaseType.GetGenericTypeDefinition() == handlerType)
                        .ToDictionary(t => t.BaseType.GetGenericArguments()[0]);            
                }
         */

        public void Subscribe<T>(IMessageHandler<T> handler) where T : class, IMessage
        {
            string key = typeof(T).ToString();
            string innerKey = handler.GetType().ToString();

            if (_messageHandlers.ContainsKey(key))
            {
                var dict = _messageHandlers[key];

                if (!dict.ContainsKey(innerKey))
                    dict.Add(innerKey, new BusMessageHandler<T>(handler));
            }
            else
            {
                var dict = new Dictionary<string, IBusMessageHandler>();
                dict.Add(innerKey, new BusMessageHandler<T>(handler));
                _messageHandlers.Add(key, dict);
            }
        }

        public void Unsubscribe<T>(IMessageHandler<T> handler) where T : IMessage
        {
            string key = typeof(T).ToString();

            if (_messageHandlers.ContainsKey(key))
            {
                var dict = _messageHandlers[key];
                string innerKey = handler.GetType().ToString();

                if (dict.ContainsKey(innerKey))
                    dict.Remove(innerKey);
            }
        }

        public void Publish<T>(T message) where T : class, IMessage
        {
            string key = message.GetType().ToString();

            if (_messageHandlers.ContainsKey(key))
            {
                var dict = _messageHandlers[key];

                foreach (var innerKey in dict.Keys)
                {
                    var handler = dict[innerKey];
                    handler.Handle(message);
                }
            }
        }

        #region Internals

        protected interface IBusMessageHandler
        {
            void Handle(IMessage message);
        }

        protected class BusMessageHandler<T> : IBusMessageHandler where T : class, IMessage
        {
            private readonly IMessageHandler<T> _handler;

            public BusMessageHandler(IMessageHandler<T> handler)
            {
                _handler = handler;
            }

            public void Handle(IMessage message)
            {
                var msg = message as T;

                if (msg != null)
                    _handler.Handle(msg);
            }
        }

        #endregion
    }

    /// <summary>
    /// Higher abstraction deals with commands and events
    /// </summary>
    public class Bus : BaseBus
    {
        // todo: quick & dirty but not very performant

        /// <summary>
        /// There can be only one command handler per command
        /// </summary>
        private readonly Dictionary<string, IBusMessageHandler> _commandHandlers;

        public Bus()
        {
            _commandHandlers = new Dictionary<string, IBusMessageHandler>();

            // todo: get something that will resolve all existing command handlers
        }

        // it's public for now until I won't find nice way to register handlers automatically
        public void Register<T>(ICommandHandler<T> handler) where T : class, ICommand
        {
            string key = typeof(T).ToString();

            if (!_commandHandlers.ContainsKey(key))
            {
                _commandHandlers.Add(key, new BusMessageHandler<T>(handler));
            }
        }

        public new void Publish<T>(T @event) where T : class, IEvent
        {
            base.Publish(@event);
        }

        public new void Subscribe<T>(IEventHandler<T> handler) where T : class, IEvent
        {
            base.Subscribe(handler);
        }

        public new void Unsubscribe<T>(IEventHandler<T> handler) where T : class, IEvent
        {
            base.Unsubscribe(handler);
        }

        public void Send<T>(T command) where T : class, ICommand
        {
            string key = command.GetType().ToString();

            if (_commandHandlers.ContainsKey(key))
            {
                var handler = _commandHandlers[key];
                handler.Handle(command);
            }
        }
    }

    // Publish Event - we can Subscribe & Unsubscribe for events, we can have 0 .. n subscribers
    // Send Command - we can have only one command handler per command

    public class CombinedBus : IBus
    {
        // todo: quick & dirty but not very performant
        private readonly Dictionary<string, Dictionary<string, IHandler>> _eventHandlers;
        private readonly Dictionary<string, IHandler> _commandHandlers;

        public CombinedBus()
        {
            _eventHandlers = new Dictionary<string, Dictionary<string, IHandler>>();
            _commandHandlers = new Dictionary<string, IHandler>();
        }

        public void Subscribe<T>(IEventHandler<T> handler) where T : class, IEvent
        {
            string key = typeof(T).ToString();
            string innerKey = handler.GetType().ToString();

            if (_eventHandlers.ContainsKey(key))
            {
                var dict = _eventHandlers[key];

                if (!dict.ContainsKey(innerKey))
                    dict.Add(innerKey, new Handler<T>(handler));
            }
            else
            {
                var dict = new Dictionary<string, IHandler>();
                dict.Add(innerKey, new Handler<T>(handler));
                _eventHandlers.Add(key, dict);
            }
        }

        public void Unsubscribe<T>(IEventHandler<T> handler) where T : IEvent
        {
            string key = typeof(T).ToString();

            if (_eventHandlers.ContainsKey(key))
            {
                var dict = _eventHandlers[key];
                string innerKey = handler.GetType().ToString();

                if (dict.ContainsKey(innerKey))
                    dict.Remove(innerKey);
            }
        }

        public void Register<T>(ICommandHandler<T> handler) where T : class, ICommand
        {
            string key = typeof(T).ToString();

            if (!_commandHandlers.ContainsKey(key))
            {
                _commandHandlers.Add(key, new Handler<T>(handler));
            }
        }

        public void Publish<T>(T message) where T : class, IEvent
        {
            string key = message.GetType().ToString();

            if (_eventHandlers.ContainsKey(key))
            {
                var dict = _eventHandlers[key];

                foreach (var innerKey in dict.Keys)
                {
                    var handler = dict[innerKey];
                    handler.Handle(message);
                }
            }
        }

        public void Send<T>(T command) where T : class, ICommand
        {
            string key = command.GetType().ToString();

            if (_commandHandlers.ContainsKey(key))
            {
                var handler = _commandHandlers[key];
                handler.Handle(command);
            }
        }

        #region Internals

        private interface IHandler
        {
            void Handle(IMessage message);
        }

        private class Handler<T> : IHandler where T : class, IMessage
        {
            private readonly IMessageHandler<T> _handler;

            public Handler(IMessageHandler<T> handler)
            {
                _handler = handler;
            }

            public void Handle(IMessage message)
            {
                var msg = message as T;

                if (msg != null)
                    _handler.Handle(msg);
            }
        }

        #endregion
    }
}