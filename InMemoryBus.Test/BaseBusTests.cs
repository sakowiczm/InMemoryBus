using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace InMemoryBus.Test
{
    public class TestMessage : IMessage
    {
        public string Data { get; set; }
    }

    public class TestHandler1 : IMessageHandler<TestMessage>
    {
        public string Data { get; set; }

        public void Handle(TestMessage @event)
        {
            Data = @event.Data;
        }
    }

    public class TestHandler2 : IMessageHandler<TestMessage>
    {
        public string Data { get; set; }

        public void Handle(TestMessage @event)
        {
            Data = @event.Data;
        }
    }


    public class BaseBusTests
    {
        [Fact]
        public void BaseBus_Send_Ok()
        {
            var bus = new BaseBus();

            string value = Guid.NewGuid().ToString();

            var handler = new TestHandler1();

            bus.Subscribe(handler);
            bus.Publish(new TestMessage() { Data = value });

            Assert.True(handler.Data == value);
        }

        [Fact]
        public void BaseBus_Send_MultipleMessages_Ok()
        {
            var bus = new BaseBus();

            var handler1 = new TestHandler1();

            bus.Subscribe(handler1);

            Enumerable.Repeat<Action>(() =>
            {
                string value = Guid.NewGuid().ToString();

                bus.Publish(new TestMessage() { Data = value });

                Assert.True(handler1.Data == value);
            }, 10);
        }


        [Fact]
        public void BaseBus_Send_MultipleHandlers_Ok()
        {
            var bus = new BaseBus();

            var handler1 = new TestHandler1();
            var handler2 = new TestHandler2();

            bus.Subscribe(handler1);
            bus.Subscribe(handler2);

            Enumerable.Repeat<Action>(() =>
            {
                string value = Guid.NewGuid().ToString();

                bus.Publish(new TestMessage() { Data = value });

                Assert.True(handler1.Data == value);
                Assert.True(handler2.Data == value);
            }, 10);
        }

    }
}
