using System;
using Xunit;

namespace InMemoryBus.Test
{
    public class TestEvent : IEvent
    {
        public string Data { get; set; }
    }

    public class TestEventHandler1 : IEventHandler<TestEvent>
    {
        public string Data { get; set; }

        public void Handle(TestEvent @event)
        {
            Data = @event.Data;
        }
    }

    public class BusTests
    {
        [Fact]
        public void Bus_PublishEvent_Ok()
        {
            var bus = new Bus();

            string value = Guid.NewGuid().ToString();

            var handler = new TestEventHandler1();

            bus.Subscribe(handler);
            bus.Publish(new TestEvent() { Data = value });

            Assert.True(handler.Data == value);
        }          
    }
}