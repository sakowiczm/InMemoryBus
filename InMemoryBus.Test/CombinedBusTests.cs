using System;
using System.Linq;
using Xunit;

namespace InMemoryBus.Test
{
    public class TestEventHandler2 : IEventHandler<TestEvent>
    {
        public string Data { get; set; }

        public void Handle(TestEvent @event)
        {
            Data = @event.Data;
        }
    }

    public class TestCommand : ICommand
    {
        public string Data { get; set; }   
    }

    public class TestCommandHandler1 : ICommandHandler<TestCommand>
    {
        public string Data { get; set; }

        public void Handle(TestCommand command)
        {
            Data = command.Data;
        }
    }

    public class CombinedBusTests
    {
        [Fact]
        public void Bus_Publish_Event_Ok()
        {
            var bus = new CombinedBus();

            string value = Guid.NewGuid().ToString();

            var handler = new TestEventHandler1();

            bus.Subscribe(handler);
            bus.Publish(new TestEvent() { Data = value });

            Assert.True(handler.Data == value);
        }

        [Fact]
        public void Bus_Publish_MultipleEvents_Ok()
        {
            var bus = new CombinedBus();

            var handler1 = new TestEventHandler1();

            bus.Subscribe(handler1);

            Enumerable.Repeat<Action>(() =>
            {
                string value = Guid.NewGuid().ToString();

                bus.Publish(new TestEvent() { Data = value });

                Assert.True(handler1.Data == value);
            }, 10);
        }


        [Fact]
        public void Bus_Publish_MultipleHandlers_Ok()
        {
            var bus = new CombinedBus();

            var handler1 = new TestEventHandler1();
            var handler2 = new TestEventHandler2();

            bus.Subscribe(handler1);
            bus.Subscribe(handler2);

            Enumerable.Repeat<Action>(() =>
            {
                string value = Guid.NewGuid().ToString();

                bus.Publish(new TestEvent() { Data = value });

                Assert.True(handler1.Data == value);
                Assert.True(handler2.Data == value);
            }, 10);
        }

        [Fact]
        public void Bus_Send_Command_Ok()
        {
            string value = Guid.NewGuid().ToString();

            var bus = new CombinedBus();

            var handler = new TestCommandHandler1();

            bus.Register(handler);

            bus.Send(new TestCommand() { Data = value });

            Assert.True(handler.Data == value);
        }

        // todo: test for error when trying to register two CommandHandlers for the same Command
    }
}