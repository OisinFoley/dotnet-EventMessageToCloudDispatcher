using EventDispatcher.Generic;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventDispatcher.Azure
{
    public sealed class EventReceiver<T> : IEventReceiver<EventMessage<T>>, IDisposable
    {
        private readonly ISubscriptionClient m_SubscriptionClient;
        private readonly ILogger m_Logger;

        public EventReceiver(
            ILogger logger,
            string connectionString,
            string topicPath,
            string subscriptionName)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
            if (string.IsNullOrWhiteSpace(topicPath))
                throw new ArgumentException("Topic name cannot be null or empty", nameof(topicPath));
            if (string.IsNullOrWhiteSpace(subscriptionName))
                throw new ArgumentException("Subscription cannot be null or empty", nameof(subscriptionName));

            m_SubscriptionClient = new SubscriptionClient(connectionString, topicPath, subscriptionName);
        }

        public void RegisterHandler(Func<EventMessage<T>, Task> handler)
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False means that the Completion will be handled by the User Callback in the lambda function inside "ConvertToCorrectMessageHandlerFuncReturnType".
                AutoComplete = false
            };

            // Register the function that will process messages
            m_SubscriptionClient.RegisterMessageHandler(ConvertToCorrectMessageHandlerFuncReturnType(handler), messageHandlerOptions);
        }

        public void Dispose()
        {
            m_SubscriptionClient.CloseAsync().Wait();
        }

        private Func<Message, CancellationToken, Task> ConvertToCorrectMessageHandlerFuncReturnType(Func<EventMessage<T>, Task> handler)
        {
            return async (message, token) =>
            {
                // convert received message to an EventMessage
                EventMessage<T> eventItem = new EventMessage<T>
                {
                    Id = message.MessageId,
                    Header = message.Label,
                    Content = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(message.Body))
                };

                await handler(eventItem);
                await m_SubscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
            };
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionEventArgs)
        {
            m_Logger.LogError($"Exception encountered with message handler {exceptionEventArgs.Exception}.");
            var exceptionReceivedContext = exceptionEventArgs.ExceptionReceivedContext;
            m_Logger.LogError("Exception event context properties for troubleshooting:");
            m_Logger.LogError($"Endpoint: {exceptionReceivedContext.Endpoint}");
            m_Logger.LogError($"Entity Path: {exceptionReceivedContext.EntityPath}");
            m_Logger.LogError($"Action: {exceptionReceivedContext.Action}");
            return Task.CompletedTask;
        }

    }
}
