using EventDispatcher.generic;
using Microsoft.Azure.ServiceBus;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using EventDispatcher.Generic;

namespace EventDispatcher.Azure
{
    public sealed class EventSender<T> : IEventSender<EventMessage<T>>, IDisposable
    {
        private readonly ITopicClient m_TopicClient;

        public EventSender(
            string connectionString,
            string topicName
            )
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
            if (string.IsNullOrWhiteSpace(topicName))
                throw new ArgumentException("Topic name cannot be null or empty", nameof(connectionString));

            m_TopicClient = new TopicClient(connectionString, topicName);
        }

        public async Task SendAsync(EventMessage<T>[] items)
        {
            if (m_TopicClient.IsClosedOrClosing)
            {
                var exception = new InvalidOperationException("Connection to event broker is not open");
                throw exception;
            }

            IEnumerable<Task> sendTasks = items.Select(item =>
            {
                var message = new Message
                {
                    MessageId = item.Id,
                    Label = item.Header,
                    Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(item.Content)),
                };
                return m_TopicClient.SendAsync(message);
            });
            await Task.WhenAll(sendTasks);
        }

        public void Dispose()
        {
            m_TopicClient.CloseAsync().Wait();
        }
    }
}
