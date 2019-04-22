using System;
using System.Collections.Generic;
using System.IO;
using DataGenerator.Models;
using DataGenerator.Senders;
using Microsoft.Azure.WebJobs;
using System.Threading;
using System.Text;
using System.Globalization;

namespace DataGenerator
{
    public class Program
    {
        #region Private variables 
        private static TextWriter _log;
        private static int _eventCount;
        #endregion

        public static void Main()
        {
            var host = new JobHost();
            host.Call(typeof(Program).GetMethod("SendData"));
        }

        [NoAutomaticTrigger]
        public static void SendData(TextWriter log)
        {
            var eventHubSender = new EventHubSender();
            _log = log;

            try
            {
                int noiseOccurence = int.Parse(CloudConfiguration.NoiseSentiments);
                int modifiableOccurence = int.Parse(CloudConfiguration.ModifiableSentiments);
                int noiseCount = 1;
                int modifiableCount = 1;

                List<Sentiments> noise = GetSentiments("Noise");
                List<Sentiments> modifiable = GetSentiments("Modifiable");

                while (true)
                {
                    if (noiseCount == 0)
                    {
                        noise = GetSentiments("Noise");
                    }

                    if (modifiableCount == 0)
                    {
                        modifiable = GetSentiments("Modifiable");
                    }

                    Sentiments sentiment;
                    for (var i = 0; i < noiseOccurence; i++)
                    {
                        sentiment = GetRandomSentiments(noise);
                        noise.Remove(sentiment);
                        noiseCount = noise.Count;
                        Console.WriteLine("Data from Noise file > Date: {0} Hashtag: {1} Handles: {2} Tweets: {3}",
                            sentiment.Date, sentiment.Hashtag, sentiment.Handles, sentiment.Tweets);
                        eventHubSender.SendInfo(sentiment);
                        ++_eventCount;
                    }

                    for (var i = 0; i < modifiableOccurence; i++) 
                    {
                        sentiment = GetRandomSentiments(modifiable);
                        modifiable.Remove(sentiment);
                        modifiableCount = modifiable.Count;
                        Console.WriteLine("Data from Modifiable file > Date: {0} Hashtag: {1} Handles: {2} Tweets: {3}",sentiment.Date, sentiment.Hashtag, sentiment.Handles, sentiment.Tweets);
                        eventHubSender.SendInfo(sentiment);
                        ++_eventCount;
                    }

                    _log.WriteLine("Sending event hub data");

                    //checking number of events sent to the event hub
                    Timer timer = new Timer(Callback, null, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _log.WriteLine(ex.Message);
            }
            finally
            {
                eventHubSender.DisposeSender();
            }
        }

        private static List<Sentiments> GetSentiments(string sentimentType)
        {
            List<Sentiments> sentiments = new List<Sentiments>();

            switch (sentimentType)
            {
                case "Noise":
                    using (var reader = new StreamReader(@"Tweets/Noise.csv", Encoding.UTF8))
                    {
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            if (line != null)
                            {
                                string[] values = line.Split(',');

                                Sentiments data = new Sentiments
                                {
                                    Date = DateTime.ParseExact(values[0], "ddd MMM d HH:mm:ss UTC yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None),
                                    Hashtag = values[1],
                                    Handles = values[2],
                                    Tweets = values[3]
                                };

                                sentiments.Add(data);
                            }
                        }
                    }
                    break;
                case "Modifiable":
                    using (var reader = new StreamReader(@"Tweets/Modifiable.csv", Encoding.UTF8))
                    {
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            if (line != null)
                            {
                                string[] values = line.Split(',');

                                Sentiments data = new Sentiments
                                {
                                    Date = DateTime.ParseExact(values[0], "ddd MMM d HH:mm:ss UTC yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None),
                                    Hashtag = values[1],
                                    Handles = values[2],
                                    Tweets = values[3]
                                };

                                sentiments.Add(data);
                            }
                        }
                    }
                    break;
            }
            return sentiments;
        }

        private static Sentiments GetRandomSentiments(List<Sentiments> sentimentList)
        {
            int timer = int.Parse(CloudConfiguration.Timer);

            Random rnd = new Random();

            Sentiments objSentiments = sentimentList[rnd.Next(0, sentimentList.Count)];

            //Sleep is added to reduce the time of data generation
            //the timer config is taken from app config in seconds and is multiplied by 1000 to convert to miliseconds
            Thread.Sleep(timer * 1000);

            return objSentiments;
        }

        private static void Callback(object state)
        {
            if (_eventCount != 0)
            {
                _log.WriteLine("Number of events sent as at " + DateTime.UtcNow + " is: " + _eventCount);
            }
        }

    }
}
