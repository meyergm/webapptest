using DataGenerator.Models;
using Newtonsoft.Json;
using System;
using System.Text;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace DataGenerator.Senders
{
    /// <summary>
    /// Creates the event hub and sends data to the event hub
    /// </summary>
    public class EventHubSender 
    {
        #region Private Variables
        private readonly EventHubClient _eventHubClient;
        #endregion

        #region Construcor
        public EventHubSender()
        {
            // create Event Hub
            var eventHubConnectionString = CloudConfiguration.EventHubConnString;

            var manager = NamespaceManager.CreateFromConnectionString(eventHubConnectionString);
            manager.CreateEventHubIfNotExistsAsync(CloudConfiguration.EventHubName);

            _eventHubClient = EventHubClient.CreateFromConnectionString(eventHubConnectionString, CloudConfiguration.EventHubName);
        }

        #endregion



        #region - Public Methods -

        public void SendInfo(Sentiments sentiments)
        {
            try
            {
                var jsonData = JsonConvert.SerializeObject(sentiments);
                Console.WriteLine("{0} > Sending message to Event Hub: {1}", DateTime.Now, jsonData);
                _eventHubClient.Send(new EventData(Encoding.UTF8.GetBytes(jsonData)));
            }
            catch (Exception exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0} > Exception: {1}", DateTime.Now, exception.Message);
                Console.ResetColor();
            }
        }

        public void DisposeSender()
        {
            _eventHubClient.Close();
        }

        #endregion
    }
}
